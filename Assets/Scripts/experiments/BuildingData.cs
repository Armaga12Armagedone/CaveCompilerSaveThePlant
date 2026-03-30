using UnityEngine;
using System;

[CreateAssetMenu(fileName = "BuildingDatas", menuName = "Scriptable Objects/BuildingData")]
public class BuildingDatas : ScriptableObject
{
    public int buildingID;
    public string buildingName;
    public Vector2Int size = Vector2Int.one;   
    public GameObject prefab;
    public int maxItems = 100;   
    public Vector2 pivotOffset = new Vector2(0.5f, 0.5f);

    public bool rotatable = true;

    public ItemInput[] needItems;   
    public ItemData outputItem; 
    public int outputAmount;
    public float productionTime = 1f;
    public ResorceCost[] buildCost;

    public int maxHealth = 100;

    public bool Destroyable = true;
    public bool attackable = true;
    public bool hasCollider = true;

    public enum BuildingType
    {
        Producer,
        Conveyor,
        Storage,
        PowerPlant
    }

    public BuildingType buildingType = BuildingType.Producer;

    }

[Serializable]
public class ItemInput
{
    public ItemData item;
    public int amount;
}

[Serializable]
public class ResorceCost
{
    public ItemData item;
    public int amount;
}