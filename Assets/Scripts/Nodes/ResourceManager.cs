using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Nodes
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
    
        private Dictionary<Vector2Int, ResourceNode> resourceNodes = new Dictionary<Vector2Int, ResourceNode>();
    
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
    
        public void RegisterNode(Vector2Int cell, ResourceNode node)
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
    
        public ResourceNode GetNodeAtPosition(Vector2Int cell)
        {
            resourceNodes.TryGetValue(cell, out ResourceNode node);
            return node;
        }
    
        public void DeregisterNode(Vector2Int cell)
        {
            if (resourceNodes.ContainsKey(cell))
            {
                resourceNodes.Remove(cell);
            }
        }
    
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
}


