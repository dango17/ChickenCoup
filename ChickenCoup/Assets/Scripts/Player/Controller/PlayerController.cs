using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Speeds")]
        public float moveSpeed = 0.4f;
        public float rotateSpeed = 0.2f;
        [Header("Cover")]
        public float wallSpeed = 0.2f;
        public float wallCheckDistance = 0.2f;
        [Header("Flags")]
        public bool isOnCover;
        [HideInInspector]
        public Transform mTransform;
        [HideInInspector]
        public Animator animator;
        [HideInInspector]
        new Rigidbody rigidbody;

        private void Start()
        {
            mTransform = this.transform;
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>(); 
        }

        public void WallMovement(Vector3 moveDirection, Vector3 normal ,float delta)
        {
            //Movement 
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normal);
            Debug.DrawRay(mTransform.position, projectedVelocity, Color.blue);
            Vector3 relativeDir = mTransform.InverseTransformDirection(projectedVelocity);
            Vector3 origin = mTransform.position;
            origin.y += 1;

            if (Mathf.Abs(relativeDir.x) > 0.01f)
            {
                if (relativeDir.x > 0)
                    origin += mTransform.right * wallCheckDistance;

                if (projectedVelocity.x < 0)
                    origin -= mTransform.right * wallCheckDistance; 

                Debug.DrawRay(origin, -normal, Color.red);
                if (Physics.Raycast(origin, -normal, out RaycastHit hit, 2f))
                {
                    //Do nothing
                }
                else
                {
                    projectedVelocity = Vector3.zero;
                    relativeDir.x = 0; 
                }
            }
            else
            {
                projectedVelocity = Vector3.zero;
                relativeDir.x = 0; 
            }

            rigidbody.velocity = projectedVelocity * wallSpeed;
            HandleRotation(-normal, delta);

            float m = 0;
       
            //Looking inward therefore 
            m = relativeDir.x;
            if(m < 0.1f && m > -0.1f)
            {
                m = 0;
            }
            else
            {
                m = (m < 0) ? -1 : 1; 
            }

            animator.SetFloat("movement", m, 0.1f, delta); 
        }

        public void Move(Vector3 moveDirection, float delta)
        { 
            rigidbody.velocity = moveDirection * moveSpeed;
            HandleRotation(moveDirection, delta);         
        }

        void HandleRotation(Vector3 lookDir, float delta)
        {
            //Handle Rotation
            //lookDir = currentRotation point      
            if (lookDir == Vector3.zero)
                lookDir = mTransform.forward;

            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            mTransform.rotation = Quaternion.Slerp(mTransform.rotation, lookRotation, delta / rotateSpeed);
        }

        public void HandleAnimatorStates()
        {
            animator.SetBool("isOnCover", isOnCover);
        }

        public void HandleMovementAnimations(float moveAmount, float delta)
        {
            //Set moveAmount Values 
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
