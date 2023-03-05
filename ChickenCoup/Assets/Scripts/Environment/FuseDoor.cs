// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 04/03/2023

using UnityEngine;

namespace DO
{
    public class FuseDoor : MonoBehaviour
    {
        //Sliding door animation
        public AnimationCurve openSpeedCurve = new AnimationCurve(new Keyframe[]
        { new Keyframe(0, 1, 0, 0), new Keyframe(0.8f, 1, 0, 0), new Keyframe(1, 0, 0, 0) });

        public enum OpenDirection { x, y, z }
        public OpenDirection direction = OpenDirection.y;
        public float openDistance = 3f;
        public float openSpeedMultiplier = 2.0f;
        public Transform doorBody;

        public GameObject fuseObject;
        public Light lightIndicator; 

        bool open = false;

        Vector3 defaultDoorPosition;
        Vector3 currentDoorPosition;
        float openTime = 0;

        void Start()
        {
            lightIndicator.color = Color.red;

            if (doorBody)
            {
                defaultDoorPosition = doorBody.localPosition;
            }

            GetComponent<Collider>().isTrigger = true;
        }

        void Update()
        {
            if (!doorBody)
                return;

            if (openTime < 1)
            {
                openTime += Time.deltaTime * openSpeedMultiplier * openSpeedCurve.Evaluate(openTime);
            }

            if (direction == OpenDirection.x)
            {
                doorBody.localPosition = new Vector3(Mathf.Lerp(currentDoorPosition.x, defaultDoorPosition.x +
                    (open ? openDistance : 0), openTime), doorBody.localPosition.y, doorBody.localPosition.z);
            }
            else if (direction == OpenDirection.y)
            {
                doorBody.localPosition = new Vector3(doorBody.localPosition.x, Mathf.Lerp(currentDoorPosition.y, defaultDoorPosition.y +
                    (open ? openDistance : 0), openTime), doorBody.localPosition.z);
            }
            else if (direction == OpenDirection.z)
            {
                doorBody.localPosition = new Vector3(doorBody.localPosition.x, doorBody.localPosition.y, Mathf.Lerp(currentDoorPosition.z, defaultDoorPosition.z +
                    (open ? openDistance : 0), openTime));
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Fuse"))
            {
                open = true;
                currentDoorPosition = doorBody.localPosition;

                Destroy(GameObject.Find("FuseObject"));
                lightIndicator.color = Color.green; 
                openTime = 0.5f;
            }
        }
    }
}