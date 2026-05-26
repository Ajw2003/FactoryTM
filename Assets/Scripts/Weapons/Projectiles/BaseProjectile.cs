using System;
using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    public void Start()
    {
        Invoke(nameof(Despawn), 1f); 
    }

    public float despawnTime = 2f;
    
    public void Despawn()
    {
        Destroy(gameObject);
    }
}
