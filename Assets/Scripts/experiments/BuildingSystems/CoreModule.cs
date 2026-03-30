using UnityEngine;
using System.Collections.Generic;

public class CoreModule : MonoBehaviour, IBuildingModule
{
    private BuildingObject building;

    void Start()
    {
        building = GetComponent<BuildingObject>();
        building.SelfInit();
    }

    public BuildingObject GetBuildingObject() {
        return building;
    }

    public void OnUpdate() { } // не требует обновления

    public bool TryGiveItem(out ItemData item)
    {
        // Ядро не отдаёт предметы (все ресурсы глобальные)
        item = null;
        return false;
    }

    public bool TryTakeItem(ItemData item)
    {
        // Принимаем предмет и передаём в глобальные ресурсы
        Debug.Log("Core Try TAke");
        if (CanAcceptItem(item))
        {
            ItemDatabase.Instance.AddResource(item, 1);
            // Не добавляем в локальный инвентарь
            return true;
        }
        return false;
    }

    public bool CanAcceptItem(ItemData item)
    {
        // Ядро может принимать любые предметы
        return true;
    }

    public Dictionary<ItemData, int> GetInventory()
    {
        // Показываем глобальные ресурсы, а не локальные
        return ItemDatabase.Instance.GetAllResources();
    }
}