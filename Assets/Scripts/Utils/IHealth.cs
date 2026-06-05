using UnityEngine;

public interface IHealth 
{
    float Health { get; set; }
    void TakeDamage(float amount);
    void Die();
}
