using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public GameObject minedItemPrefab;
    public float miningSpeed = 1f; // Items per second
    public Vector3Int myCell { get; private set; }

    public void Setup(Vector3Int cell)
    {
        myCell = cell;
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RegisterNode(myCell, this);
        }
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.DeregisterNode(myCell);
        }
    }
}
