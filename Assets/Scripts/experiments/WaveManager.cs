using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("References")]
    public GridManager gridManager;
    public GameObject enemyPrefab;
    public Transform coreTransform;
    public GameObject spawnMarkerPrefab;   // префаб маркера (например, красный куб)

    [Header("Wave Settings")]
    public float timeBetweenWaves = 15f;
    public float timeBetweenSpawns = 0.5f;
    public int startingWave = 1;

    [Header("Spawn Point Settings")]
    public SpawnCorner spawnCorner = SpawnCorner.BottomLeft;
    public int offsetFromCorner = 2;        // отступ от края (в клетках)
    public float minDistanceFromCore = 15f; // минимальное расстояние от ядра

    private int currentWave;
    private int enemiesRemaining;
    private bool waveInProgress;
    private Vector2Int spawnCell;
    private GameObject spawnMarker;
    private Transform core;

    private List<GameObject> activeEnemies = new List<GameObject>();

    private float waveTimer = 0f;        // таймер до следующей волны
    private bool waitingForWave = true;

    public enum SpawnCorner
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        if (gridManager == null) gridManager = GridManager.Instance;
        if (core == null)
        {
            CoreModule coreModule = FindObjectOfType<CoreModule>();
            if (coreModule != null)
                core = coreModule.transform;
        }
        if (core == null)
        {
            Debug.LogError("WaveManager: Core not found!");
            return;
        }

        // Находим точку спавна один раз (фиксированный угол)
        if (!FindFixedSpawnPoint(out spawnCell))
        {
            Debug.LogError("Could not find valid spawn point at corner! Wave system disabled.");
            return;
        }

        // Создаём маркер
        if (spawnMarkerPrefab != null)
        {
            Vector3 worldPos = gridManager.GridToGlobal(spawnCell);
            spawnMarker = Instantiate(spawnMarkerPrefab, worldPos, Quaternion.identity);
            spawnMarker.name = "SpawnMarker";
        }

        currentWave = startingWave;
        StartCoroutine(WaveLoop());
    }

    bool FindFixedSpawnPoint(out Vector2Int spawnCell)
    {
        Vector2Int coreCell = gridManager.GetCorePosition();
        Vector2Int walkableTarget = gridManager.GetWalkableTargetNearCore();

        float maxDist = -1f;
        Vector2Int bestCell = Vector2Int.zero;
        int candidateCount = 0;
        int pathExistsCount = 0;

        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (gridManager.IsWalkable(x, y))
                {
                    candidateCount++;
                    Vector2Int candidate = new Vector2Int(x, y);
                    float dist = Vector2Int.Distance(candidate, coreCell);

                    if (PathExists(candidate, walkableTarget))
                    {
                        pathExistsCount++;
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            bestCell = candidate;
                        }
                    }
                }
            }
        }

        Debug.Log($"Total walkable cells: {candidateCount}, cells with path to core: {pathExistsCount}");

        if (maxDist > 0)
        {
            spawnCell = bestCell;
            Debug.Log($"✅ Spawn point found at {spawnCell}, distance to core = {maxDist}");
            return true;
        }

        // Fallback: любая проходимая клетка
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (gridManager.IsWalkable(x, y))
                {
                    spawnCell = new Vector2Int(x, y);
                    Debug.LogWarning($"⚠️ Fallback spawn point: {spawnCell}");
                    return true;
                }
            }
        }

        spawnCell = Vector2Int.zero;
        Debug.LogError("❌ No walkable cells on the map!");
        return false;
    }

    bool IsValidSpawnCell(Vector2Int cell, Vector2Int coreCell)
    {
        return gridManager.IsWalkable(cell.x, cell.y) &&
            Vector2Int.Distance(cell, coreCell) >= minDistanceFromCore &&
            PathExists(cell, coreCell);
    }

    bool IsCellValidForSpawn(Vector2Int cell)
    {
        if (!gridManager.IsWalkable(cell.x, cell.y)) return false;

        Vector2 corePos = gridManager.GlobalToGrid(core.position);
        Vector2Int coreCell = new Vector2Int(Mathf.RoundToInt(corePos.x), Mathf.RoundToInt(corePos.y));
        if (Vector2Int.Distance(cell, coreCell) < minDistanceFromCore) return false;

        return PathExists(cell, coreCell);
    }

    bool PathExists(Vector2Int start, Vector2Int goal)
    {
        // BFS
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == goal) return true;

            foreach (var dir in dirs)
            {
                Vector2Int next = current + dir;
                if (next.x < 0 || next.x >= gridManager.width || next.y < 0 || next.y >= gridManager.height)
                    continue;
                if (!gridManager.IsWalkable(next.x, next.y)) continue;
                if (visited.Contains(next)) continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }
        return false;
    }

     IEnumerator WaveLoop()
    {
        while (true)
        {
            waitingForWave = true;
            waveTimer = timeBetweenWaves;
            while (waveTimer > 0)
            {
                waveTimer -= Time.deltaTime;
                yield return null;
            }
            waitingForWave = false;
            if (!waveInProgress)
                StartCoroutine(RunWave());
        }
    }

    IEnumerator RunWave()
    {
        waveInProgress = true;
        int enemyCount = CalculateEnemyCount(currentWave);
        Debug.Log($"Wave {currentWave} starting! Enemies: {enemyCount}");

        Vector3 spawnWorldPos = gridManager.GridToGlobal(spawnCell);
        enemiesRemaining = enemyCount;

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnWorldPos, Quaternion.identity);
            activeEnemies.Add(enemy);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        while (enemiesRemaining > 0)
            yield return null;

        activeEnemies.RemoveAll(e => e == null);
        waveInProgress = false;
        currentWave++;
    }

    int CalculateEnemyCount(int wave)
    {
        if (wave <= 5)
            return 2 + wave;
        else if (wave <= 10)
            return 7 + (wave - 5) * 2;
        else
            return 17 + Random.Range(1, 3) * (wave - 10);
    }

    public void OnEnemyDestroyed()
    {
        enemiesRemaining--;
    }

    public int GetCurrentWave() => currentWave;
    public float GetTimeToNextWave() => waitingForWave ? waveTimer : 0f;
    public int GetNextWaveEnemyCount() => CalculateEnemyCount(currentWave);
}