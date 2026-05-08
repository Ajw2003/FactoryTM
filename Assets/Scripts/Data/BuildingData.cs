using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings
{
    [CreateAssetMenu(fileName = "New Building", menuName = "Construction/Building")]
    public class BuildingData : ScriptableObject
    {
        public string buildingName;
        public TileBase[] rotatedTiles; // 0:Right, 1:Down, 2:Left, 3:Up
        public float spawnInterval = 2.0f;
        public GameObject itemPrefab; // The "Resource" it creates
        public int cost;
        public BuildingType type;
    }

    public enum BuildingType
    {
        Chest,
        Miner,
        Seller,
        Conveyor
    }
}

