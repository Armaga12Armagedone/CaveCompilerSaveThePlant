using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class DroneController : MonoBehaviour
{
    public string droneTag = "Drone";
    public string[] resourceTags = new string[] { "Iron", "Water", "Stone" };
    public string buildingTag = "Building";

    private enum InputState { Idle, DroneSelected, WaitingForSource, WaitingForTarget }
    private InputState state = InputState.Idle;
    private DroneBase selectedDrone;
    private GameObject pendingSource;
    private Building highlightedBuilding;

    private bool isMobile;

    private void Awake()
    {
        isMobile = Application.isMobilePlatform;
        if (isMobile) EnhancedTouchSupport.Enable();
    }

    private void Update()
    {
        if (isMobile) HandleTouch();
        else HandleMouse();
    }

    private void HandleTouch()
    {
        if (Touch.activeTouches.Count > 0 && Touch.activeTouches[0].phase == TouchPhase.Began)
            ProcessInput(Touch.activeTouches[0].screenPosition);
    }

    private void HandleMouse()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            ProcessInput(Mouse.current.position.ReadValue());
    }

    private void ProcessInput(Vector2 screenPos)
    {
        Debug.Log($"ProcessInput at {screenPos}");
        if (GlobalManager.Instance.isPlacing)
        {
            GlobalManager.Instance.HandlePlacementInput(screenPos);
            return;
        }

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name}, tag: {hit.collider.tag}");
            GameObject hitObj = hit.collider.gameObject;
            string tag = hitObj.tag;

            switch (state)
            {
                case InputState.Idle:
                    if (tag == droneTag)
                    {

                        if (selectedDrone != null)
                            selectedDrone.SetSelected(false);
                        selectedDrone = hitObj.GetComponent<DroneBase>();
                        if (selectedDrone != null)
                        {
                            selectedDrone.SetSelected(true);
                            state = InputState.DroneSelected;
                            Debug.Log("Дрон выбран. Теперь выберите источник.");
                        }
                    }
                    else if (IsResourceTag(tag))
                    {
                        GlobalManager.Instance.CollectResourceByClick(hitObj);
                    }
                    else if (tag == buildingTag)
                    {
                        Building b = hitObj.GetComponent<Building>();
                        if (b != null && b.buildingType == BuildingType.Core)
                        {
                            GlobalManager.Instance.GenerateEnergyClick();
                        }
                    }
                    break;

                case InputState.DroneSelected:
                    if (IsResourceTag(tag) || tag == buildingTag)
                    {

                        if (highlightedBuilding != null)
                            highlightedBuilding.SetHighlight(false);

                        if (tag == buildingTag)
                        {
                            highlightedBuilding = hitObj.GetComponent<Building>();
                            if (highlightedBuilding != null)
                                highlightedBuilding.SetHighlight(true);
                        }
                        pendingSource = hitObj;
                        state = InputState.WaitingForTarget;
                        Debug.Log("Источник выбран. Теперь выберите здание-приёмник.");
                    }
                    else
                    {

                        if (selectedDrone != null)
                            selectedDrone.SetSelected(false);
                        selectedDrone = null;
                        state = InputState.Idle;
                        Debug.Log("Выбор отменён.");
                    }
                    break;

                case InputState.WaitingForTarget:
                    if (tag == buildingTag && hitObj != pendingSource)
                    {
                        Building target = hitObj.GetComponent<Building>();
                        if (target != null)
                        {
                            DroneBase.DroneTask task = CreateTask(pendingSource, target);
                            if (task != null)
                            {
                                selectedDrone.AssignTask(task);
                                Debug.Log($"Задача назначена.");
                            }
                        }

                        if (selectedDrone != null)
                            selectedDrone.SetSelected(false);
                        if (highlightedBuilding != null)
                            highlightedBuilding.SetHighlight(false);
                        selectedDrone = null;
                        pendingSource = null;
                        state = InputState.Idle;
                    }
                    else Debug.Log("Нужно выбрать другое здание.");
                    break;
            }
        }
        else if (state != InputState.Idle)
        {
            Debug.Log("No hit");

            if (selectedDrone != null)
                selectedDrone.SetSelected(false);
            if (highlightedBuilding != null)
                highlightedBuilding.SetHighlight(false);
            selectedDrone = null;
            pendingSource = null;
            state = InputState.Idle;
            Debug.Log("Выбор сброшен.");
        }
    }

    private bool IsResourceTag(string tag)
    {
        foreach (string rt in resourceTags)
            if (rt == tag) return true;
        return false;
    }

    private ResourceType GetResourceTypeFromTag(string tag)
    {
        switch (tag)
        {
            case "Iron": return ResourceType.Iron;
            case "Stone": return ResourceType.Stone;
            case "Water": return ResourceType.Water;
            default: return ResourceType.Iron;
        }
    }

    private DroneBase.DroneTask CreateTask(GameObject source, Building target)
    {
        var task = new DroneBase.DroneTask();
        task.targetBuilding = target;

        if (IsResourceTag(source.tag))
        {
            task.type = DroneBase.DroneTask.TaskType.Mining;
            task.sourceObject = source;
            task.resource = GetResourceTypeFromTag(source.tag);
        }
        else if (source.CompareTag(buildingTag))
        {
            Building sourceBuilding = source.GetComponent<Building>();
            if (sourceBuilding == null) return null;

            foreach (ResourceType rt in System.Enum.GetValues(typeof(ResourceType)))
            {
                if (sourceBuilding.GetResource(rt) > 0 &&
                    target.CanBeDestinationFor(rt) &&
                    sourceBuilding.CanBeSourceFor(rt))
                {
                    task.type = DroneBase.DroneTask.TaskType.Transfer;
                    task.sourceObject = source;
                    task.resource = rt;
                    return task;
                }
            }
            return null;
        }
        else return null;

        if (!task.targetBuilding.CanBeDestinationFor(task.resource)) return null;
        return task;
    }
}