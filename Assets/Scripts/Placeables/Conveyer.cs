using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Placeables
{
    using UnityEngine;
    using UnityEngine.Tilemaps;
    
    [CreateAssetMenu]
    public class ConveyorTile : Tile 
    {
        public Vector3Int direction; // e.g., (1, 0, 0) for Right
    }
}


