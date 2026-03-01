using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum PickupType { Health, Armor, Ammo }

    [Header("Pickup Settings")]
    public PickupType pickupType = PickupType.Health;
    public int amount = 25;

    [Tooltip("Only applies to Health/Armor. Can this push the player past 100%?")]
    public bool canOvercharge = false;

    [Header("Ammo Settings (Only used if Type is Ammo)")]
    public WeaponBase.AmmoType ammoCategory = WeaponBase.AmmoType.Bullets;

    [Header("Juice & Audio")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    [Tooltip("How fast it spins in place")]
    public float spinSpeed = 90f;
    [Tooltip("How fast it bobs up and down")]
    public float bobSpeed = 2f;
    [Tooltip("How high it bobs")]
    public float bobHeight = 0.2f;

    private Vector3 startPos;
    private bool isCollected = false;

    private void Start()
    {
        // Record the starting position so it knows where to bob from
        startPos = transform.position;
    }

    private void Update()
    {
        // 1. Classic DOOM Spin
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

        // 2. Classic DOOM Bobbing (using Sine wave math)
        float newY = startPos.y + (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent double-triggering if the player has multiple colliders on their body
        if (isCollected) return;

        // Check if the thing that touched us is the Player
        // We use GetComponentInParent because the collider might be on a child object of the XR Origin
        PlayerCharacter player = other.GetComponentInParent<PlayerCharacter>();

        if (player != null)
        {
            isCollected = true; // Lock it so it only applies once

            // Play sound at this location with our custom volume!
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
            }

            // Apply the specific effect based on the Enum!
            switch (pickupType)
            {
                case PickupType.Health:
                    player.Heal(amount, canOvercharge);
                    break;

                case PickupType.Armor:
                    player.AddArmor(amount, canOvercharge);
                    break;

                case PickupType.Ammo:
                    player.AddAmmo(ammoCategory, amount);
                    break;
            }

            // Destroy the pickup from the world
            Destroy(gameObject);
        }
    }
}