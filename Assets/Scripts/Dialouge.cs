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

        private int index;

       public InputHandler inputHandler;

        private void Start()
        {
            dialougeText.text = string.Empty;
            StartDialouge(); 
        }

        public void Update()
        { 
            //Keycode.Return == Enter Key
            if(inputHandler.isClucking == true)
            {
               if(dialougeText.text == lines[index])
               {
                    NextLine();
                    inputHandler.isClucking = false; 
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
            //Type each character one at a time 
            foreach (char c in lines[index].ToCharArray())
            {
                dialougeText.text += c;
                yield return new WaitForSeconds(textSpeed); 
            }
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
                gameObject.SetActive(false); 
            }
        }
    }
}
