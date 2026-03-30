using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public GridManager grid;
    public int width = 100, height = 100;

    [Header("Wall generation (Perlin noise)")]
    [Range(0, 1)] public float wallDensity = 0.2f;      // общая плотность стен
    public float noiseScale = 10f;                      // масштаб шума (чем больше, тем мельче детали)
    public float wallThreshold = 0.5f;                  // порог для стены (чем выше, тем меньше стен)
    public GameObject wallPrefab;

    [Header("Ore clusters")]
    public OreData ironOre;
    public OreData copperOre;
    public int ironClusterCount = 8;                    // количество залежей железа
    public int copperClusterCount = 5;                  // залежей меди
    public int clusterRadius = 3;                       // радиус залежи (в клетках)
    public float clusterFillRatio = 0.6f;               // плотность заполнения залежи (0..1)

    void Start()
    {
        if (grid == null) grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }
        width = grid.width;
        height = grid.height;
        Generate();
    }

    [ContextMenu("Generate New Map")]
    public void Generate()
    {
        ClearMap();

        // 1. Стены на основе шума Перлина
        GenerateWallsWithNoise();

        // 2. Залежи руды
        GenerateOreClusters(ironOre, ironClusterCount, clusterRadius, clusterFillRatio);
        GenerateOreClusters(copperOre, copperClusterCount, clusterRadius, clusterFillRatio);

        grid.UpdateTileTexture(); // обновить визуализацию руды
    }

    void ClearMap()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid.RemoveBlock(new Vector2Int(x, y));
    }

    void GenerateWallsWithNoise()
    {
        if (wallPrefab == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Значение шума (0..1)
                float noise = Mathf.PerlinNoise(x / noiseScale, y / noiseScale);
                // Корректируем порог в зависимости от желаемой плотности
                float adjustedThreshold = wallThreshold * (1 - wallDensity) + wallDensity;
                if (noise > adjustedThreshold)
                {
                    PlaceWall(x, y);
                }
            }
        }
    }

    void GenerateOreClusters(OreData ore, int clusterCount, int radius, float fillRatio)
    {
        if (ore == null) return;

        for (int i = 0; i < clusterCount; i++)
        {
            // Выбираем случайный центр залежи (не слишком близко к краю)
            int centerX = Random.Range(radius, width - radius);
            int centerY = Random.Range(radius, height - radius);

            // Заполняем клетки внутри круга
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    if (x < 0 || x >= width || y < 0 || y >= height) continue;

                    // Круглая форма
                    float dist = Mathf.Sqrt(dx*dx + dy*dy);
                    if (dist > radius) continue;

                    // Вероятность заполнения уменьшается к краям
                    float prob = fillRatio * (1 - dist / radius);
                    if (Random.value < prob)
                    {
                        // Не перезаписываем уже существующую руду (можно и перезаписывать, но лучше не смешивать)
                        if (grid.GetOre(x, y) == null)
                            grid.SetOre(x, y, ore);
                    }
                }
            }
        }
    }

    void PlaceWall(int x, int y)
    {
        // Проверяем, не занято ли уже место стеной или другим зданием
        if (grid.GetBlock(x, y) != null) return;

        Vector3 worldPos = grid.GridToGlobal(new Vector2(x, y));
        GameObject wall = Instantiate(wallPrefab, worldPos, Quaternion.identity);
        BuildingObject building = wall.GetComponent<BuildingObject>();
        if (building != null)
            grid.ForceSetBlock(new Vector2Int(x, y), building, 0);
    }
}