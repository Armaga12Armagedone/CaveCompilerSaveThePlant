// ItemDatabase.cs
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public ResourceUI resourceUI;
    public ItemData[] allItems;
    private Dictionary<int, ItemData> idToItem;

    public Dictionary<ItemData, int> globalItems;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        idToItem = new Dictionary<int, ItemData>();
        foreach (var item in allItems)
            idToItem[item.id] = item;

        globalItems = new Dictionary<ItemData, int>();
    }

    public void AddResource(ItemData item, int amount) {
        if (globalItems.ContainsKey(item))
            globalItems[item] += amount;
        else
            globalItems[item] = amount;

        Debug.Log("Added Resource");

        resourceUI.UpdateText(globalItems);
    }

    public Dictionary<ItemData, int> GetAllResources() {
        return globalItems;
    }

    public ItemData GetItem(int id) => idToItem.TryGetValue(id, out var item) ? item : null;
    public string GetName(int id) => GetItem(id)?.displayName ?? "Unknown";
}