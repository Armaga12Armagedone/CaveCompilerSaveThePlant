using UnityEngine;

[CreateAssetMenu]
public class OreData : ScriptableObject
{
    public ItemData oreItem;
    public int id; // уникальный идентификатор
    public float miningTime = 2f;
    public int outputAmount = 1;
    public Sprite oreSprite;
    public Color oreColor;
}