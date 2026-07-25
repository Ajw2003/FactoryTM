using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public Sprite sprite;
    public string itemName;
    public string itemDescription;
    public specificItemType specificItemType;

}

public enum specificItemType
{
    Miner,
    IDT,
    Conveyor,
    Furnace,
    Turret,
    Wall,
    Chest,
    Copper,
    Stone,
    Iron,
    Diamond,
    Coal,
    Titanium,
    Uranium,
    Quartz,
}
