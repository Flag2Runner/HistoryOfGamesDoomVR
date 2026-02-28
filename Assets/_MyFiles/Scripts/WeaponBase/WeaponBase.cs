using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem; // Added for the reload button

[RequireComponent(typeof(XRGrabInteractable))]
public class WeaponBase : MonoBehaviour
{
    [Header("Base Weapon Settings")]
    public Transform firePoint;
    public float fireRate = 0.5f;
    protected float nextFireTime;

    [Header("Ammo System")]
    public int maxAmmo = 6;
    protected int currentAmmo;
    [Tooltip("Drag the controller button action here (e.g., XRI RightHand/Primary Button)")]
    public InputActionReference reloadButton;

    [Header("Juice (Effects)")]
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip emptyClickSound;
    public AudioClip reloadSound;

    [Header("VR Haptics")]
    [Range(0, 1)] public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;

    protected XRGrabInteractable grabInteractable;

    protected virtual void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        currentAmmo = maxAmmo; // Start loaded
    }

    protected virtual void OnEnable()
    {
        grabInteractable.activated.AddListener(OnTriggerPulled);

        // Turn on the reload listener
        if (reloadButton != null)
        {
            reloadButton.action.Enable();
            reloadButton.action.performed += HandleReloadInput;
        }
    }

    protected virtual void OnDisable()
    {
        grabInteractable.activated.RemoveListener(OnTriggerPulled);

        if (reloadButton != null)
        {
            reloadButton.action.performed -= HandleReloadInput;
            reloadButton.action.Disable();
        }
    }

    private void HandleReloadInput(InputAction.CallbackContext context)
    {
        // Only trigger the virtual reload if we are holding THIS gun
        if (grabInteractable.isSelected)
        {
            Reload();
        }
    }

    // VIRTUAL RELOAD: Other weapons can override this if they have unique reloads!
    protected virtual void Reload()
    {
        if (currentAmmo == maxAmmo) return;

        currentAmmo = maxAmmo;
        Debug.Log($"{gameObject.name} Reloaded!");

        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);
    }

    protected virtual void OnTriggerPulled(ActivateEventArgs args)
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            // Base class handles checking and consuming ammo!
            if (currentAmmo > 0)
            {
                currentAmmo--;
                Fire(args);
            }
            else
            {
                DryFire();
            }
        }
    }

    // VIRTUAL FIRE: specific weapons override this for their bullet logic
    protected virtual void Fire(ActivateEventArgs args)
    {
        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);

        if (muzzleFlash != null)
            muzzleFlash.Play();

        TriggerHaptics(args);
    }

    protected virtual void DryFire()
    {
        Debug.Log("Click! Out of ammo.");
        if (audioSource != null && emptyClickSound != null)
            audioSource.PlayOneShot(emptyClickSound);
    }

    protected void TriggerHaptics(ActivateEventArgs args)
    {
        if (args.interactorObject is XRBaseInputInteractor controllerInteractor)
        {
            controllerInteractor.SendHapticImpulse(hapticIntensity, hapticDuration);
        }
    }
}