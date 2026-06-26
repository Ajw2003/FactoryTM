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
