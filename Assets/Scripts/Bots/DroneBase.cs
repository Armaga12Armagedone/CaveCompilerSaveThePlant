using UnityEngine;

public class DroneBase : MonoBehaviour
{
    public int ID = -1;
    public float speed = 5f;
    public int carryCapacity = 1;
    public GameObject selectionIndicator;

    [System.Serializable]
    public class DroneTask
    {
        public enum TaskType { Mining, Transfer }
        public TaskType type;
        public ResourceType resource;
        public GameObject sourceObject;
        public Building targetBuilding;
        public int amount = 1;
    }

    private DroneTask currentTask;
    private int carriedAmount = 0;
    private ResourceType carriedResource;
    private bool movingToTarget = true;

    private GlobalManager manager;

    void Start()
    {
        manager = GlobalManager.Instance;
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
    }

    void Update()
    {
        if (currentTask == null) return;

        if (movingToTarget)
        {
            if (currentTask.targetBuilding == null)
            {
                currentTask = null;
                return;
            }
        }
        else
        {
            if (currentTask.sourceObject == null)
            {
                currentTask = null;
                return;
            }
        }

        Vector3 targetPosition = movingToTarget ?
            currentTask.targetBuilding.transform.position :
            currentTask.sourceObject.transform.position;

        MoveTowards(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            Arrived();
    }

    private void MoveTowards(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    private void Arrived()
    {
        if (movingToTarget)
        {
            currentTask.targetBuilding.AddResource(carriedResource, carriedAmount);
            carriedAmount = 0;

            if (currentTask.type == DroneTask.TaskType.Mining)
            {
                if (currentTask.sourceObject != null)
                {
                    Resource res = currentTask.sourceObject.GetComponent<Resource>();
                    if (res != null && res.resourceCount > 0)
                    {
                        movingToTarget = false;
                    }
                    else
                    {
                        currentTask = null;
                    }
                }
                else
                {
                    currentTask = null;
                }
            }
            else
            {
                movingToTarget = false;
            }
        }
        else
        {
            if (currentTask.type == DroneTask.TaskType.Mining)
            {
                carriedAmount = carryCapacity;
                carriedResource = currentTask.resource;
                Resource res = currentTask.sourceObject.GetComponent<Resource>();
                if (res != null)
                {
                    res.resourceCount -= carryCapacity;
                    if (res.resourceCount <= 0)
                        Destroy(currentTask.sourceObject);
                }
            }
            else
            {
                Building sourceBuilding = currentTask.sourceObject.GetComponent<Building>();
                if (sourceBuilding != null)
                {
                    int taken = sourceBuilding.RemoveResource(currentTask.resource, carryCapacity);
                    carriedAmount = taken;
                    carriedResource = currentTask.resource;
                }
            }

            if (carriedAmount > 0)
                movingToTarget = true;
            else
                currentTask = null;
        }
    }

    public void AssignTask(DroneTask task)
    {
        currentTask = task;
        movingToTarget = false;
        carriedAmount = 0;
    }

    public void ClearTask()
    {
        currentTask = null;
    }

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);
    }
}