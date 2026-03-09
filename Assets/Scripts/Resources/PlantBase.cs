using UnityEngine;

public class Plant : Building
{
    public int health = 10;
    public int waterConsumePerTick = 5;
    public int stoneConsumePerTick = 1;
    private int noWaterTicks = 0;

    protected override void Start()
    {

        buildingType = BuildingType.Core;
        base.Start();
    }

    public override void OnTick()
    {

        if (manager.core != null)
        {
            bool waterOk = manager.core.RemoveResource(ResourceType.Water, waterConsumePerTick) == waterConsumePerTick;
            bool stoneOk = manager.core.RemoveResource(ResourceType.Stone, stoneConsumePerTick) == stoneConsumePerTick;

            if (waterOk && stoneOk)
            {
                noWaterTicks = 0;
            }
            else
            {

                if (waterOk) manager.core.AddResource(ResourceType.Water, waterConsumePerTick);
                if (stoneOk) manager.core.AddResource(ResourceType.Stone, stoneConsumePerTick);

                noWaterTicks++;
                if (noWaterTicks >= 5)
                {
                    health--;
                    noWaterTicks = 0;
                    if (health <= 0) Die();
                }
            }
        }
        else
        {

            health = 0;
            Die();
        }
    }

    private void Die()
    {
        manager.GameOver();
        Destroy(gameObject);
    }

    public override bool CanBeDestinationFor(ResourceType resource) => false;
    public override bool CanBeSourceFor(ResourceType resource) => false;
}