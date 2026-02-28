using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem; // Needed for the reload button

public class Pistol : WeaponBase
{
    [Header("DOOM Pistol Stats")]
    public float range = 100f;
    public int damage = 10;

    [Header("Ammo System")]
    public int maxAmmoInMag = 6;
    private int currentAmmo;

    [Header("VR Controls")]
    [Tooltip("Drag the controller button action here (e.g., XRI RightHand/Primary Button)")]
    public InputActionReference reloadButton;

    protected override void Awake()
    {
        base.Awake();
        currentAmmo = maxAmmoInMag; // Start with a full cylinder
    }

    protected override void OnEnable()
    {
        base.OnEnable(); // Keeps the trigger firing working

        // Turn on the reload button listener
        if (reloadButton != null)
        {
            reloadButton.action.Enable();
            reloadButton.action.performed += TriggerReload;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Clean up the listener so we don't cause errors
        if (reloadButton != null)
        {
            reloadButton.action.performed -= TriggerReload;
            reloadButton.action.Disable();
        }
    }

    private void TriggerReload(InputAction.CallbackContext context)
    {
        // ONLY reload if the player is currently holding this specific gun!
        if (grabInteractable.isSelected)
        {
            Reload();
        }
    }

    void Reload()
    {
        if (currentAmmo == maxAmmoInMag) return; // Already full

        currentAmmo = maxAmmoInMag;
        Debug.Log("Reloaded Revolver!");

        // Optional: Play a nice clicky reload sound here
        // if (audioSource) audioSource.PlayOneShot(reloadClip);
    }

    protected override void Fire(ActivateEventArgs args)
    {
        // 1. Check Ammo
        if (currentAmmo <= 0)
        {
            Debug.Log("Click! Out of ammo in mag.");
            // Optional: Play an empty click sound
            return;
        }

        // 2. Reduce Ammo and trigger the Base Juice (Flash, Bang, Vibrate)
        currentAmmo--;
        base.Fire(args);

        // 3. HITSCAN MATH (The Invisible Laser)
        // Shoot a raycast out from the firePoint. If it hits something within our range...
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range))
        {
            Debug.Log("Shot hit: " + hit.collider.name);

            // [We will add the Enemy Damage logic here later!]
            // Example: hit.collider.GetComponent<Enemy>()?.TakeDamage(damage);

            // Bonus Juice: Spawn a bullet hole or spark at 'hit.point'
        }
    }
}