using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public UnityEvent<int> onHealthChanged;
    public UnityEvent onDeath;

    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        onDeath?.Invoke();
        
        // Default behavior: destroy object or disable it
        // Depending on requirements, we might want to handle this differently
        Debug.Log($"{gameObject.name} died.");
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
