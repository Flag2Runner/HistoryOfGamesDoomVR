using UnityEngine;

public class PlayerCharacter : CharacterBase
{
    // Singleton so weapons can easily find the player's backpack
    public static PlayerCharacter Instance { get; private set; }

    [Header("DOOM Ammo Inventory")]
    public int reserveBullets = 50;
    public int reserveShells = 0;
    public int reserveRockets = 0;
    public int reserveCells = 0;

    [Header("Active Equipment")]
    // The Player tracks what they are currently holding!
    public WeaponBase activeWeapon;

    // REMOVED 'override' - It's just a standard Unity Awake now!
    private void Awake()
    {
        // Set up the Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    protected override void Start()
    {
        base.Start();
        entityType = EntityType.Player;
        currentArmor = 50;

        // Update the UI at the start (Safety check added in case UI hasn't loaded yet)
        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.UpdateHealthArmor(currentHealth, currentArmor);
        }
    }

    // Weapons will call this to check how much ammo they can grab
    public int GetReserveAmmo(WeaponBase.AmmoType type)
    {
        switch (type)
        {
            case WeaponBase.AmmoType.Bullets: return reserveBullets;
            case WeaponBase.AmmoType.Shells: return reserveShells;
            case WeaponBase.AmmoType.Rockets: return reserveRockets;
            case WeaponBase.AmmoType.Cells: return reserveCells;
            default: return 999; // For AmmoType.None (Infinite)
        }
    }

    // Weapons call this during Reload() to take ammo out of the backpack
    public int ConsumeAmmo(WeaponBase.AmmoType type, int amountNeeded)
    {
        int amountGiven = 0;

        switch (type)
        {
            case WeaponBase.AmmoType.Bullets:
                amountGiven = Mathf.Min(reserveBullets, amountNeeded);
                reserveBullets -= amountGiven;
                break;
            case WeaponBase.AmmoType.Shells:
                amountGiven = Mathf.Min(reserveShells, amountNeeded);
                reserveShells -= amountGiven;
                break;
                // Add Rockets and Cells here later!
        }

        return amountGiven;
    }

    // Pickups call this!
    public void AddAmmo(WeaponBase.AmmoType type, int amount)
    {
        switch (type)
        {
            case WeaponBase.AmmoType.Bullets: reserveBullets += amount; break;
            case WeaponBase.AmmoType.Shells: reserveShells += amount; break;
            case WeaponBase.AmmoType.Rockets: reserveRockets += amount; break;
            case WeaponBase.AmmoType.Cells: reserveCells += amount; break;
        }

        Debug.Log($"Picked up {amount} {type}!");

        // If the player is currently holding a gun, force its UI to update
        if (activeWeapon != null)
        {
            activeWeapon.UpdateAmmoUI();
        }
    }

    protected override void Die()
    {
        Debug.Log("PLAYER HAS DIED! GAME OVER!");
        onDeath?.Invoke();

        // Reset stats for now so you aren't stuck dead
        currentHealth = maxHealth;
        currentArmor = 0;

        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.UpdateHealthArmor(currentHealth, currentArmor);
        }
    }
    public override void Heal(int healAmount, bool canOvercharge = false)
    {
        base.Heal(healAmount, canOvercharge); // Do the normal math first

        // Then force the UI to refresh!
        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.UpdateHealthArmor(currentHealth, currentArmor);
        }
    }

    public override void AddArmor(int armorAmount, bool canOvercharge = false)
    {
        base.AddArmor(armorAmount, canOvercharge); // Do the normal math first

        // Then force the UI to refresh!
        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.UpdateHealthArmor(currentHealth, currentArmor);
        }
    }
}