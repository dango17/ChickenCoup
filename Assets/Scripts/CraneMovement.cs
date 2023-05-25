// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 22/04/2023

using UnityEngine;
using System.Collections;

namespace DO
{
    public class CraneMovement : MonoBehaviour
    {
        [SerializeField] private float moveDistance = 2f;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float delay = 1f;

        private Vector3 startPosition;
        private Vector3 endPosition;
        private bool playerOnPlatform = false;
        private bool movingUp = true;
        private bool isMoving = false;

        private void Start()
        {
            startPosition = transform.position;
            endPosition = transform.position + Vector3.up * moveDistance;
        }

        private void FixedUpdate()
        {
            if (isMoving)
            {
                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, movingUp ? endPosition : startPosition, step);

                if (transform.position == (movingUp ? endPosition : startPosition))
                {
                    if (movingUp)
                    {
                        StartCoroutine(WaitForDelay());
                    }
                    else
                    {
                        isMoving = false;
                        movingUp = !movingUp;
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isMoving)
            {
                StartCoroutine(StartMovingAfterDelay());
            }
        }

        private IEnumerator StartMovingAfterDelay()
        {
            yield return new WaitForSeconds(delay);
            isMoving = true;
        }

        private IEnumerator WaitForDelay()
        {
            yield return new WaitForSeconds(delay);
            isMoving = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (movingUp && transform.position == endPosition)
                {
                    movingUp = false;
                    isMoving = true;
                }
                else
                {
                    playerOnPlatform = false;
                    isMoving = false;
                    movingUp = false;
                }
            }
        }
    }
}