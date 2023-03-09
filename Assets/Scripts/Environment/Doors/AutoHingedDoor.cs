using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class AutoHingedDoor : MonoBehaviour
    {
        //Animate door
        public AnimationCurve openSpeedCurve = new AnimationCurve(new Keyframe[]
        { new Keyframe(0, 1, 0, 0), new Keyframe(0.8f, 1, 0, 0), new Keyframe(1, 0, 0, 0) });

        public float openSpeedMultiplier = 2.0f;
        public float doorOpenAngle = 90.0f;

        bool open = false;
        bool enter = false;

        float defaultRotationAngle;
        float currentRotationAngle;
        float openTime = 0;


        void Start()
        {
            defaultRotationAngle = transform.localEulerAngles.y;
            currentRotationAngle = transform.localEulerAngles.y;

            //Set Collider as trigger
            GetComponent<Collider>().isTrigger = true;
        }

        void Update()
        {
            if (openTime < 1)
            {
                openTime += Time.deltaTime * openSpeedMultiplier * openSpeedCurve.Evaluate(openTime);
            }
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.LerpAngle
            (currentRotationAngle, defaultRotationAngle + (open ? doorOpenAngle : 0), openTime), transform.localEulerAngles.z);

            if (enter)
            {
                open = !open;
                currentRotationAngle = transform.localEulerAngles.y;
                openTime = 0;
            }
            else
            {
                
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Farmer"))
            {
                enter = true;
            }
        }
  

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Farmer"))
            {
                enter = false;
            }
        }
    }
}