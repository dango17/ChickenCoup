// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 02/05/2023

using UnityEngine;

public class PotBreak : MonoBehaviour
{
    public float breakHeight = 3.0f;
    public bool isBroken = false;
    public Vector3 initialPosition;

    [SerializeField] private GameObject crackedPot;
    [SerializeField] private GameObject pot;
    [SerializeField] private BoxCollider boxCollider;

    public void Start()
    {
        // Store the initial position of the pot
        initialPosition = transform.position;
    }

    public void Update()
    {
        // Calculate the fall height of the pot
        float fallHeight = initialPosition.y - transform.position.y;

        if (!isBroken && fallHeight >= breakHeight)
        {
            BreakOpen();
        }
    }

    public void BreakOpen()
    {
        pot.SetActive(false);
        crackedPot.SetActive(true);

        Vector3 crackedPotPosition = transform.position - new Vector3(0, 0.1f, 0);
        crackedPot.transform.position = crackedPotPosition;
        crackedPot.transform.rotation = transform.rotation;

        //Disable the rigidbody
        Rigidbody currentRigidbody = GetComponent<Rigidbody>();
        if (currentRigidbody != null)
        {
            currentRigidbody.isKinematic = true;
        }
        //Disable the box collider
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        isBroken = true;
    }
}