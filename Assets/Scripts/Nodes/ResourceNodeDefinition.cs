using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceNodeDefinition", menuName = "FactoryTM/Resource Node Definition")]
public class ResourceNodeDefinition : ScriptableObject
{
    public GameObject minedItemPrefab;
    public float miningSpeed = 1f;
    public GameObject resourceNodePrefab; // The prefab that has the ResourceNode component
    public int spawnWeight = 1; // For weighted random selection
}
