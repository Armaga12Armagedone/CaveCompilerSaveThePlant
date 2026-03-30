using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems;
using TMPro;

public class BuildSystem : MonoBehaviour
{
    private GridManager gridManager;

    [SerializeField] private GameObject building;
    [SerializeField] private BuildingDatas data;

    [SerializeField] private Color invalidPlace;
    [SerializeField] private Color validPlace;

    public bool isBuilding = false;
    [SerializeField] private GameObject preview;
    private SpriteRenderer previewRender;

    private Dictionary<int, BuildingDatas> buildingsDictionary;
    public BuildingDatas[] allBuildings;

    private int currentRotation = 0;

    private int lastBuilding = -1;

    [Header("Player Interaction")]
    public float playerAttackDamage = 20f;
    public LineRenderer attackLine;              public float attackRayDuration = 0.1f;

    private float attackRayTimer;

        [SerializeField] private TMP_Text textResources;
    [SerializeField] private GameObject panelObject;                   [SerializeField] private Vector2 panelOffset = new Vector2(80, 80); 
    private BuildingObject selectedBuilding;   
    void Start()
    {
        gridManager = GridManager.Instance;
        if (isBuilding) UpdatePreview();
        previewRender = preview.GetComponent<SpriteRenderer>();

        buildingsDictionary = new Dictionary<int, BuildingDatas>();

        foreach (var building in allBuildings)
        {
            if (!buildingsDictionary.ContainsKey(building.buildingID))
                buildingsDictionary.Add(building.buildingID, building);
            else
                Debug.LogError($"Обнаружен дубликат ID: {building.buildingID} для {building.name}");
        }
    }

    void TryMine(Vector2Int cell)
    {
        OreData ore = gridManager.GetOre(cell.x, cell.y);
        if (ore != null)
        {
                        ItemDatabase.Instance.AddResource(ore.oreItem, 1);
                                            }
    }

    void TryAttack(Vector3 worldPos)
    {
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            EnemyUnit enemy = hit.collider.GetComponent<EnemyUnit>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)playerAttackDamage);
                ShowAttackRay(worldPos, hit.point);
            }
        }
    }

    bool CanAfford(BuildingDatas data)
    {
        if (data.buildCost == null || data.buildCost.Length == 0) return true;
        var global = ItemDatabase.Instance.globalItems;
        foreach (var cost in data.buildCost)
        {
            if (!global.ContainsKey(cost.item) || global[cost.item] < cost.amount)
                return false;
        }
        return true;
    }

    void SpendResources(BuildingDatas data)
    {
        if (data.buildCost == null) return;
        var global = ItemDatabase.Instance.globalItems;
        foreach (var cost in data.buildCost)
        {
            global[cost.item] -= cost.amount;
            if (global[cost.item] <= 0)
                global.Remove(cost.item);
        }
                if (ItemDatabase.Instance.resourceUI != null)
            ItemDatabase.Instance.resourceUI.UpdateText(global);
    }

    void ShowAttackRay(Vector3 start, Vector3 end)
    {
        if (attackLine == null) return;
        attackLine.enabled = true;
        attackLine.SetPosition(0, start);
        attackLine.SetPosition(1, end);
        attackRayTimer = attackRayDuration;
    }

        Vector3 GetCenterPosition(Vector2 cellIndex, int rotation)
    {
        Vector3 corner = gridManager.GridToGlobalCorner(cellIndex);
        Vector2Int rotatedSize = GridManager.GetRotatedSize(data.size, rotation);
        float width = rotatedSize.x * gridManager.cellSize;
        float height = rotatedSize.y * gridManager.cellSize;
        return corner + new Vector3(width * 0.5f, height * 0.5f, 0);
    }

    public void SelectBuilding(int buildingID)
    {
        if (buildingsDictionary.TryGetValue(buildingID, out BuildingDatas buildingData) && lastBuilding != buildingID)
        {
            isBuilding = true;
            lastBuilding = buildingID;
            data = buildingData;
            building = buildingData.prefab;
            currentRotation = 0;
            preview.transform.rotation = Quaternion.identity;
            preview.transform.localScale = new Vector3(
                data.size.x * gridManager.cellSize,
                data.size.y * gridManager.cellSize,
                1
            );
                        if (panelObject.activeSelf)
                panelObject.SetActive(false);
            selectedBuilding = null;
            preview.SetActive(true);
        }
        else
        {
            Debug.LogError($"Постройка с ID {buildingID} не найдена!");
            if (lastBuilding == buildingID) {
                lastBuilding = -1;
                data = null;
                building = null;
                isBuilding = false;
                preview.SetActive(false);
            }
        }

        
    }

    void UpdatePreview()
    {
        preview.transform.localScale = new Vector3(data.size.x * gridManager.cellSize, data.size.y * gridManager.cellSize, 0f);
    }

        void ShowBuildingData(BuildingObject building)
    {
        selectedBuilding = building;
        UpdatePanelContent();
        UpdatePanelPosition();
        panelObject.SetActive(true);
    }

        void UpdatePanelContent()
    {
        if (selectedBuilding == null) return;
                                textResources.text = "Empty";
        var inventory = selectedBuilding.GetInventory();
         if (inventory.Count == 0)
        {
                        return;
        }

        string displayText = "";
        foreach (var kv in inventory)
        {
            var item = kv.Key;
            var count = kv.Value;

                        
            displayText = $"{item.displayName}: {count}\n";

                                                        }

        textResources.text = displayText;

    }

        void UpdatePanelPosition()
    {
        if (selectedBuilding == null || Camera.main == null) return;

                Vector2Int rotatedSize = GridManager.GetRotatedSize(selectedBuilding.data.size, selectedBuilding.rotation);
        float width = rotatedSize.x * gridManager.cellSize;
        float height = rotatedSize.y * gridManager.cellSize;
        Vector3 worldCenter = gridManager.GridToGlobalCorner(selectedBuilding.zeroPos)
                              + new Vector3(width * 0.5f, height * 0.5f, 0);

                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldCenter);

                screenPos += new Vector3(panelOffset.x, panelOffset.y, 0);

                RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        if (panelRect != null)
            panelRect.position = screenPos;
        else
            panelObject.transform.position = screenPos;
    }

    void Update()
    {
                Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        worldPos.z = 0;
        Vector2 cellIndex = gridManager.GlobalToGrid(worldPos);
        Vector2Int cellInt = new Vector2Int((int)cellIndex.x, (int)cellIndex.y);

                bool canPlace = (data != null) && gridManager.CanPlace(data, cellInt, currentRotation);

                if (data != null)
        {
                        bool canAfford = CanAfford(data);
                previewRender.color = (canPlace && canAfford) ? validPlace : invalidPlace;
            preview.transform.position = GetCenterPosition(cellIndex, currentRotation);

                        if (data.rotatable)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll != 0)
                {
                    currentRotation = (currentRotation + (scroll > 0 ? 1 : 3)) % 4;
                    Vector2Int rotatedSize = GridManager.GetRotatedSize(data.size, currentRotation);
                    preview.transform.localScale = new Vector3(
                        rotatedSize.x * gridManager.cellSize,
                        rotatedSize.y * gridManager.cellSize,
                        1
                    );
                }
            }

                        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace && !EventSystem.current.IsPointerOverGameObject())
            {
                if (!CanAfford(data))
                {
                    return;
                }
                SpendResources(data);
                Vector3 placePos = GetCenterPosition(cellIndex, currentRotation);
                GameObject build = Instantiate(building, placePos, Quaternion.Euler(0, 0, -currentRotation * 90));

                BuildingObject comp = build.GetComponent<BuildingObject>();
                comp.Init(cellIndex, data, cellIndex, currentRotation);
                gridManager.SetBlock(cellInt, comp, currentRotation);
            }
        }
        else         {
                        if (Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
            {
                                TryMine(cellInt);
                                TryAttack(worldPos);
                                BuildingObject clicked = gridManager.GetBlock(cellInt.x, cellInt.y);
                if (clicked != null)
                {
                    ShowBuildingData(clicked);
                }
                else
                {
                                        panelObject.SetActive(false);
                    selectedBuilding = null;
                }
            }
        }

                if (panelObject.activeSelf && selectedBuilding != null)
        {
            UpdatePanelContent();
            UpdatePanelPosition();
        }

                if (Mouse.current.rightButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
        {
            BuildingObject buildingToRemove = gridManager.GetBlock(cellInt.x, cellInt.y);
            if (buildingToRemove != null && buildingToRemove.data != null)
            {
                                if (buildingToRemove.data.buildCost != null)
                {
                    foreach (var cost in buildingToRemove.data.buildCost)
                    {
                        ItemDatabase.Instance.AddResource(cost.item, cost.amount);
                    }
                    Debug.Log($"Возвращены ресурсы за {buildingToRemove.data.buildingName}");
                }
            }
            gridManager.RemoveBlock(cellInt);
        }

                if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            data = null;
            preview.SetActive(false);
            panelObject.SetActive(false);
            selectedBuilding = null;
        }

                if (attackRayTimer > 0)
        {
            attackRayTimer -= Time.deltaTime;
            if (attackRayTimer <= 0 && attackLine != null)
                attackLine.enabled = false;
        }
    }
}