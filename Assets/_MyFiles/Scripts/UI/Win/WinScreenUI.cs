using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class WinScreenUI : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI enemiesText;
    public TextMeshProUGUI itemsText;

    // THE FIX: This boolean locks the menu as soon as a button is clicked!
    private bool isProcessingAction = false;

    private void OnEnable()
    {
        // Unlock the menu every time the screen pops up
        isProcessingAction = false;

        if (PlayerCharacter.Instance != null)
        {
            if (scoreText != null) scoreText.text = $"Score: {PlayerCharacter.Instance.currentScore}";
            if (enemiesText != null) enemiesText.text = $"Enemies Killed: {PlayerCharacter.Instance.enemiesKilled}";
            if (itemsText != null) itemsText.text = $"Items Picked Up: {PlayerCharacter.Instance.itemsPickedUp}";
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void OnRestartClicked()
    {
        if (isProcessingAction) return;
        isProcessingAction = true;

        Debug.Log("Restarting Game...");

        // 1. UNFREEZE TIME FIRST!
        Time.timeScale = 1f;

        if (PlayerCharacter.Instance != null) PlayerCharacter.Instance.ResetStats();
        if (LevelGenerator.Instance != null) LevelGenerator.Instance.LevelComplete();

        gameObject.SetActive(false);
    }

    public void OnMainMenuClicked()
    {
        if (isProcessingAction) return;
        isProcessingAction = true;

        // UNFREEZE TIME!
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void OnQuitClicked()
    {
        if (isProcessingAction) return;
        isProcessingAction = true;

        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}