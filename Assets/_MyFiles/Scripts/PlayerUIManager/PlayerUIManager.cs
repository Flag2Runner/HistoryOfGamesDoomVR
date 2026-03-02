using UnityEngine;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance { get; private set; }

    [Header("Damage Flash Settings")]
    public UnityEngine.UI.Image damageFlashImage;
    public float flashDuration = 0.5f;
    [Tooltip("How dark the red gets. 0.35 is safe for VR.")]
    public float maxAlpha = 0.35f;

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

    [Header("Game Over Screen")]
    public GameObject gameOverCanvas;

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

    public void TriggerDamageFlash()
    {
        if (damageFlashImage != null)
        {
            // Stop any flashes currently happening so they don't fight each other
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }
    }
    private System.Collections.IEnumerator FlashRoutine()
    {
        // 1. Instantly spike the red color to 35% opacity
        Color flashColor = damageFlashImage.color;
        flashColor.a = maxAlpha;
        damageFlashImage.color = flashColor;

        // 2. Smoothly fade it back to 0% over time
        float timer = 0f;
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            // Mathf.Lerp smoothly blends between the max alpha and zero based on time
            flashColor.a = Mathf.Lerp(maxAlpha, 0f, timer / flashDuration);
            damageFlashImage.color = flashColor;

            yield return null; // Wait for the next frame
        }

        // 3. Ensure it's perfectly invisible when done
        flashColor.a = 0f;
        damageFlashImage.color = flashColor;
    }

    // Call this when the player dies
    public void ShowGameOver()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);

            // 1. Find the VR Headset
            if (Camera.main != null)
            {
                Transform head = Camera.main.transform;

                // 2. Teleport the canvas exactly 2 meters in front of the player's face
                // We lock the Y axis so the menu doesn't spawn tilted into the ceiling/floor if they are looking up/down
                Vector3 spawnPos = head.position + (head.forward * 2.0f);
                spawnPos.y = head.position.y;
                gameOverCanvas.transform.position = spawnPos;

                // 3. Make the canvas look at the player, then flip it 180 degrees so the text isn't backward!
                gameOverCanvas.transform.LookAt(head);
                gameOverCanvas.transform.Rotate(0, 180, 0);
            }

            // 4. Freeze time so Pinky stops attacking you while you try to click Restart!
            Time.timeScale = 0f;
        }
    }
}