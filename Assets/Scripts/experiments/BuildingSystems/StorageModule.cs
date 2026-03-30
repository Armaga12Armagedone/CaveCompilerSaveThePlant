using System.Collections.Generic;
using UnityEngine;

public class StorageModule : MonoBehaviour, IBuildingModule
{
    private BuildingObject building;

    void Start()
    {
        building = GetComponent<BuildingObject>();
    }

    public void OnUpdate() {
     } // склад не требует обновления

    public bool TryGiveItem(out ItemData item)
    {
        Debug.Log("Try Give");
        var inv = building.GetInventory();
        foreach (var kv in inv)
        {
            item = kv.Key;
            building.RemoveItem(item, 1);
            return true;
        }
        item = null;
        return false;
    }

    public bool TryTakeItem(ItemData item)
    {
        Debug.Log("Try Take func");
        if (!CanAcceptItem(item)) return false;
        building.AddItem(item, 1);
        return true;
    }

    public bool CanAcceptItem(ItemData item) {
        Debug.Log("Accept func");
        return building.GetItemCount(item) < building.data.maxItems;
    }

    public Dictionary<ItemData, int> GetInventory() => building.GetInventory();
}