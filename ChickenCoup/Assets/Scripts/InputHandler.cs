using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class InputHandler : MonoBehaviour
    {
        public Transform camHolder;

        public ExecutionOrder movementOrder;
        public PlayerController controller; 

        Vector3 moveDirection; 

        public enum ExecutionOrder
        {
            fixedUpdate, update, lateUpdate
        }

        private void FixedUpdate()
        {
            if(movementOrder == ExecutionOrder.fixedUpdate)
            {
                controller.Move(moveDirection, Time.fixedDeltaTime);
            }
        }

        private void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float Vertical = Input.GetAxis("Vertical");

            moveDirection = camHolder.forward * Vertical;
            moveDirection += camHolder.right * horizontal;
            moveDirection.Normalize(); 

            if(movementOrder == ExecutionOrder.update)
            {
                controller.Move(moveDirection, Time.deltaTime); 
            }
        }
    }
}

