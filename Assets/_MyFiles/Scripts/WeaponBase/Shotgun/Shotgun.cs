using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Shotgun : WeaponBase
{
    [Header("Shotgun Spread Stats")]
    [Tooltip("How many pellets are fired per trigger pull")]
    public int pelletCount = 8;

    [Tooltip("How wide the cone of fire is (0.1 is tight, 0.5 is very wide)")]
    public float spreadAngle = 0.1f;

    protected override void Fire(ActivateEventArgs args)
    {
        // 1. Play sound, flash, deduct ammo, vibrate, and update UI
        // (This completely handles the Base class stuff!)
        base.Fire(args);

        // 2. THE PELLET LOOP
        for (int i = 0; i < pelletCount; i++)
        {
            // Generate a random 2D point for this specific pellet
            Vector2 randomSpread = Random.insideUnitCircle * spreadAngle;

            // Apply that offset to the barrel's forward direction
            Vector3 pelletDirection = firePoint.forward
                                    + (firePoint.right * randomSpread.x)
                                    + (firePoint.up * randomSpread.y);

            // Normalize ensures the vector stays a true direction (length of 1)
            pelletDirection.Normalize();

            // 3. HITSCAN MATH
            if (Physics.Raycast(firePoint.position, pelletDirection, out RaycastHit hit, range))
            {
                Debug.Log($"Pellet {i} hit: {hit.collider.name}");

                // Did we hit an enemy or prop?
                CharacterBase target = hit.collider.GetComponent<CharacterBase>();

                if (target != null && target.entityType != CharacterBase.EntityType.Player)
                {
                    // Deal damage! 
                    // Note: If damage is 10, and all 8 pellets hit, it deals 80 total damage!
                    target.TakeDamage(damage);
                }
                else
                {
                    // Optional: You can instantiate a spark or bullet hole particle at 'hit.point' here!
                }
            }
        }
    }
}