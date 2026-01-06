using UnityEngine;

/// <summary>
/// Abstract base class for ALL buildings.
/// Cannot be added directly - use SimpleBuilding or DefenseBuilding instead.
/// Provides health system and building management.
/// </summary>
public abstract class Building : MonoBehaviour
{
    

    public enum BuildingType
    {
        Base,
        Resource,
        MeleeDefense,
        RangedDefense,
        Normal
    }
    [Header("Building Identity")]
    //public BuildingType buildingType;
    // ==================== HEALTH SYSTEM ====================

    [Header("Health")]
    public float maxHealth = 200f;

    public float currentHealth;
    public bool IsDestroyed = false;

    // ==================== EVENTS ====================

    public event System.Action<Building> OnDestroyed;
    public event System.Action<float> OnHealthChanged; // Passes health percentage (0-1)

    // ==================== INITIALIZATION ====================

    protected virtual void Awake()
    {
        // Initialize health
        currentHealth = maxHealth;

      
    }

   

    // ==================== DAMAGE SYSTEM ====================

    /// <summary>
    /// Building takes damage from enemies.
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (IsDestroyed) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Fire health changed event (for health bars, UI)
        float healthPercent = GetHealthPercent();
        OnHealthChanged?.Invoke(healthPercent);

        Debug.Log($"[{gameObject.name}] took {damage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Destroyed();
        }
    }

    /// <summary>
    /// Repair building (restore health).
    /// </summary>
    public virtual void Repair(float amount)
    {
        if (IsDestroyed) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        float healthPercent = GetHealthPercent();
        OnHealthChanged?.Invoke(healthPercent);

        Debug.Log($"[{gameObject.name}] repaired {amount}. HP: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Called when health reaches zero.
    /// Can be overridden by child classes for custom death behavior.
    /// </summary>
    protected virtual void Destroyed()
    {
        if (IsDestroyed) return;

        IsDestroyed = true;


        // Fire destroyed event
        OnDestroyed?.Invoke(this);

        // TODO: Play destruction effects, particles, sound

        // Destroy GameObject after delay
        Destroy(gameObject, 1f);
    }

    // ==================== UTILITY METHODS ====================

    /// <summary>
    /// Get current health as percentage (0 to 1).
    /// Useful for health bars.
    /// </summary>
    public float GetHealthPercent()
    {
        return maxHealth > 0 ? currentHealth / maxHealth : 0f;
    }

  
}