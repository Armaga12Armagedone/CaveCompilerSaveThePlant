using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width, height;
    public float cellSize = 1f;

    private BuildingObject[,] grid;

    [Header("Ores")]
    public OreData[] allOres;                private OreData[,] oreGrid;

    public Material worldMaterial;

    private Texture2D tileDataTexture;

    private Dictionary<OreData, int> oreToID;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        oreToID = new Dictionary<OreData, int>();
        for (int i = 0; i < allOres.Length; i++)
        {
            if (allOres[i] != null)
                oreToID[allOres[i]] = i + 1;         }
    
                grid = new BuildingObject[width, height];
        oreGrid = new OreData[width, height];
        grid = new BuildingObject[width, height];

                if (allOres.Length > 0)
        {
            oreGrid[5, 5] = allOres[0];             if (allOres.Length > 1)
                oreGrid[7, 8] = allOres[1];         }

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.transform.position = transform.position + new Vector3(width * cellSize / 2, height * cellSize / 2, 0);
        plane.transform.localScale = new Vector3(width * cellSize, height * cellSize, 1);

                Shader shader = Shader.Find("Custom/TilemapShader");
        if (shader == null)
        {
            Debug.LogError("Shader 'Custom/TilemapShader' not found!");
            return;
        }
        Material mat = new Material(shader);
        plane.GetComponent<Renderer>().sharedMaterial = mat;
        worldMaterial = mat; 
                plane.GetComponent<Renderer>().sortingOrder = -1;

                UpdateTileTexture();

                if (worldMaterial.HasProperty("_TileData"))
        {
            worldMaterial.SetTexture("_TileData", tileDataTexture);
            Debug.Log("Texture set to material: " + tileDataTexture.name);
        }
        else
        {
            Debug.LogError("Material does not have _TileData property!");
        }

                Debug.Log($"Color at (5,5): {tileDataTexture.GetPixel(5, 5)}");
        Debug.Log($"Texture in material: {worldMaterial.GetTexture("_TileData")?.name}");
    }

    public void RegisterBuilding(Vector2Int cell, BuildingObject building, int rotation)
    {
        Vector2Int size = GetRotatedSize(building.data.size, rotation);
        for (int ix = 0; ix < size.x; ix++)
            for (int iy = 0; iy < size.y; iy++)
            {
                Vector2Int pos = cell + new Vector2Int(ix, iy);
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                    grid[pos.x, pos.y] = building;
            }

                if (building.GetComponent<CoreModule>() != null)
            corePos = cell;
    }

    public Vector2Int GetWalkableTargetNearCore()
    {
        Vector2Int coreCell = GetCorePosition();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector2Int neighbor = coreCell + dir;
            if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height)
                if (IsWalkable(neighbor.x, neighbor.y))
                    return neighbor;
        }
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(coreCell);
        visited.Add(coreCell);
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (var dir in dirs)
            {
                Vector2Int next = current + dir;
                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height) continue;
                if (visited.Contains(next)) continue;
                if (IsWalkable(next.x, next.y))
                    return next;
                visited.Add(next);
                queue.Enqueue(next);
            }
        }
        return coreCell;     }

    public void UpdateTileTexture()
    {
        if (tileDataTexture == null)
        {
            tileDataTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tileDataTexture.filterMode = FilterMode.Point;
            tileDataTexture.wrapMode = TextureWrapMode.Clamp;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                OreData ore = oreGrid[x, y];
                int id = (ore != null && oreToID.TryGetValue(ore, out int val)) ? val : 0;
                tileDataTexture.SetPixel(x, y, GetColorForTile(id));
            }
        }
        tileDataTexture.Apply();
    }

    public void ForceSetBlock(Vector2Int cell, BuildingObject building, int rotation)
    {
        Vector2Int size = GetRotatedSize(building.data.size, rotation);
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = cell + new Vector2Int(x, y);
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                    grid[pos.x, pos.y] = building;
            }
    }

    private Color GetColorForTile(int id)
    {
        if (id == 0) return Color.black;
        if (id-1 < allOres.Length && allOres[id-1] != null)
            return allOres[id-1].oreColor;
        return Color.magenta;
    }

    public void SetOre(int x, int y, OreData ore)
    {
        oreGrid[x, y] = ore;
        UpdateTileTexture();     }

    public OreData GetOre(int x, int y) => oreGrid[x, y];

        
    public BuildingObject GetBlock(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return grid[x, y];
    }

    public bool SetBlock(Vector2Int cell, BuildingObject building, int rotation)
    {
        if (!CanPlace(building.data, cell, rotation)) return false;

        Vector2Int size = GetRotatedSize(building.data.size, rotation);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = cell + new Vector2Int(x, y);
                grid[pos.x, pos.y] = building;
            }
        }
        return true;
    }

    public Vector2 GlobalToGrid(Vector2 globalCoords)
    {
        Vector2 localPos = globalCoords - (Vector2)transform.position;
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);
        return new Vector2(x, y);
    }

    public Vector2 GridToGlobal(Vector2 cell)
    {
        return (Vector2)transform.position + new Vector2(
            cell.x * cellSize + cellSize * 0.5f,
            cell.y * cellSize + cellSize * 0.5f
        );
    }

    public bool InBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    public bool CanPlace(BuildingDatas data, Vector2Int cell, int rotation)
    {
        Vector2Int size = GetRotatedSize(data.size, rotation);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int check = cell + new Vector2Int(x, y);
                if (!InBounds(check) || grid[check.x, check.y] != null)
                    return false;
            }
        }
        return true;
    }

    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        BuildingObject building = GetBlock(x, y);
        if (building == null) return true;
        return !building.data.hasCollider;
    }

    

                                                                                                                                    
    private Vector2Int corePos = Vector2Int.zero; 
    public Vector2Int GetCorePosition()
    {
                if (corePos != Vector2Int.zero)
            return corePos;

                CoreModule core = FindFirstObjectByType<CoreModule>();
        if (core == null)
            return Vector2Int.zero;

                BuildingObject building = core.GetComponent<BuildingObject>();
        if (building != null)
        {
                        if (building.x != 0 || building.y != 0)
            {
                corePos = new Vector2Int(building.x, building.y);
                return corePos;
            }
        }

                        Vector2 worldPos = core.transform.position;
        Vector2 cell = GlobalToGrid(worldPos);
        corePos = new Vector2Int(Mathf.FloorToInt(cell.x), Mathf.FloorToInt(cell.y));
        return corePos;
    }

    public void RemoveBlock(Vector2Int cell)
    {
        BuildingObject building = GetBlock(cell.x, cell.y);
        if (building != null && building.data.Destroyable)
        {
            Destroy(building.objectBuild);
            Vector2 origin = building.zeroPos;
            Vector2Int size = GetRotatedSize(building.data.size, building.rotation);
            for (int ix = 0; ix < size.x; ix++)
            {
                for (int iy = 0; iy < size.y; iy++)
                {
                    Vector2Int cellPos = new Vector2Int((int)origin.x, (int)origin.y) + new Vector2Int(ix, iy);
                    if (cellPos.x >= 0 && cellPos.x < width && cellPos.y >= 0 && cellPos.y < height)
                        grid[cellPos.x, cellPos.y] = null;
                }
            }
        }
    }

    public static Vector2Int GetRotatedSize(Vector2Int originalSize, int rotation)
    {
        if (rotation % 2 == 1)             return new Vector2Int(originalSize.y, originalSize.x);
        else
            return originalSize;
    }

    private void OnDrawGizmos()
    {
        if (width <= 0 || height <= 0) return;

        Vector3 startPos = transform.position;

        Gizmos.color = Color.gray;

        for (int x = 0; x <= width; x++)
        {
            Vector3 from = startPos + new Vector3(x * cellSize, 0, 0);
            Vector3 to   = startPos + new Vector3(x * cellSize, height * cellSize, 0);
            Gizmos.DrawLine(from, to);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 from = startPos + new Vector3(0, y * cellSize, 0);
            Vector3 to   = startPos + new Vector3(width * cellSize, y * cellSize, 0);
            Gizmos.DrawLine(from, to);
        }

        if (grid != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null)
                    {
                        Vector3 center = startPos + new Vector3(
                            x * cellSize + cellSize * 0.5f,
                            y * cellSize + cellSize * 0.5f,
                            0
                        );
                        Vector3 size = Vector3.one * cellSize;
                        Gizmos.DrawCube(center, size);
                    }
                }
            }
        }
    }

    public Vector3 GridToGlobalCorner(Vector2 cell)
    {
        return transform.position + new Vector3(cell.x * cellSize, cell.y * cellSize, 0);
    }
}