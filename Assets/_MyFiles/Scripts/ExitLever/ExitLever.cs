using UnityEngine;

public class ExitLever : MonoBehaviour
{
    [Header("End Game Setup")]
    public GameObject winScreenUI;

    [Tooltip("How far in front of the player's face the UI spawns")]
    public float spawnDistance = 1.5f;

    private bool isPulled = false;

    private void Start()
    {
        if (winScreenUI != null) winScreenUI.SetActive(false);
    }

    public void OnLeverPulled()
    {
        if (isPulled) return;
        isPulled = true;

        Debug.Log("Lever Pulled! Freezing time and showing Win Screen...");

        if (winScreenUI != null)
        {
            Camera vrCamera = Camera.main;

            if (vrCamera != null)
            {
                // 1. Calculate the ideal position (in front of the player's face)
                Vector3 idealPos = vrCamera.transform.position + (vrCamera.transform.forward * spawnDistance);

                // 2. THE SMART CHECK: Is the path blocked, or is the spot inside a wall/pedestal?
                // We use a 0.3f radius sphere to represent the physical size of your UI panel.
                bool isBlocked = Physics.Linecast(vrCamera.transform.position, idealPos)
                              || Physics.CheckSphere(idealPos, 0.3f);

                if (isBlocked)
                {
                    // COLLISION DETECTED! Fall back to spawning above the pedestal.
                    Debug.Log("UI path blocked! Falling back to pedestal top.");
                    winScreenUI.transform.position = transform.position + new Vector3(0, 1.5f, 0);
                }
                else
                {
                    // IT'S CLEAR! Spawn right in front of their face.
                    winScreenUI.transform.position = idealPos;
                }

                // 3. Rotate the UI to look directly at the player's Headset
                Vector3 lookDirection = winScreenUI.transform.position - vrCamera.transform.position;
                lookDirection.y = 0; // Lock the Y axis so the menu stands straight
                winScreenUI.transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            winScreenUI.SetActive(true);
        }

        // 4. FREEZE THE GAME!
        Time.timeScale = 0f;
    }
}