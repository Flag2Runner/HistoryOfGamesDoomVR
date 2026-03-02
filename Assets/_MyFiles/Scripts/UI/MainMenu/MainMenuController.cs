using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Needed for the Slider

public class MainMenuController : MonoBehaviour
{
    private bool isLoading = false;

    [Header("UI References")]
    [Tooltip("Drag your Volume Slider object here")]
    public Slider volumeSlider; // <-- We need a reference to the physical slider!

    void Start()
    {
        Time.timeScale = 1f;
        // When the menu loads, force the slider to match the game's actual current volume
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
        }
    }

    // --- BUTTON FUNCTIONS ---
    public void StartGame()
    {
        if (isLoading) return;
        isLoading = true;

        Debug.Log("Starting Game... Loading Scene 1!");
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        if (isLoading) return;
        isLoading = true;

        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // --- SETTINGS FUNCTIONS ---
    public void SetVolume(float sliderValue)
    {
        AudioListener.volume = sliderValue;
    }
}