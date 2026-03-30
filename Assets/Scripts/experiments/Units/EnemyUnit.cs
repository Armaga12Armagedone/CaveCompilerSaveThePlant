using UnityEngine;
using System.Collections.Generic;

public class EnemyUnit : MonoBehaviour
{
    public float speed = 2f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;
    public int damage = 10;

    private Vector2Int currentCell;
    private Vector2Int targetCell;
    private List<Vector2Int> path = new List<Vector2Int>();
    private float attackTimer;
    private GridManager gridManager;

    public LineRenderer attackLine;
    public float rayDuration = 0.1f;
    private float rayTimer = 0f;

    public int health = 50;
    private float recalcTimer = 0f;

    void Start()
    {
        gridManager = GridManager.Instance;
        if (gridManager == null) return;

        Vector2 cell2D = gridManager.GlobalToGrid(transform.position);
        currentCell = Vector2Int.RoundToInt(cell2D);
        targetCell = gridManager.GetWalkableTargetNearCore();

        RecalculatePath();

        if (attackLine != null)
            attackLine.enabled = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            Destroy(gameObject);
    }

    void Update()
    {
        if (gridManager == null) return;

        recalcTimer += Time.deltaTime;
        if (recalcTimer >= 1f)
        {
            recalcTimer = 0f;
            RecalculatePath();
        }

        float distToCore = Vector2.Distance(transform.position, gridManager.GridToGlobal(targetCell));
        if (distToCore <= attackRange)
        {
            AttackCore();
            return;
        }

        if (path.Count > 0)
        {
            Vector2 targetPos = gridManager.GridToGlobal(path[0]);
            Vector3 targetPos3 = new Vector3(targetPos.x, targetPos.y, 0);
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPos3, step);

            if (Vector3.Distance(transform.position, targetPos3) < 0.1f)
            {
                currentCell = path[0];
                path.RemoveAt(0);
            }

            if (path.Count == 0)
                RecalculatePath();
        }
        else
        {
            AttackNearestBuilding();
        }

        if (rayTimer > 0)
        {
            rayTimer -= Time.deltaTime;
            if (rayTimer <= 0 && attackLine != null)
                attackLine.enabled = false;
        }
    }

    void RecalculatePath()
    {
        Vector2Int newTarget = gridManager.GetWalkableTargetNearCore();
        if (newTarget != targetCell)
            targetCell = newTarget;
        path = FindPath(currentCell, targetCell);
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (!gridManager.IsWalkable(start.x, start.y) || !gridManager.IsWalkable(goal.x, goal.y))
            return new List<Vector2Int>();

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == goal)
                return ReconstructPath(cameFrom, start, goal);

            foreach (var dir in dirs)
            {
                Vector2Int neighbor = current + dir;
                if (!gridManager.IsWalkable(neighbor.x, neighbor.y)) continue;
                if (cameFrom.ContainsKey(neighbor)) continue;

                cameFrom[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }
        return new List<Vector2Int>();
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = goal;
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    void AttackNearestBuilding()
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector2Int check = currentCell + dir;
            BuildingObject b = gridManager.GetBlock(check.x, check.y);
            if (b != null && b.data.attackable && b.GetComponent<CoreModule>() == null)
            {
                AttackBuilding(b);
                return;
            }
        }
        // Если нет зданий рядом – стоим
    }

    void AttackBuilding(BuildingObject building)
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            ShowAttackRay(building.transform.position);
            building.TakeDamage(damage);
        }
    }

    void ShowAttackRay(Vector3 targetPos)
    {
        if (attackLine == null) return;
        attackLine.enabled = true;
        attackLine.SetPosition(0, transform.position);
        attackLine.SetPosition(1, targetPos);
        rayTimer = rayDuration;
    }

    void AttackCore()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            CoreModule core = FindObjectOfType<CoreModule>();
            if (core != null)
            {
                ShowAttackRay(core.GetBuildingObject().transform.position);
                core.GetBuildingObject().TakeDamage(damage);
            }
        }
    }

    void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnEnemyDestroyed();
    }
}