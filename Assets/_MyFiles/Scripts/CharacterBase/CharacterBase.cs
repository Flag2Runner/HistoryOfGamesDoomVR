using UnityEngine;
using UnityEngine.Events;

public class CharacterBase : MonoBehaviour
{
    public enum EntityType { Player, Enemy, Prop, NPC }

    [Header("Identity")]
    public EntityType entityType = EntityType.Enemy;

    [Header("DOOM Stats")]
    public int maxHealth = 100; // Normal cap
    public int hardCapHealth = 200; // Soulsphere cap
    protected int currentHealth;

    public int maxArmor = 100; // Green armor cap
    public int hardCapArmor = 200; // Megaarmor cap
    protected int currentArmor;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent onTakeDamage;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        currentArmor = 0; // Most entities spawn with 0 armor
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        // Armor absorbs damage first
        if (currentArmor > 0)
        {
            if (currentArmor >= damageAmount)
            {
                currentArmor -= damageAmount;
                damageAmount = 0;
            }
            else
            {
                damageAmount -= currentArmor;
                currentArmor = 0;
            }
        }

        if (damageAmount > 0)
        {
            currentHealth -= damageAmount;
        }

        Debug.Log($"{gameObject.name} took damage! HP: {currentHealth} | Armor: {currentArmor}");
        onTakeDamage?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} was killed!");
        onDeath?.Invoke();

        // Base behavior is just to disappear. No scoring logic here!
        Destroy(gameObject);
    }

    // Pickups will call these!
    // 'canOvercharge = true' is for Soulspheres/Megaarmor
    public virtual void Heal(int healAmount, bool canOvercharge = false)
    {
        int cap = canOvercharge ? hardCapHealth : maxHealth;

        // Only heal if we are below the allowed cap
        if (currentHealth < cap)
        {
            currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, cap);
        }
    }

    public virtual void AddArmor(int armorAmount, bool canOvercharge = false)
    {
        int cap = canOvercharge ? hardCapArmor : maxArmor;

        if (currentArmor < cap)
        {
            currentArmor = Mathf.Clamp(currentArmor + armorAmount, 0, cap);
        }
    }
}