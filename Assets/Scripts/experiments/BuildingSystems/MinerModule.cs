using UnityEngine;
using System.Collections.Generic;

public class MinerModule : MonoBehaviour, IBuildingModule
{
    private BuildingObject building;
    private float miningTimer;
    private int activeCellCount;
    private List<OreData> activeOres = new List<OreData>();

    void Start()
    {
        building = GetComponent<BuildingObject>();
        if (building == null) return;
        RefreshActiveCells();
    }

    void RefreshActiveCells()
    {
        activeOres.Clear();
        Vector2Int origin = new Vector2Int((int)building.zeroPos.x, (int)building.zeroPos.y);
        Vector2Int size = GridManager.GetRotatedSize(building.data.size, building.rotation);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                OreData ore = GridManager.Instance.GetOre(cell.x, cell.y);
                if (ore != null)
                {
                    activeOres.Add(ore);
                }
            }
        }
        activeCellCount = activeOres.Count;
    }

    public void OnUpdate()
    {
        if (building == null) return;
        if (activeCellCount == 0) return;

        // Скорость: чем больше клеток, тем быстрее цикл (базовое время делим на количество клеток)
        float cycleTime = building.data.productionTime / activeCellCount;
        miningTimer += Time.deltaTime;
        if (miningTimer >= cycleTime)
        {
            miningTimer = 0f;
            // Добываем из каждой клетки один раз за цикл
            foreach (var ore in activeOres)
            {
                building.AddItem(ore.oreItem, ore.outputAmount);
            }
        }
    }

    // IBuildingModule методы
    public bool TryGiveItem(out ItemData item)
    {
        var inv = building.GetInventory();
        foreach (var kv in inv)
        {
            if (kv.Value > 0)
            {
                item = kv.Key;
                building.RemoveItem(item, 1);
                return true;
            }
        }
        item = null;
        return false;
    }

    public bool TryTakeItem(ItemData item) => false;
    public bool CanAcceptItem(ItemData item) => false;
    public Dictionary<ItemData, int> GetInventory() => building.GetInventory();
}