using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDatabase", menuName = "Database/GameDatabase")]
public class GameDatabase : ScriptableObject
{
    public static GameDatabase Instance;

    public List<BuildingDatas> allBuildings;
    public List<ItemData> allItems;
    public List<OreData> allOres;

    private Dictionary<int, BuildingDatas> buildingById;
    private Dictionary<int, ItemData> itemById;
    private Dictionary<int, OreData> oreById;

    public void Initialize()
    {
        buildingById = new Dictionary<int, BuildingDatas>();
        foreach (var b in allBuildings)
            buildingById[b.buildingID] = b;

        itemById = new Dictionary<int, ItemData>();
        foreach (var i in allItems)
            itemById[i.id] = i;

        oreById = new Dictionary<int, OreData>();
        foreach (var o in allOres)
            oreById[o.id] = o;
    }

    public BuildingDatas GetBuilding(int id) => buildingById.GetValueOrDefault(id);
    public ItemData GetItem(int id) => itemById.GetValueOrDefault(id);
    public OreData GetOre(int id) => oreById.GetValueOrDefault(id);
}

