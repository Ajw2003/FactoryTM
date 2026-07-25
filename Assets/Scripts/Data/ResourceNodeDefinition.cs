using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewResourceNodeDefinition", menuName = "FactoryTM/Resource Node Definition")]
public class ResourceNodeDefinition : ScriptableObject
{
    public GameObject minedItemPrefab;
    public float miningSpeed = 1f;
    public GameObject resourceNodePrefab; // The prefab that has the ResourceNode component
    public float spawnWeight = 1; // For weighted random selection
    public int oreCount = 1000;
    public TileBase oreGroundTile; // Painted onto GameManager.OreTileMap under the node so the ground itself reads as ore
}

