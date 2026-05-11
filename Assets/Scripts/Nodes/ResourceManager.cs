using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private Dictionary<Vector3Int, ResourceNode> resourceNodes = new Dictionary<Vector3Int, ResourceNode>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void RegisterNode(Vector3Int cell, ResourceNode node)
    {
        if (!resourceNodes.ContainsKey(cell))
        {
            resourceNodes.Add(cell, node);
        }
        else
        {
            Debug.LogWarning($"ResourceNode already exists at cell: {cell}. Overwriting.");
            resourceNodes[cell] = node;
        }
    }

    public ResourceNode GetNodeAtPosition(Vector3Int cell)
    {
        resourceNodes.TryGetValue(cell, out ResourceNode node);
        return node;
    }

    public void DeregisterNode(Vector3Int cell)
    {
        if (resourceNodes.ContainsKey(cell))
        {
            resourceNodes.Remove(cell);
        }
    }
}
