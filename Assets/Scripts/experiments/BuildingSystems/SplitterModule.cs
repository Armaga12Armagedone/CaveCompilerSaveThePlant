using UnityEngine;
using System.Collections.Generic;

public class SplitterModule : MonoBehaviour, IBuildingModule
{
    private BuildingObject building;
    private Queue<ItemData> buffer = new Queue<ItemData>();
    public int bufferSize = 5;
    private int outputIndex = 0; // 0 – левый выход, 1 – правый

    void Start()
    {
        building = GetComponent<BuildingObject>();
    }

    public void OnUpdate()
    {
        if (building == null) return;

        // 1. Забрать предмет сзади (вход)
        if (buffer.Count < bufferSize)
        {
            Vector2Int backCell = GetCellInDirection(GetBackwardDirection());
            BuildingObject source = GridManager.Instance.GetBlock(backCell.x, backCell.y);
            if (source != null && source.TryGiveItem(out ItemData item))
            {
                buffer.Enqueue(item);
                Debug.Log($"[Splitter] {building.name} получил {item.displayName}");
            }
        }

        // 2. Отдать предмет на выход (попеременно)
        if (buffer.Count > 0)
        {
            ItemData item = buffer.Peek();
            // Получаем два направления: лево и право
            Vector2Int leftDir = GetLeftDirection();
            Vector2Int rightDir = GetRightDirection();

            // Выбираем следующее направление
            Vector2Int outputDir = (outputIndex == 0) ? leftDir : rightDir;
            Vector2Int outputCell = GetCellInDirection(outputDir);
            BuildingObject target = GridManager.Instance.GetBlock(outputCell.x, outputCell.y);

            if (target != null && target.CanAcceptItem(item) && target.TryTakeItem(item))
            {
                buffer.Dequeue();
                outputIndex = (outputIndex + 1) % 2; // переключаем выход
                Debug.Log($"[Splitter] {building.name} передал {item.displayName} в направление {outputDir}");
            }
        }
    }

    // Вспомогательные методы, использующие поворот здания
    private Vector2Int GetCellInDirection(Vector2Int dir)
    {
        return new Vector2Int((int)building.zeroPos.x, (int)building.zeroPos.y) + dir;
    }

    private Vector2Int GetBackwardDirection()
    {
        int rot = building.rotation;
        switch (rot)
        {
            case 0: return Vector2Int.down;
            case 1: return Vector2Int.left;
            case 2: return Vector2Int.up;
            case 3: return Vector2Int.right;
            default: return Vector2Int.down;
        }
    }

    private Vector2Int GetLeftDirection()
    {
        int rot = building.rotation;
        switch (rot)
        {
            case 0: return Vector2Int.left;
            case 1: return Vector2Int.up;
            case 2: return Vector2Int.right;
            case 3: return Vector2Int.down;
            default: return Vector2Int.left;
        }
    }

    private Vector2Int GetRightDirection()
    {
        int rot = building.rotation;
        switch (rot)
        {
            case 0: return Vector2Int.right;
            case 1: return Vector2Int.down;
            case 2: return Vector2Int.left;
            case 3: return Vector2Int.up;
            default: return Vector2Int.right;
        }
    }

    // Методы интерфейса
    public bool TryGiveItem(out ItemData item)
    {
        if (buffer.Count > 0)
        {
            item = buffer.Peek();
            return true;
        }
        item = null;
        return false;
    }

    public bool TryTakeItem(ItemData item)
    {
        if (buffer.Count < bufferSize)
        {
            buffer.Enqueue(item);
            return true;
        }
        return false;
    }

    public bool CanAcceptItem(ItemData item) => buffer.Count < bufferSize;
    public Dictionary<ItemData, int> GetInventory() => new Dictionary<ItemData, int>();
}