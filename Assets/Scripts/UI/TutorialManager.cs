using UnityEngine;
using System.Collections;
using TMPro;

// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 30/04/2023
namespace DO
{
    public class TutorialManager : MonoBehaviour
    {
        public string[] tutorialPrompts;
        private int currentPromptIndex = 0;
        private bool currentPromptCompleted = false;
        private bool allPromptsCompleted = false;

        InputHandler inputHandler;
        PlayerController playerController; 

        public float delaySeconds = 2.0f;
        public TextMeshProUGUI promptText;

        void Start()
        {
            inputHandler = FindObjectOfType<InputHandler>();
            playerController = FindObjectOfType<PlayerController>();

            promptText.text = tutorialPrompts[currentPromptIndex];
            currentPromptCompleted = false;
        }

        void Update()
        {
            if (!currentPromptCompleted)
            {
                CheckPromptConditions();
            }
            else
            {
                if (allPromptsCompleted)
                {
                    EndTutorial();
                }
                else
                {
                    StartCoroutine(ShowNextPromptWithDelay());
                }
            }
        }

        public void CheckPromptConditions()
        {
            switch (currentPromptIndex)
            {
                case 0: // "Use the left joystick to move"
                    if (Mathf.Abs(inputHandler.moveDirection.x) >= 0.1f || Mathf.Abs(inputHandler.moveDirection.y) >= 0.1f || Mathf.Abs(inputHandler.moveDirection.z) >= 0.1f)
                    {
                        currentPromptCompleted = true;
                    }
                    break;
                case 1: // "Press the A button to jump"
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        currentPromptCompleted = true;
                    }
                    break;
                    // Add more cases for other prompts as needed
            }

            if (currentPromptCompleted)
            {
                currentPromptIndex++;
                currentPromptCompleted = false;
                if (currentPromptIndex >= tutorialPrompts.Length)
                {
                    allPromptsCompleted = true;
                }
            }
        }

        IEnumerator ShowNextPromptWithDelay()
        {
            //Wait with delay
            yield return new WaitForSeconds(delaySeconds);

            if (allPromptsCompleted)
            {
                //End the tutorial if there are no more prompts
                EndTutorial();
            }
            else
            {
                // Set the text of the UI prompt to the next prompt
                promptText.text = tutorialPrompts[currentPromptIndex];
            }
        }

        public void EndTutorial()
        {
            promptText.gameObject.SetActive(false);
            Debug.Log("Congratulations! You have completed the tutorial.");
        }
    }
} 