using UnityEngine;
using TMPro;

public enum BuildingType
{
    Core,
    IronStorage,
    StoneStorage,
    WaterStorage,
    Housing,
    Generator
}

public class Building : MonoBehaviour
{
    public BuildingType buildingType;
    public string buildingName;
    public GameObject highlightIndicator;

    public int iron, stone, water, energy;
    public int maxIron = 10, maxStone = 10, maxWater = 100, maxEnergy = 100;

    public int energyPerTick = 5;
    public ResourceType fuelType = ResourceType.Stone;
    public int fuelPerTick = 1;

    public TMP_Text resourceDisplay;

    protected GlobalManager manager;
    private int droneSlotsAdded = 0;

    protected virtual void Start()
    {
        manager = GlobalManager.Instance;
        SetInitialLimits();
        manager.RegisterBuilding(this);
        UpdateDisplay();
        if (highlightIndicator != null)
            highlightIndicator.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        if (manager != null)
        {
            if (buildingType == BuildingType.Housing)
                manager.maxDrones -= droneSlotsAdded;
            manager.UnregisterBuilding(this);
        }
    }

    public virtual void OnTick()
    {
        switch (buildingType)
        {
            case BuildingType.Generator:
                if (fuelPerTick > 0)
                {
                    if (HasResource(fuelType, fuelPerTick))
                    {
                        RemoveResource(fuelType, fuelPerTick);
                        AddResource(ResourceType.Energy, energyPerTick);
                    }
                }
                else
                    AddResource(ResourceType.Energy, energyPerTick);
                break;
        }
        UpdateDisplay();
    }

    private void SetInitialLimits()
    {
        switch (buildingType)
        {
            case BuildingType.Core:
                maxIron = 50; maxStone = 50; maxWater = 500; maxEnergy = 200;
                break;
            case BuildingType.IronStorage:
                maxIron = 100; maxStone = 0; maxWater = 0; maxEnergy = 0;
                break;
            case BuildingType.StoneStorage:
                maxIron = 0; maxStone = 100; maxWater = 0; maxEnergy = 0;
                break;
            case BuildingType.WaterStorage:
                maxIron = 0; maxStone = 0; maxWater = 500; maxEnergy = 0;
                break;
            case BuildingType.Housing:
                droneSlotsAdded = 1;
                manager.maxDrones += droneSlotsAdded;
                break;
            case BuildingType.Generator:
                maxEnergy = 200;
                maxStone = 50;
                break;
        }
    }

    public int GetResource(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Iron: return iron;
            case ResourceType.Stone: return stone;
            case ResourceType.Water: return water;
            case ResourceType.Energy: return energy;
            default: return 0;
        }
    }

    public int GetMax(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Iron: return maxIron;
            case ResourceType.Stone: return maxStone;
            case ResourceType.Water: return maxWater;
            case ResourceType.Energy: return maxEnergy;
            default: return 0;
        }
    }

    public int AddResource(ResourceType type, int amount)
    {
        int current = GetResource(type);
        int max = GetMax(type);
        int newValue = Mathf.Clamp(current + amount, 0, max);
        int added = newValue - current;
        SetResource(type, newValue);
        UpdateDisplay();
        return added;
    }

    public int RemoveResource(ResourceType type, int amount)
    {
        int current = GetResource(type);
        int removed = Mathf.Min(current, amount);
        SetResource(type, current - removed);
        UpdateDisplay();
        return removed;
    }

    public bool HasResource(ResourceType type, int amount) => GetResource(type) >= amount;

    private void SetResource(ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Iron: iron = value; break;
            case ResourceType.Stone: stone = value; break;
            case ResourceType.Water: water = value; break;
            case ResourceType.Energy: energy = value; break;
        }
    }

    private void UpdateDisplay()
    {
        if (resourceDisplay == null) return;

        switch (buildingType)
        {
            case BuildingType.IronStorage:
                resourceDisplay.text = $"Iron: {iron}/{maxIron}";
                break;
            case BuildingType.StoneStorage:
                resourceDisplay.text = $"Stone: {stone}/{maxStone}";
                break;
            case BuildingType.WaterStorage:
                resourceDisplay.text = $"Water: {water}/{maxWater}";
                break;
            case BuildingType.Generator:
                resourceDisplay.text = $"Energy: {energy}/{maxEnergy}\nFuel: {stone}";
                break;
            case BuildingType.Core:
                resourceDisplay.text = $"I:{iron} S:{stone} W:{water} E:{energy}";
                break;
            case BuildingType.Housing:
                resourceDisplay.text = $"Drones: {manager.drones}/{manager.maxDrones}";
                break;
        }
    }

    public virtual bool CanBeSourceFor(ResourceType resource)
    {
        switch (buildingType)
        {
            case BuildingType.IronStorage: return resource == ResourceType.Iron;
            case BuildingType.StoneStorage: return resource == ResourceType.Stone;
            case BuildingType.WaterStorage: return resource == ResourceType.Water;
            case BuildingType.Core: return true;
            case BuildingType.Generator: return resource == ResourceType.Energy;
            default: return false;
        }
    }

    public virtual bool CanBeDestinationFor(ResourceType resource)
    {
        switch (buildingType)
        {
            case BuildingType.IronStorage: return resource == ResourceType.Iron;
            case BuildingType.StoneStorage: return resource == ResourceType.Stone;
            case BuildingType.WaterStorage: return resource == ResourceType.Water;
            case BuildingType.Core: return true;
            case BuildingType.Generator: return resource == ResourceType.Stone;
            case BuildingType.Housing: return false;
            default: return false;
        }
    }

    public void SetHighlight(bool highlight)
    {
        if (highlightIndicator != null)
            highlightIndicator.SetActive(highlight);
    }
}

public enum ResourceType
{
    Iron,
    Stone,
    Water,
    Energy
}