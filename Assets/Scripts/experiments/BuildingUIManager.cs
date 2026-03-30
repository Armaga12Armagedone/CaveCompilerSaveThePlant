using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class BuildingUIManager : MonoBehaviour
{
    [Header("References")]
    public BuildSystem buildSystem; // перетащите объект с BuildSystem
    public GridLayoutGroup gridLayout; // ссылка на GridLayoutGroup (можно оставить пустым, найдётся автоматически)
    public GameObject buttonPrefab; // префаб кнопки (должен содержать TextMeshProUGUI)

    [Header("Button Appearance")]
    public Vector2 buttonSize = new Vector2(120, 80);
    public Vector2 spacing = new Vector2(10, 10);
    public Color normalColor = Color.white;
    public Color pressedColor = Color.gray;
    public Color selectedColor = Color.yellow; // цвет для активной кнопки (опционально)

    private Button[] buttons;
    private TextMeshProUGUI[] buttonTexts;

    void Start()
    {
        if (buildSystem == null)
            buildSystem = FindObjectOfType<BuildSystem>();

        if (gridLayout == null)
            gridLayout = GetComponent<GridLayoutGroup>();

        if (gridLayout == null)
        {
            Debug.LogError("GridLayoutGroup not found on this object! Add it or assign manually.");
            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError("Button prefab not assigned!");
            return;
        }

        // Настраиваем GridLayout
        gridLayout.cellSize = buttonSize;
        gridLayout.spacing = spacing;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4; // количество колонок (можно сделать настраиваемым)

        // Очищаем существующие дочерние элементы (если есть)
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // Создаём кнопки для каждого здания
        buttons = new Button[buildSystem.allBuildings.Length];
        buttonTexts = new TextMeshProUGUI[buildSystem.allBuildings.Length];

        for (int i = 0; i < buildSystem.allBuildings.Length; i++)
        {
            BuildingDatas building = buildSystem.allBuildings[i];
            if (building == null) continue;

            // Создаём кнопку
            GameObject btnObj = Instantiate(buttonPrefab, transform);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI text = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // Настройка внешнего вида
            ColorBlock colors = btn.colors;
            colors.normalColor = normalColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = selectedColor;
            btn.colors = colors;

            // Формируем текст с названием и стоимостью
            string displayText = $"{building.buildingName}\n";
            if (building.buildCost != null && building.buildCost.Length > 0)
            {
                StringBuilder costStr = new StringBuilder();
                foreach (var cost in building.buildCost)
                {
                    if (costStr.Length > 0) costStr.Append(" ");
                    costStr.Append($"{cost.item.displayName}:{cost.amount}");
                }
                displayText += costStr.ToString();
            }
            else
            {
                displayText += "Free";
            }

            text.text = displayText;
            text.fontSize = 12; // настройте по желанию
            text.alignment = TextAlignmentOptions.Center;

            // Привязываем событие
            int buildingID = building.buildingID;
            btn.onClick.AddListener(() => buildSystem.SelectBuilding(buildingID));

            buttons[i] = btn;
            buttonTexts[i] = text;
        }
    }

    // Опционально: обновить выделенную кнопку (подсветить выбранное здание)
    public void HighlightSelected(int buildingID)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            // Если нужно менять цвет, используйте ColorBlock или другой механизм
        }
    }
}