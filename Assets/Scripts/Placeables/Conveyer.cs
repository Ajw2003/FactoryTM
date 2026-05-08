using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class ConveyorTile : Tile 
{
    public Vector3Int direction; // e.g., (1, 0, 0) for Right
}