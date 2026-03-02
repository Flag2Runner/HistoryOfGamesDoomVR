using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;

// Forces Unity to ensure an XRGrabInteractable is attached
[RequireComponent(typeof(XRGrabInteractable))]
public class WeaponBase : MonoBehaviour
{
    // --- ENUMS ---
    public enum AmmoType { Bullets, Shells, Rockets, Cells, None }

    // --- WEAPON STATS ---
    [Header("Combat Stats")]
    public int damage = 10;
    public float range = 100f;
    public Transform firePoint;
    public float fireRate = 0.5f;
    protected float nextFireTime;

    // --- AMMO SYSTEM ---
    [Header("Ammo System")]
    public AmmoType ammoType = AmmoType.Bullets;
    public int maxAmmoInMag = 6;
    public int currentAmmoInMag;

    [Tooltip("If false, this gun has infinite reserve ammo (like the Pistol or Enemy weapons)")]
    public bool usesReserveAmmo = true;

    // --- VR CONTROLS ---
    [Header("VR Controls")]
    [Tooltip("Drag the controller button action here (e.g., XRI RightHand/Primary Button)")]
    public InputActionReference reloadButton;

    // --- JUICE & EFFECTS ---
    [Header("Juice (Effects)")]
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip emptyClickSound;
    public AudioClip reloadSound;

    [Header("VR Haptics")]
    [Range(0, 1)] public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;

    // --- INTERNAL REFERENCES ---
    protected XRGrabInteractable grabInteractable;

    // ==========================================
    // INITIALIZATION
    // ==========================================
    protected virtual void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        currentAmmoInMag = maxAmmoInMag; // Start the game fully loaded
    }

    protected virtual void OnEnable()
    {
        // 1. Listen for the Trigger being pulled
        grabInteractable.activated.AddListener(OnTriggerPulled);

        // 2. Listen for the gun being grabbed and dropped (for UI)
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnDropped);

        // 3. Listen for the Reload button
        if (reloadButton != null)
        {
            reloadButton.action.Enable();
            reloadButton.action.performed += HandleReloadInput;
        }
    }

    protected virtual void OnDisable()
    {
        // Clean up listeners to prevent memory leaks!
        grabInteractable.activated.RemoveListener(OnTriggerPulled);
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnDropped);

        if (reloadButton != null)
        {
            reloadButton.action.performed -= HandleReloadInput;
            reloadButton.action.Disable();
        }
    }

    // ==========================================
    // VR GRAB & DROP (UI UPDATES)
    // ==========================================
    protected virtual void OnGrabbed(SelectEnterEventArgs args)
    {
        // Tell the Player that THIS is the gun they are holding
        if (PlayerCharacter.Instance != null)
        {
            PlayerCharacter.Instance.activeWeapon = this;
        }

        Transform handTransform = args.interactorObject.transform;

        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.OnWeaponGrabbed(handTransform);
        }

        UpdateAmmoUI();
    }

    protected virtual void OnDropped(SelectExitEventArgs args)
    {
        // 1. Check if another hand is STILL holding the weapon
        // interactorsSelecting.Count tells us how many hands are currently gripping it
        if (grabInteractable.interactorsSelecting.Count > 0)
        {
            // Get the transform of the hand that is still holding the gun
            Transform remainingHand = grabInteractable.interactorsSelecting[0].transform;

            // Tell the UI to anchor to the remaining hand instead of turning off
            if (PlayerUIManager.Instance != null)
            {
                PlayerUIManager.Instance.OnWeaponGrabbed(remainingHand);
            }

            return; // Stop the code here so it doesn't run the full drop logic!
        }

        // 2. If the count is 0, the weapon was COMPLETELY dropped.
        // Tell the player they let go of THIS gun
        if (PlayerCharacter.Instance != null && PlayerCharacter.Instance.activeWeapon == this)
        {
            PlayerCharacter.Instance.activeWeapon = null;
        }

        // Hide the ammo UI
        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.OnWeaponDropped();
        }
    }

    // ==========================================
    // RELOADING LOGIC
    // ==========================================
    private void HandleReloadInput(InputAction.CallbackContext context)
    {
        // Only trigger the reload if we are actively holding THIS gun
        if (grabInteractable.isSelected)
        {
            Reload();
        }
    }

    protected virtual void Reload()
    {
        // 1. Are we already full?
        if (currentAmmoInMag == maxAmmoInMag) return;

        int amountNeeded = maxAmmoInMag - currentAmmoInMag;

        // 2. Do we have infinite reserve ammo? (e.g., Pistol)
        if (!usesReserveAmmo || ammoType == AmmoType.None)
        {
            currentAmmoInMag = maxAmmoInMag;
            PlayReloadEffects();
            return;
        }

        // 3. We need real ammo. Ask the Player's backpack for it!
        if (PlayerCharacter.Instance != null)
        {
            int ammoReceived = PlayerCharacter.Instance.ConsumeAmmo(ammoType, amountNeeded);

            if (ammoReceived > 0)
            {
                currentAmmoInMag += ammoReceived;
                PlayReloadEffects();
            }
            else
            {
                Debug.Log("Out of reserve ammo!");
                if (audioSource != null && emptyClickSound != null)
                    audioSource.PlayOneShot(emptyClickSound); // Play a click if you try to reload with no ammo
            }
        }
    }

    private void PlayReloadEffects()
    {
        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        UpdateAmmoUI();
    }

    // ==========================================
    // FIRING LOGIC
    // ==========================================
    protected virtual void OnTriggerPulled(ActivateEventArgs args)
    {
        // Fire Rate Check
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            // Ammo Check
            if (currentAmmoInMag > 0)
            {
                currentAmmoInMag--;
                Fire(args);
            }
            else
            {
                DryFire();
            }
        }
    }

    // Specific weapons (like Pistol or Shotgun) will override this to add bullets/raycasts
    protected virtual void Fire(ActivateEventArgs args)
    {
        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);

        if (muzzleFlash != null)
            muzzleFlash.Play();

        TriggerHaptics(args);
        UpdateAmmoUI();
    }

    protected virtual void DryFire()
    {
        if (audioSource != null && emptyClickSound != null)
            audioSource.PlayOneShot(emptyClickSound);
    }

    // ==========================================
    // HELPERS
    // ==========================================
    protected void TriggerHaptics(ActivateEventArgs args)
    {
        if (args.interactorObject is XRBaseInputInteractor controllerInteractor)
        {
            controllerInteractor.SendHapticImpulse(hapticIntensity, hapticDuration);
        }
    }

    public void UpdateAmmoUI()
    {
        // First check if the UI even exists and if we are holding the gun
        if (PlayerUIManager.Instance != null && grabInteractable.isSelected)
        {
            int reserve = 999;

            // If the gun uses reserve ammo, safely try to get it from the player
            if (usesReserveAmmo)
            {
                if (PlayerCharacter.Instance != null)
                {
                    reserve = PlayerCharacter.Instance.GetReserveAmmo(ammoType);
                }
                else
                {
                    Debug.LogWarning("Missing PlayerCharacter! Did you forget to add the PlayerCharacter script to your XR Origin?");
                    reserve = 0; // Default to 0 so the game doesn't crash
                }
            }

            // Pass the data to the UI Singleton
            PlayerUIManager.Instance.UpdateAmmo(ammoType, currentAmmoInMag, reserve, !usesReserveAmmo);
        }
    }
}