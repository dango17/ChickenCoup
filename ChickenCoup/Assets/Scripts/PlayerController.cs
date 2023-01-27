using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class PlayerController : MonoBehaviour
    {
        new Rigidbody rigidbody;
        public float moveSpeed = 0.4f;
        public float rotateSpeed = 0.2f; 
        Transform mTransform; 

        private void Start()
        {
            mTransform = this.transform;
            rigidbody = GetComponent<Rigidbody>(); 
        }

        public void Move(Vector3 moveDirection, float delta)
        {
            rigidbody.velocity = moveDirection * moveSpeed;

            //Handle Rotation
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            mTransform.rotation = Quaternion.Slerp(mTransform.rotation, lookRotation, delta / rotateSpeed);
        }
    }
}
