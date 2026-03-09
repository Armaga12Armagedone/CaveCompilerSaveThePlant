using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int maxDrones;
    public int drones;
    public int droneSpeedLevel;
    public int droneCapacityLevel;
    public int droneSpeed;
    public int droneCapacity;

    public List<BuildingData> buildings = new List<BuildingData>();
    public List<DroneData> dronesList = new List<DroneData>();
    public List<ResourceData> resources = new List<ResourceData>();
}

[System.Serializable]
public class BuildingData
{
    public BuildingType type;
    public float posX, posY;
    public int iron, stone, water, energy;
    public int health;
}

[System.Serializable]
public class DroneData
{
    public float posX, posY;
    public float speed;
    public int carryCapacity;
}

[System.Serializable]
public class ResourceData
{
    public ResourceType type;
    public float posX, posY;
    public int count;
}