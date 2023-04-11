using UnityEngine;

namespace DO
{
    public class EggCrack : MonoBehaviour
    {
        public float crackHeight = 3.0f;
        public bool isCracked = false;
        public Vector3 initialPosition;

        [SerializeField] private GameObject crackedEgg;
        [SerializeField] private GameObject egg;

        public void Update()
        {
            float height = transform.position.y - initialPosition.y;
            float velocity = GetComponent<Rigidbody>().velocity.y;

            if (!isCracked && height < 0 && Mathf.Abs(velocity) < 0.1f)
            {
                CrackOpen(); 
            }
        }

        public void CrackOpen()
        {
            initialPosition = transform.position;

            egg.SetActive(false);
            crackedEgg.SetActive(true);

            Vector3 crackedEggPosition = transform.position - new Vector3(0, 0.1f, 0);
            crackedEgg.transform.position = crackedEggPosition;
            crackedEgg.transform.rotation = transform.rotation;

            //Disable the rigidbody
            Rigidbody currentRigidbody = GetComponent<Rigidbody>();
            if (currentRigidbody != null)
            {
                currentRigidbody.isKinematic = true;
            }

            isCracked = true;

        }
    }
}