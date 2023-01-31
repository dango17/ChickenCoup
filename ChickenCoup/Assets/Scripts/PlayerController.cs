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
        public Animator animator;

        private void Start()
        {
            mTransform = this.transform;
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>(); 
        }

        public void Move(Vector3 moveDirection, float delta)
        {
            rigidbody.velocity = moveDirection * moveSpeed;

            //Handle Rotation
            //lookDir = currentRotation point
            Vector3 lookDir = moveDirection;
            if (lookDir == Vector3.zero)
                lookDir = mTransform.forward; 

            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            mTransform.rotation = Quaternion.Slerp(mTransform.rotation, lookRotation, delta / rotateSpeed);
        }

        public void HandleMovementAnimations(float moveAmount, float delta)
        {
            float m = moveAmount;
            if (m > 0.1f && m < 0.51f)
                m = 0.5f;

            if (m > 0.51f)
                m = 1;

            if (m < 0.1f)
                m = 0f; 

            animator.SetFloat("movement", m, 0.1f, delta);
        }
    }
}
