using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // 1. Added this to load the game!

namespace Unity.VRTemplate
{
    /// <summary>
    /// Controls the steps in the in coaching card.
    /// </summary>
    public class StepManager : MonoBehaviour
    {
        [Serializable]
        class Step
        {
            [SerializeField]
            public GameObject stepObject;

            [SerializeField]
            public string buttonText;
        }

        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField;

        [SerializeField]
        List<Step> m_StepList = new List<Step>();

        // 2. Added this so you can set the Scene Index right in the Inspector
        [Header("Game Start Settings")]
        [Tooltip("The build index of your main game scene (usually 1).")]
        [SerializeField]
        public int gameSceneIndex = 1;

        int m_CurrentStepIndex = 0;

        // Safety lock to prevent crashing if the player double-clicks
        private bool isLoading = false;

        public void Next()
        {
            // 3. Check if we are currently on the VERY LAST step
            if (m_CurrentStepIndex == m_StepList.Count - 1)
            {
                if (isLoading) return;
                isLoading = true;

                Debug.Log("Tutorial finished! Starting game...");

                // Load the actual maze level!
                SceneManager.LoadScene(gameSceneIndex);

                return; // Stop running the rest of this function
            }

            // 4. If we are NOT on the last step, move forward normally
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);

            // Just add 1 (Removed the looping modulo math)
            m_CurrentStepIndex++;

            m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;
        }
    }
}