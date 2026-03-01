using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pistol : WeaponBase
{
    protected override void Fire(ActivateEventArgs args)
    {
        // 1. Play sound, flash, deduct ammo, and vibrate controller
        base.Fire(args);

        // 2. HITSCAN MATH
        // Shoot a ray using the inherited 'range' variable
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range))
        {
            Debug.Log($"Pistol hit: {hit.collider.name}");

            // Did we hit a CharacterBase (Enemy, Prop, or Player)?
            CharacterBase target = hit.collider.GetComponent<CharacterBase>();

            if (target != null)
            {
                // Make sure we aren't shooting ourselves! (Friendly fire check)
                if (target.entityType != CharacterBase.EntityType.Player)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }
}