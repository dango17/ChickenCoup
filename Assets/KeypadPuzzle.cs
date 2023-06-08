using UnityEngine;

namespace DO
{
    public class KeypadPuzzle : MonoBehaviour
    {
        public bool isKeypadLocked = false;
        public float moveAmount = 1f;

        public GameObject doorToMove;

        public void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Keycard")
            {
                isKeypadLocked = true; 
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.tag == "Keycard")
            {
                isKeypadLocked = true;
            }
        }

        public void Update()
        {
            if(isKeypadLocked == true)
            {
                MoveDoorUp(); 
            }
        }

        public void MoveDoorUp()
        {
            doorToMove.transform.Translate(Vector3.up * moveAmount, Space.World);
        }
    }
}
