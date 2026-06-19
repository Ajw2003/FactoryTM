using UnityEngine;

public interface IHealth 
{
    int Health { get; set; }
    void TakeDamage(int amount);
    
    void ChangeHealth(int amount, int previous);
    void Die();
}
