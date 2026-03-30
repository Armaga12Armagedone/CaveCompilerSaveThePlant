using UnityEngine;
using System.Collections.Generic;

public class BuildingObject : MonoBehaviour
{  
    public int x, y;
    public BuildingDatas data;
    public Vector2 zeroPos;
    public GameObject objectBuild;
    public int rotation = 0;
    
        private Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
    
        [SerializeField] public IBuildingModule currentModule;       private float productionTimer;

    public int health;
    

    public void Init(Vector2 coords, BuildingDatas buildingData, Vector2 zeroPos, int rotat)
    {
        x = (int)coords.x;
        y = (int)coords.y;
        data = buildingData;
        this.zeroPos = zeroPos;
        objectBuild = gameObject;
        rotation = rotat;
        currentModule = GetComponent<IBuildingModule>();
        health = data.maxHealth;
    }

    public void SelfInit()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError("GridManager не найден!");
            return;
        }

        if (data == null)
        {
            Debug.LogError($"BuildingObject на {name} не имеет данных!");
            return;
        }

                Vector2 cell = GridManager.Instance.GlobalToGrid(transform.position);
        Vector2Int cellInt = new Vector2Int(Mathf.RoundToInt(cell.x), Mathf.RoundToInt(cell.y));

                rotation = Mathf.RoundToInt(transform.eulerAngles.z / 90f) % 4;

                        Vector2Int sizeRotated = GridManager.GetRotatedSize(data.size, rotation);
        cellInt.x -= (sizeRotated.x - 1) / 2;
        cellInt.y -= (sizeRotated.y - 1) / 2;

                x = cellInt.x;
        y = cellInt.y;
        zeroPos = cellInt;

                GridManager.Instance.SetBlock(cellInt, this, rotation);
        objectBuild = gameObject;
        currentModule = GetComponent<IBuildingModule>();
        health = data.maxHealth;
        Debug.Log($"Здание {name} самоинициализировано на клетке ({x},{y}) с поворотом {rotation}");
    }

    public bool HasPlacedCells(Vector2 cell)
    {
        return cell.x >= zeroPos.x && cell.x < zeroPos.x + data.size.x &&
               cell.y >= zeroPos.y && cell.y < zeroPos.y + data.size.y;
    }
    
    void Update()
    {
        if (data == null) return;

        if (currentModule != null)
        {
            currentModule.OnUpdate();
            return;
        }
        
        if (CanProduce())
        {
            productionTimer += Time.deltaTime;
            
            if (productionTimer >= data.productionTime)
            {
                Produce();
                productionTimer = 0f;
            }
        }
        else
        {
            productionTimer = 0f;
        }
    }
    
    private bool CanProduce()
    {
        if (data.needItems == null || data.needItems.Length == 0)
            return true;
            
        foreach (var input in data.needItems)
        {
            if (!items.ContainsKey(input.item) || items[input.item] < input.amount)
                return false;
        }
        return true;
    }

    public virtual bool TryGiveItem(out ItemData item)
    {
        if (currentModule != null)
        {
            return currentModule.TryGiveItem(out item);
        }
        item = null;
                if (data.outputItem != null && GetItemCount(data.outputItem) > 0)
        {
            item = data.outputItem;
            RemoveItem(data.outputItem, 1);
            return true;
        }
        return false;
    }

        public virtual bool TryTakeItem(ItemData item)
    {
        if (currentModule != null)
        {
            return currentModule.TryTakeItem(item);
        }
                if (!CanAcceptItem(item)) return false;

        AddItem(item, 1);
        return true;
    }

        public virtual bool CanAcceptItem(ItemData item)
    {
        if (currentModule != null)
        {
            return currentModule.CanAcceptItem(item);
        }
        Debug.Log("Base accept");
        if (data.needItems == null) return false;
        foreach (var need in data.needItems)
            if (need.item == item)
                return true;
        return false;
    }
    
    private void Produce()
    {
        if (data.needItems != null)
        {
            foreach (var input in data.needItems)
            {
                RemoveItem(input.item, input.amount);
            }
        }
        
        AddItem(data.outputItem, data.outputAmount);
    }
    
    public void AddItem(ItemData item, int amount)
    {
        if (amount <= 0) return;
        
        if (items.ContainsKey(item))
            items[item] += amount;
        else
            items[item] = amount;
            
        if (items[item] > data.maxItems)
            items[item] = data.maxItems;
    }
    
    public bool RemoveItem(ItemData item, int amount)
    {
        if (amount <= 0) return false;
        if (!items.ContainsKey(item) || items[item] < amount) return false;
        
        items[item] -= amount;
        if (items[item] <= 0)
            items.Remove(item);
            
        return true;
    }
    
    public int GetItemCount(ItemData item)
    {
        return items.ContainsKey(item) ? items[item] : 0;
    }

    public Dictionary<ItemData, int> GetInventory() {
        return items;
    }
    
    public bool CanOutputItem(ItemData item)
    {
        return items.ContainsKey(item) && items[item] > 0;
    }
    
    public int TakeOutputItem(ItemData item, int maxAmount)
    {
        if (!items.ContainsKey(item)) return 0;
        
        int available = items[item];
        int take = Mathf.Min(available, maxAmount);
        
        items[item] -= take;
        if (items[item] <= 0)
            items.Remove(item);
            
        return take;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            GridManager.Instance.RemoveBlock(new Vector2Int(x, y));
            Destroy(gameObject);
        }
    }
}