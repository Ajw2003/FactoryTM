using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public Sprite sprite;
    public string itemName;
    public string itemDescription;
    public itemType itemType;
    public ResourceType resourceType;
    
}

public enum itemType
{
    Ore,
    Building,
    Weapon,
    Consumable
}
