// ItemData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/ItemData")]
public class ItemData : ScriptableObject
{
    public int id;
    public string displayName;
    public Sprite icon;
    // можно добавить другие поля: maxStack, цена и т.д.
}