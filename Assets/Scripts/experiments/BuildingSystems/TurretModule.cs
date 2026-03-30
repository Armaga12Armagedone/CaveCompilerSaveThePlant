using UnityEngine;
using System.Collections.Generic;

public class TurretModule : MonoBehaviour, IBuildingModule
{
    [Header("Turret Settings")]
    public float range = 5f;
    public float fireRate = 1f;
    public int damage = 10;

    private BuildingObject building;
    private float shootCooldown;
    private Transform currentTarget;
    [SerializeField] private LineRenderer shotLine;

    void Start()
    {
        building = GetComponent<BuildingObject>();
        if (building == null) return;

        // Настройка LineRenderer для визуализации выстрела
        shotLine = GetComponent<LineRenderer>();
        if (shotLine == null)
            shotLine = gameObject.AddComponent<LineRenderer>();
        shotLine.enabled = false;
        shotLine.startWidth = 0.1f;
        shotLine.endWidth = 0.1f;
        shotLine.material = new Material(Shader.Find("Sprites/Default")) { color = Color.red };
    }

    public void OnUpdate()
    {
        if (building == null || building.data == null) return;

        // Поиск цели
        if (currentTarget == null || !IsTargetValid())
            FindTarget();

        if (currentTarget != null)
        {
            // Поворот башни к цели
            Vector3 dir = currentTarget.position - building.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            building.transform.rotation = Quaternion.Euler(0, 0, angle - 90);

            // Проверка аммуниции (берём из needItems)
            Debug.Log(HasAmmo());
            if (HasAmmo())
            {
                shootCooldown -= Time.deltaTime;
                if (shootCooldown <= 0)
                {
                    Shoot();
                    shootCooldown = 1f / fireRate;
                }
            }
        }
    }

    bool HasAmmo()
    {
        if (building.data.needItems == null || building.data.needItems.Length == 0)
            return true; // если нет потребностей, стреляет без аммуниции
        // Берём первый требуемый предмет как аммуницию
        var ammo = building.data.needItems[0];
        return building.GetItemCount(ammo.item) >= ammo.amount;
    }

    void ConsumeAmmo()
    {
        if (building.data.needItems == null || building.data.needItems.Length == 0) return;
        var ammo = building.data.needItems[0];
        building.RemoveItem(ammo.item, ammo.amount);
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(building.transform.position, range);
        float closest = range;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(building.transform.position, hit.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    currentTarget = hit.transform;
                }
            }
        }
    }

    bool IsTargetValid()
    {
        if (currentTarget == null) return false;
        float dist = Vector2.Distance(building.transform.position, currentTarget.position);
        return dist <= range && currentTarget.gameObject.activeSelf;
    }

    void Shoot()
    {
        ConsumeAmmo();
        Debug.Log("shoot");
        EnemyUnit unit = currentTarget.GetComponent<EnemyUnit>();
        if (unit != null)
            unit.TakeDamage(damage);
        StartCoroutine(ShowShotLine());
    }

    System.Collections.IEnumerator ShowShotLine()
    {
        if (shotLine == null) yield break;
        shotLine.enabled = true;
        shotLine.SetPosition(0, building.transform.position);
        shotLine.SetPosition(1, currentTarget.position);
        yield return new WaitForSeconds(0.1f);
        shotLine.enabled = false;
    }

    // Методы интерфейса
    public bool TryGiveItem(out ItemData item)
    {
        // Турель не отдаёт предметы
        item = null;
        return false;
    }

    public bool TryTakeItem(ItemData item)
    {
        // Турель принимает предметы, только если они нужны (аммуниция)
        if (!CanAcceptItem(item)) return false;
        building.AddItem(item, 1);
        return true;
    }

    public bool CanAcceptItem(ItemData item)
    {
        // Турель принимает только те предметы, которые указаны в needItems
        if (building.data.needItems == null) return false;
        foreach (var need in building.data.needItems)
        {
            if (need.item == item)
                return true;
        }
        return false;
    }
    public Dictionary<ItemData, int> GetInventory() => building.GetInventory();
}