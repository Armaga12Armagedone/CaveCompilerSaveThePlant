using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text ironText;
    [SerializeField] private TMP_Text waterText;
    [SerializeField] private TMP_Text stoneText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text dronesText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text errorMessageText;
    [SerializeField] private float errorDisplayTime = 2f;
    [SerializeField] private UIManager uiManager;

    [Header("Settings")]
    public float tickInterval = 1f;

    public List<Building> buildings = new List<Building>();
    public Plant plant;
    public Building core;

    public int maxDrones = 1;
    public int drones = 0;

    private float timer;

    [Header("Drone Prices")]
    public int droneBuyPriceEnergy = 10;
    public int droneBuyPriceIron = 10;
    public int droneSpeedPriceEnergy = 10;
    public int droneSpeedPriceIron = 5;
    public int droneCapacityPriceEnergy = 15;
    public int droneCapacityPriceIron = 10;

    public int droneSpeedLevel = 0;
    public int droneCapacityLevel = 0;
    public int droneSpeed = 2;
    public int droneCapacity = 1;

    public GameObject dronePrefab;
    public Transform coreSpawnPoint;

    [Header("Building Placement")]
    public bool isPlacing = false;
    private BuildingType placingType;
    private GameObject buildingGhost;
    public Material ghostValidMaterial;
    public Material ghostInvalidMaterial;
    public LayerMask buildingLayer;
    public LayerMask resourceLayer;
    public Vector2 mapMinBounds;
    public Vector2 mapMaxBounds;

    [System.Serializable]
    public struct BuildingPrice
    {
        public BuildingType type;
        public int ironCost;
        public int stoneCost;
        public int waterCost;
        public int energyCost;
    }
    public BuildingPrice[] buildingPrices;

    public GameObject ironStoragePrefab;
    public GameObject stoneStoragePrefab;
    public GameObject waterStoragePrefab;
    public GameObject housingPrefab;
    public GameObject generatorPrefab;

    [Header("Resources Regeneration")]
    public int regenPriceEnergy = 50;
    public int regenPriceIron = 20;
    public GameObject[] resourcePrefabs;
    public int resourceCountPerType = 5;
    public Vector2 resourceSpawnAreaMin;
    public Vector2 resourceSpawnAreaMax;

    [Header("Auto Save")]
    public bool autoSaveEnabled = true;
    public float autoSaveInterval = 30f;
    private float autoSaveTimer;

    private Coroutine errorCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    void Start()
    {

        if (File.Exists(GetSavePath()))
        {
            LoadGame();
        }
        autoSaveTimer = autoSaveInterval;
    }

    void Update()
    {
        timer += Time.deltaTime;
        while (timer >= tickInterval)
        {
            timer -= tickInterval;
            Tick();
        }


        if (autoSaveEnabled)
        {
            autoSaveTimer -= Time.deltaTime;
            if (autoSaveTimer <= 0f)
            {
                SaveGame();
                autoSaveTimer = autoSaveInterval;
            }
        }
    }

    void Tick()
    {
        foreach (var building in buildings)
            building.OnTick();
        UpdateUI();
    }

    public void RegisterBuilding(Building building)
    {
        if (!buildings.Contains(building))
            buildings.Add(building);

        if (building is Plant) plant = building as Plant;
        if (building.buildingType == BuildingType.Core) core = building;
    }

    public void UnregisterBuilding(Building building)
    {
        buildings.Remove(building);
        if (plant == building) plant = null;
        if (core == building) core = null;
    }

    public int GetTotalResource(ResourceType type)
    {
        int total = 0;
        foreach (var b in buildings)
            total += b.GetResource(type);
        return total;
    }

    public int GetTotalMaxResource(ResourceType type)
    {
        int total = 0;
        foreach (var b in buildings)
            total += b.GetMax(type);
        return total;
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        int remaining = amount;
        foreach (var b in buildings)
        {
            if (remaining <= 0) break;
            int available = b.GetResource(type);
            if (available > 0)
            {
                int take = Mathf.Min(available, remaining);
                b.RemoveResource(type, take);
                remaining -= take;
            }
        }
        return remaining == 0;
    }

    public void UpdateUI()
    {
        int totalIron = GetTotalResource(ResourceType.Iron);
        int totalStone = GetTotalResource(ResourceType.Stone);
        int totalWater = GetTotalResource(ResourceType.Water);
        int totalEnergy = GetTotalResource(ResourceType.Energy);

        int maxIron = GetTotalMaxResource(ResourceType.Iron);
        int maxStone = GetTotalMaxResource(ResourceType.Stone);
        int maxWater = GetTotalMaxResource(ResourceType.Water);
        int maxEnergy = GetTotalMaxResource(ResourceType.Energy);

        if (ironText != null) ironText.text = $"Iron: {totalIron}/{maxIron}";
        if (stoneText != null) stoneText.text = $"Stone: {totalStone}/{maxStone}";
        if (waterText != null) waterText.text = $"Water: {totalWater}/{maxWater}";
        if (energyText != null) energyText.text = $"Energy: {totalEnergy}/{maxEnergy}";
        if (dronesText != null) dronesText.text = $"Drones: {drones}/{maxDrones}";

        if (healthText != null)
        {
            if (plant != null)
                healthText.text = $"Plant Health: {plant.health}";
            else
                healthText.text = "Plant Health: Dead";
        }
    }

    public void ShowError(string message)
    {
        if (errorMessageText == null) return;

        if (errorCoroutine != null)
            StopCoroutine(errorCoroutine);

        errorCoroutine = StartCoroutine(DisplayErrorMessage(message));
    }

    private IEnumerator DisplayErrorMessage(string message)
    {
        errorMessageText.text = message;
        errorMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(errorDisplayTime);
        errorMessageText.gameObject.SetActive(false);
        errorCoroutine = null;
    }

    public void CollectResourceByClick(GameObject resourceObject)
    {
        Resource res = resourceObject.GetComponent<Resource>();
        if (res == null || res.resourceCount <= 0) return;

        if (core != null)
        {
            int added = core.AddResource(res.resourceType, 1);
            if (added > 0)
            {
                res.resourceCount--;
                if (res.resourceCount <= 0)
                    Destroy(resourceObject);
            }
        }
        UpdateUI();
    }

    public void GenerateEnergyClick()
    {
        if (core != null)
        {
            core.AddResource(ResourceType.Energy, 1);
            UpdateUI();
        }
    }

    public void BuyDrone()
    {
        if (drones >= maxDrones)
        {
            ShowError("Достигнут лимит дронов");
            return;
        }

        if (GetTotalResource(ResourceType.Energy) < droneBuyPriceEnergy)
        {
            ShowError($"Недостаточно энергии! Нужно {droneBuyPriceEnergy}");
            return;
        }
        if (GetTotalResource(ResourceType.Iron) < droneBuyPriceIron)
        {
            ShowError($"Недостаточно железа! Нужно {droneBuyPriceIron}");
            return;
        }

        SpendResource(ResourceType.Energy, droneBuyPriceEnergy);
        SpendResource(ResourceType.Iron, droneBuyPriceIron);

        drones++;

        if (dronePrefab != null && coreSpawnPoint != null)
        {
            Vector2 spawnPos = (Vector2)coreSpawnPoint.position + Random.insideUnitCircle * 2f;
            GameObject newDrone = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
            DroneBase droneComp = newDrone.GetComponent<DroneBase>();
            if (droneComp != null)
            {
                droneComp.speed = droneSpeed;
                droneComp.carryCapacity = droneCapacity;
            }
        }

        UpdateUI();
    }

    public void UpgradeSpeed()
    {
        int nextLevel = droneSpeedLevel + 1;
        if (nextLevel > 7)
        {
            ShowError("Максимальный уровень скорости достигнут");
            return;
        }

        int priceEnergy = droneSpeedPriceEnergy * nextLevel;
        int priceIron = droneSpeedPriceIron * nextLevel;

        if (GetTotalResource(ResourceType.Energy) < priceEnergy)
        {
            ShowError($"Недостаточно энергии! Нужно {priceEnergy}");
            return;
        }
        if (GetTotalResource(ResourceType.Iron) < priceIron)
        {
            ShowError($"Недостаточно железа! Нужно {priceIron}");
            return;
        }

        SpendResource(ResourceType.Energy, priceEnergy);
        SpendResource(ResourceType.Iron, priceIron);

        droneSpeedLevel = nextLevel;
        droneSpeed = 2 + droneSpeedLevel;

        var allDrones = FindObjectsOfType<DroneBase>();
        foreach (var d in allDrones)
            d.speed = droneSpeed;

        UpdateUI();
    }

    public void UpgradeCapacity()
    {
        int nextLevel = droneCapacityLevel + 1;
        if (nextLevel > 10)
        {
            ShowError("Максимальная грузоподъёмность достигнута");
            return;
        }

        int priceEnergy = droneCapacityPriceEnergy * nextLevel;
        int priceIron = droneCapacityPriceIron * nextLevel;

        if (GetTotalResource(ResourceType.Energy) < priceEnergy)
        {
            ShowError($"Недостаточно энергии! Нужно {priceEnergy}");
            return;
        }
        if (GetTotalResource(ResourceType.Iron) < priceIron)
        {
            ShowError($"Недостаточно железа! Нужно {priceIron}");
            return;
        }

        SpendResource(ResourceType.Energy, priceEnergy);
        SpendResource(ResourceType.Iron, priceIron);

        droneCapacityLevel = nextLevel;
        droneCapacity = 1 + droneCapacityLevel;

        var allDrones = FindObjectsOfType<DroneBase>();
        foreach (var d in allDrones)
            d.carryCapacity = droneCapacity;

        UpdateUI();
    }


    public void StartPlacingBuilding(BuildingType type)
    {
        BuildingPrice price = GetPrice(type);

        string missing = "";
        if (GetTotalResource(ResourceType.Iron) < price.ironCost)
            missing += $"Iron ({price.ironCost}) ";
        if (GetTotalResource(ResourceType.Stone) < price.stoneCost)
            missing += $"Stone ({price.stoneCost}) ";
        if (GetTotalResource(ResourceType.Water) < price.waterCost)
            missing += $"Water ({price.waterCost}) ";
        if (GetTotalResource(ResourceType.Energy) < price.energyCost)
            missing += $"Energy ({price.energyCost}) ";

        if (!string.IsNullOrEmpty(missing))
        {
            ShowError($"Недостаточно ресурсов: {missing}");
            return;
        }

        if (isPlacing) CancelPlacing();

        isPlacing = true;
        placingType = type;

        if (buildingGhost == null)
        {
            GameObject prefab = GetBuildingPrefab(type);
            buildingGhost = Instantiate(prefab);
            Collider2D col = buildingGhost.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            SetGhostMaterial(ghostValidMaterial);
        }
    }

    public BuildingPrice GetPrice(BuildingType type)
    {
        foreach (var p in buildingPrices)
            if (p.type == type) return p;
        return new BuildingPrice();
    }

    private GameObject GetBuildingPrefab(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.IronStorage: return ironStoragePrefab;
            case BuildingType.StoneStorage: return stoneStoragePrefab;
            case BuildingType.WaterStorage: return waterStoragePrefab;
            case BuildingType.Housing: return housingPrefab;
            case BuildingType.Generator: return generatorPrefab;
            default: return null;
        }
    }

    public void HandlePlacementInput(Vector2 screenPos)
    {
        if (!isPlacing) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        if (buildingGhost != null)
            buildingGhost.transform.position = worldPos;

        bool valid = IsValidPlacement(worldPos);
        SetGhostMaterial(valid ? ghostValidMaterial : ghostInvalidMaterial);

        if (GetPlacementConfirm())
        {
            if (valid)
            {
                BuildingPrice price = GetPrice(placingType);
                SpendResources(price);
                GameObject newBuilding = Instantiate(GetBuildingPrefab(placingType), worldPos, Quaternion.identity);
                CancelPlacing();
            }
            else
            {
                ShowError("Нельзя построить здесь");
            }
        }
        else if (GetPlacementCancel())
        {
            CancelPlacing();
        }
    }

    private void SpendResources(BuildingPrice price)
    {
        SpendResource(ResourceType.Iron, price.ironCost);
        SpendResource(ResourceType.Stone, price.stoneCost);
        SpendResource(ResourceType.Water, price.waterCost);
        SpendResource(ResourceType.Energy, price.energyCost);
    }

    private bool IsValidPlacement(Vector2 pos)
    {
        if (pos.x < mapMinBounds.x || pos.x > mapMaxBounds.x || pos.y < mapMinBounds.y || pos.y > mapMaxBounds.y)
            return false;

        Collider2D hit = Physics2D.OverlapCircle(pos, 0.5f, buildingLayer);
        if (hit != null) return false;

        hit = Physics2D.OverlapCircle(pos, 0.5f, resourceLayer);
        if (hit != null) return false;

        return true;
    }

    private void SetGhostMaterial(Material mat)
    {
        if (buildingGhost != null)
        {
            SpriteRenderer[] renderers = buildingGhost.GetComponentsInChildren<SpriteRenderer>();
            foreach (var r in renderers)
                r.material = mat;
        }
    }

    private bool GetPlacementConfirm()
    {
        if (Application.isMobilePlatform)
            return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0 &&
                   UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began;
        else
            return Mouse.current.leftButton.wasPressedThisFrame;
    }

    private bool GetPlacementCancel()
    {
        if (Application.isMobilePlatform)
            return false;
        else
            return Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    public void CancelPlacing()
    {
        if (buildingGhost != null) Destroy(buildingGhost);
        isPlacing = false;
    }


    private string GetSavePath()
    {
        return Application.persistentDataPath + "/save.json";
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        data.maxDrones = maxDrones;
        data.drones = drones;
        data.droneSpeedLevel = droneSpeedLevel;
        data.droneCapacityLevel = droneCapacityLevel;
        data.droneSpeed = droneSpeed;
        data.droneCapacity = droneCapacity;


        foreach (var b in buildings)
        {
            BuildingData bd = new BuildingData();
            bd.type = b.buildingType;
            bd.posX = b.transform.position.x;
            bd.posY = b.transform.position.y;
            bd.iron = b.iron;
            bd.stone = b.stone;
            bd.water = b.water;
            bd.energy = b.energy;
            if (b is Plant plant)
                bd.health = plant.health;
            data.buildings.Add(bd);
        }


        var allDrones = FindObjectsOfType<DroneBase>();
        foreach (var d in allDrones)
        {
            DroneData dd = new DroneData();
            dd.posX = d.transform.position.x;
            dd.posY = d.transform.position.y;
            dd.speed = d.speed;
            dd.carryCapacity = d.carryCapacity;
            data.dronesList.Add(dd);
        }


        var allResources = FindObjectsOfType<Resource>();
        foreach (var r in allResources)
        {
            ResourceData rd = new ResourceData();
            rd.type = r.resourceType;
            rd.posX = r.transform.position.x;
            rd.posY = r.transform.position.y;
            rd.count = r.resourceCount;
            data.resources.Add(rd);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
        ShowError("Игра сохранена");
    }

    public void LoadGame()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            ShowError("Нет сохранённой игры");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);


        maxDrones = data.maxDrones;
        drones = data.drones;
        droneSpeedLevel = data.droneSpeedLevel;
        droneCapacityLevel = data.droneCapacityLevel;
        droneSpeed = data.droneSpeed;
        droneCapacity = data.droneCapacity;


        foreach (var b in buildings)
            Destroy(b.gameObject);
        buildings.Clear();

        var oldDrones = FindObjectsOfType<DroneBase>();
        foreach (var d in oldDrones)
            Destroy(d.gameObject);

        var oldResources = FindObjectsOfType<Resource>();
        foreach (var r in oldResources)
            Destroy(r.gameObject);


        foreach (var bd in data.buildings)
        {
            GameObject prefab = GetBuildingPrefab(bd.type);
            if (prefab == null) continue;
            Vector3 pos = new Vector3(bd.posX, bd.posY, 0);
            GameObject go = Instantiate(prefab, pos, Quaternion.identity);
            Building b = go.GetComponent<Building>();
            b.iron = bd.iron;
            b.stone = bd.stone;
            b.water = bd.water;
            b.energy = bd.energy;
            if (b is Plant plant && bd.health > 0)
                plant.health = bd.health;
        }


        foreach (var dd in data.dronesList)
        {
            Vector3 pos = new Vector3(dd.posX, dd.posY, 0);
            GameObject go = Instantiate(dronePrefab, pos, Quaternion.identity);
            DroneBase d = go.GetComponent<DroneBase>();
            d.speed = dd.speed;
            d.carryCapacity = dd.carryCapacity;
        }


        foreach (var rd in data.resources)
        {
            SpawnResource(rd.type, rd.posX, rd.posY, rd.count);
        }

        UpdateUI();
        ShowError("Игра загружена");
    }

    private void SpawnResource(ResourceType type, float x, float y, int count)
    {
        if (resourcePrefabs == null || resourcePrefabs.Length == 0) return;
        GameObject prefab = null;
        switch (type)
        {
            case ResourceType.Iron: prefab = resourcePrefabs[0]; break;
            case ResourceType.Stone: prefab = resourcePrefabs[1]; break;
            case ResourceType.Water: prefab = resourcePrefabs[2]; break;
        }
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
        Resource res = go.GetComponent<Resource>();
        if (res != null)
            res.resourceCount = count;
    }


    public void RegenerateResources()
    {
        if (GetTotalResource(ResourceType.Energy) < regenPriceEnergy ||
            GetTotalResource(ResourceType.Iron) < regenPriceIron)
        {
            ShowError($"Недостаточно ресурсов! Нужно Energy: {regenPriceEnergy}, Iron: {regenPriceIron}");
            return;
        }

        SpendResource(ResourceType.Energy, regenPriceEnergy);
        SpendResource(ResourceType.Iron, regenPriceIron);


        var oldResources = FindObjectsOfType<Resource>();
        foreach (var r in oldResources)
            Destroy(r.gameObject);


        for (int i = 0; i < resourceCountPerType; i++)
        {
            for (int t = 0; t < resourcePrefabs.Length; t++)
            {
                Vector2 pos = new Vector2(
                    Random.Range(resourceSpawnAreaMin.x, resourceSpawnAreaMax.x),
                    Random.Range(resourceSpawnAreaMin.y, resourceSpawnAreaMax.y)
                );
                Instantiate(resourcePrefabs[t], pos, Quaternion.identity);
            }
        }
        ShowError("Ресурсы восстановлены");
    }


    public void GameOver()
    {
        Debug.Log("Game Over! Plant died.");


        if (File.Exists(GetSavePath()))
            File.Delete(GetSavePath());


        ShowError("Растение погибло! Игра перезапускается...");


        StopAllCoroutines();


        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}