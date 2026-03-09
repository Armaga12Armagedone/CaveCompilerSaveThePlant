using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject shopMenu;
    private bool shopActive = false;

    [Header("Drone Shop UI")]
    public TMP_Text buyDronePriceText;
    public TMP_Text speedUpgradePriceText;
    public TMP_Text capacityUpgradePriceText;
    public TMP_Text speedLevelText;
    public TMP_Text capacityLevelText;

    [Header("Building Prices UI")]
    public TMP_Text ironStoragePriceText;
    public TMP_Text stoneStoragePriceText;
    public TMP_Text waterStoragePriceText;
    public TMP_Text housingPriceText;
    public TMP_Text generatorPriceText;

    private GlobalManager gm;

    void Start()
    {
        gm = GlobalManager.Instance;
        UpdateShopUI();
    }

    public void ToggleShop()
    {
        shopActive = !shopActive;
        shopMenu.SetActive(shopActive);

        if (shopActive && gm.isPlacing)
        {
            gm.CancelPlacing();
        }

        if (shopActive) UpdateShopUI();
    }

    private void CloseShop()
    {
        if (shopActive)
        {
            shopActive = false;
            shopMenu.SetActive(false);
        }
    }

    public void OnBuyDroneClick()
    {
        gm.BuyDrone();
        UpdateShopUI();
        CloseShop();
    }

    public void OnUpgradeSpeedClick()
    {
        gm.UpgradeSpeed();
        UpdateShopUI();
        CloseShop();
    }

    public void OnUpgradeCapacityClick()
    {
        gm.UpgradeCapacity();
        UpdateShopUI();
        CloseShop();
    }

    public void OnBuyIronStorageClick()
    {
        gm.StartPlacingBuilding(BuildingType.IronStorage);
        CloseShop();
    }

    public void OnBuyStoneStorageClick()
    {
        gm.StartPlacingBuilding(BuildingType.StoneStorage);
        CloseShop();
    }

    public void OnBuyWaterStorageClick()
    {
        gm.StartPlacingBuilding(BuildingType.WaterStorage);
        CloseShop();
    }

    public void OnBuyHousingClick()
    {
        gm.StartPlacingBuilding(BuildingType.Housing);
        CloseShop();
    }

    public void OnBuyGeneratorClick()
    {
        gm.StartPlacingBuilding(BuildingType.Generator);
        CloseShop();
    }

    public void OnCancelPlacingClick()
    {
        gm.CancelPlacing();
    }


    public void OnSaveClick()
    {
        gm.SaveGame();
        CloseShop();
    }

    public void OnLoadClick()
    {
        gm.LoadGame();
        CloseShop();
    }

    public void OnDeleteClick()
    {
        gm.GameOver();
        CloseShop();
    }

    public void OnRegenerateClick()
    {
        gm.RegenerateResources();
        CloseShop();
    }

    private void UpdateShopUI()
    {

        if (buyDronePriceText != null)
            buyDronePriceText.text = $"Energy: {gm.droneBuyPriceEnergy}  Iron: {gm.droneBuyPriceIron}";

        int nextSpeedLevel = gm.droneSpeedLevel + 1;
        if (nextSpeedLevel <= 7)
        {
            int speedEnergy = gm.droneSpeedPriceEnergy * nextSpeedLevel;
            int speedIron = gm.droneSpeedPriceIron * nextSpeedLevel;
            speedUpgradePriceText.text = $"Energy: {speedEnergy}  Iron: {speedIron}";
        }
        else
            speedUpgradePriceText.text = "MAX";

        int nextCapLevel = gm.droneCapacityLevel + 1;
        if (nextCapLevel <= 10)
        {
            int capEnergy = gm.droneCapacityPriceEnergy * nextCapLevel;
            int capIron = gm.droneCapacityPriceIron * nextCapLevel;
            capacityUpgradePriceText.text = $"Energy: {capEnergy}  Iron: {capIron}";
        }
        else
            capacityUpgradePriceText.text = "MAX";

        if (speedLevelText != null)
            speedLevelText.text = $"Speed: {gm.droneSpeedLevel}/7";

        if (capacityLevelText != null)
            capacityLevelText.text = $"Capacity: {gm.droneCapacityLevel}/10";


        GlobalManager.BuildingPrice price;

        price = gm.GetPrice(BuildingType.IronStorage);
        if (ironStoragePriceText != null)
            ironStoragePriceText.text = FormatPrice(price);

        price = gm.GetPrice(BuildingType.StoneStorage);
        if (stoneStoragePriceText != null)
            stoneStoragePriceText.text = FormatPrice(price);

        price = gm.GetPrice(BuildingType.WaterStorage);
        if (waterStoragePriceText != null)
            waterStoragePriceText.text = FormatPrice(price);

        price = gm.GetPrice(BuildingType.Housing);
        if (housingPriceText != null)
            housingPriceText.text = FormatPrice(price);

        price = gm.GetPrice(BuildingType.Generator);
        if (generatorPriceText != null)
            generatorPriceText.text = FormatPrice(price);
    }

    private string FormatPrice(GlobalManager.BuildingPrice price)
    {
        string result = "";
        if (price.ironCost > 0) result += $"Iron: {price.ironCost} ";
        if (price.stoneCost > 0) result += $"Stone: {price.stoneCost} ";
        if (price.waterCost > 0) result += $"Water: {price.waterCost} ";
        if (price.energyCost > 0) result += $"Energy: {price.energyCost}";
        return result.Trim();
    }
}