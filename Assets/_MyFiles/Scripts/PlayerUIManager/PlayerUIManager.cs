using UnityEngine;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance { get; private set; }

    [Header("Controller Anchors")]
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;

    [Header("Health/Armor UI")]
    public Transform healthCanvas;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;

    [Header("Ammo UI")]
    public Transform ammoCanvas;
    public TextMeshProUGUI ammoText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Default state: Health on Left wrist, Ammo hidden
        if (healthCanvas != null && leftHandAnchor != null)
            healthCanvas.SetParent(leftHandAnchor, false);

        if (ammoCanvas != null)
            ammoCanvas.gameObject.SetActive(false);
    }

    public void UpdateHealthArmor(int health, int armor)
    {
        if (healthText != null) healthText.text = $"HP: {health}%";
        if (armorText != null) armorText.text = $"AR: {armor}%";
    }

    public void OnWeaponGrabbed(Transform grabbingHand)
    {
        if (ammoCanvas == null || healthCanvas == null) return;

        ammoCanvas.gameObject.SetActive(true);

        // THE FIX: Check if the word "Right" is in the name of the hand grabbing the gun
        bool isRightHand = grabbingHand.name.Contains("Right") ||
                           (grabbingHand.parent != null && grabbingHand.parent.name.Contains("Right"));

        if (isRightHand)
        {
            // Gun is in Right Hand -> Ammo on Right, Health on Left
            ammoCanvas.SetParent(rightHandAnchor, false);
            healthCanvas.SetParent(leftHandAnchor, false);
        }
        else
        {
            // Gun is in Left Hand -> Ammo on Left, Health on Right
            ammoCanvas.SetParent(leftHandAnchor, false);
            healthCanvas.SetParent(rightHandAnchor, false);
        }
    }

    public void OnWeaponDropped()
    {
        // Hide ammo, put health back on the left wrist by default
        if (ammoCanvas != null) ammoCanvas.gameObject.SetActive(false);
        if (healthCanvas != null && leftHandAnchor != null)
        {
            healthCanvas.SetParent(leftHandAnchor, false);
        }
    }

    public void UpdateAmmo(WeaponBase.AmmoType ammoType, int loaded, int reserve, bool isInfinite)
    {
        if (ammoText == null) return;

        string reserveString = isInfinite ? "INF" : reserve.ToString();
        string typeString = isInfinite ? "Infinite" : ammoType.ToString();

        ammoText.text = $"{typeString}: {loaded} / {reserveString}";
    }
}