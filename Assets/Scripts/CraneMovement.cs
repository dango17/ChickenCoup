using UnityEngine;
using System.Collections; 

public class CraneMovement : MonoBehaviour
{
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float delay = 1f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool playerOnPlatform = false;
    private bool movingUp = true;

    private void Start()
    {
        startPosition = transform.position;
        endPosition = transform.position + Vector3.up * moveDistance;
    }

    private void FixedUpdate()
    {
        if (playerOnPlatform)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, movingUp ? endPosition : startPosition, step);

            if (transform.position == endPosition && !movingUp)
            {
                playerOnPlatform = false;
            }

            if (transform.position == (movingUp ? endPosition : startPosition))
            {
                if (movingUp)
                {
                    playerOnPlatform = true;
                }
                else
                {
                    movingUp = !movingUp;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(StartMovingAfterDelay());
        }
    }

    private IEnumerator StartMovingAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        playerOnPlatform = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (movingUp && transform.position == endPosition)
            {
                movingUp = false;
            }
            else
            {
                playerOnPlatform = false;
                movingUp = false;
            }
        }
    }
}