using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public int prefabKey;

    public void ReturnToPool()
    {
        if (ObjectPoolManager.HasInstance)
        {
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

