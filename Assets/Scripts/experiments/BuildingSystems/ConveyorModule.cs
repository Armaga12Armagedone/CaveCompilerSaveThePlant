using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour, IBuildingModule
{
    public int bufferSize = 5;
    private BuildingObject building;
    private Queue<ItemData> buffer = new Queue<ItemData>();

    void Start()
    {
        building = GetComponent<BuildingObject>();
    }

    public void OnUpdate()
    {
        if (building == null) return;

        // 1. Забрать сзади
        if (buffer.Count < bufferSize)
        {
            Vector2Int backCell = GetCellInDirection(GetBackwardDirection());
            BuildingObject source = GridManager.Instance.GetBlock(backCell.x, backCell.y);
            if (source != null && source.TryGiveItem(out ItemData item))
            {
                buffer.Enqueue(item);
                //Debug.Log($"[Conveyor] {building.name} забрал {item.displayName}");
            }
        }

        // 2. Отдать вперёд
        //Debug.Log(buffer.Count > 0);
        if (buffer.Count > 0)
        {
            Vector2Int forwardCell = GetCellInDirection(GetForwardDirection());
            BuildingObject target = GridManager.Instance.GetBlock(forwardCell.x, forwardCell.y);
            ItemData item = buffer.Peek();
            // Debug.Log(target);
            // Debug.Log(target.CanAcceptItem(item));
            if (target != null && target.CanAcceptItem(item) && target.TryTakeItem(item))
            {
                buffer.Dequeue();
                //Debug.Log($"[Conveyor] {building.name} передал {item.displayName}");
            }
        }
    }

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

    private Vector2Int GetCellInDirection(Vector2Int dir)
    {
        return new Vector2Int((int)building.zeroPos.x, (int)building.zeroPos.y) + dir;
    }

    private Vector2Int GetForwardDirection()
    {
        return building.rotation switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.down,
            3 => Vector2Int.left,
            _ => Vector2Int.up,
        };
    }

    private Vector2Int GetBackwardDirection() => -GetForwardDirection();
}