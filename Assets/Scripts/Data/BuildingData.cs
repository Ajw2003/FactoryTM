using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Building", menuName = "Construction/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public TileBase[] rotatedTiles; // 0:Right, 1:Down, 2:Left, 3:Up
    public bool isConveyor;
    public bool isMiner;
    public float spawnInterval = 2.0f;
    public GameObject itemPrefab; // The "Resource" it creates
}