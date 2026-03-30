// public abstract class BuildingModule
// {
//     protected BuildingObject owner;

//     public BuildingModule(BuildingObject owner)
//     {
//         this.owner = owner;
//     }

//     public virtual void OnUpdate() { }
//     public virtual bool TryGiveItem(out ItemData item) { item = null; return false; }
//     public virtual bool TryTakeItem(ItemData item) { return false; }
//     public virtual bool CanAcceptItem(ItemData item) { return false; }
//     public virtual Dictionary<ItemData, int> GetInventory() => new Dictionary<ItemData, int>();
// }

using System.Collections.Generic;

public interface IBuildingModule
{
    void OnUpdate();                                             // вызывается каждый кадр
    bool TryGiveItem(out ItemData item);                         // попытка отдать предмет
    bool TryTakeItem(ItemData item);                             // попытка принять предмет
    bool CanAcceptItem(ItemData item);                           // может ли принять
    Dictionary<ItemData, int> GetInventory();                    // получить инвентарь
}