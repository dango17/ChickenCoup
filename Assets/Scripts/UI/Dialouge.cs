using System.Collections;
using UnityEngine;
using TMPro;

namespace DO
{
    public class Dialouge : MonoBehaviour
    {
        public TextMeshProUGUI dialougeText;
        public string[] lines;
        public float textSpeed;
        public Color highlightColor;

        private int index;
        private bool isSkipping;

        public GameObject tutorialPrompts; 
        InputHandler inputHandler;

        private void Start()
        {
            inputHandler = FindObjectOfType<InputHandler>();

            dialougeText.text = string.Empty;
            StartDialouge(); 
        }

        public void Update()
        { 
            if(inputHandler.isSkipping == true)
            {
               if(dialougeText.text == lines[index])
               {
                    NextLine();
                    inputHandler.isSkipping = false; 
               }
               else
               {
                   StopAllCoroutines();
                   dialougeText.text = lines[index];
                }
            }
        }

        void StartDialouge()
        {
            index = 0;
            StartCoroutine(TypeLine()); 
        }

        IEnumerator TypeLine()
        {
            string line = lines[index];
            dialougeText.text = string.Empty;

            int currentIndex = 0;
            bool isTagOpen = false;
            bool isColorTagOpen = false;

            while (currentIndex < line.Length)
            {
                char currentChar = line[currentIndex];
                dialougeText.text += currentChar;

                if (currentChar == '<')
                {
                    isTagOpen = true;
                    if (currentIndex + 1 < line.Length && line[currentIndex + 1] == '#')
                    {
                        isColorTagOpen = true;
                        dialougeText.text += "<color=red>";
                        currentIndex++;
                    }
                }
                else if (currentChar == '>')
                {
                    isTagOpen = false;
                    if (isColorTagOpen)
                    {
                        dialougeText.text += "</color>";
                        isColorTagOpen = false;
                    }
                }

                if (!isTagOpen && !isColorTagOpen)
                {
                    yield return new WaitForSeconds(textSpeed);
                }

                currentIndex++;
            }

            while (!isSkipping)
            {
                yield return null;
            }

            NextLine();
        }

        void NextLine()
        {
            if(index < lines.Length - 1)
            {
                index++;
                dialougeText.text = string.Empty;
                StartCoroutine(TypeLine());
            }
            else
            {
                tutorialPrompts.SetActive(false); 
            }
        }
    }
}
