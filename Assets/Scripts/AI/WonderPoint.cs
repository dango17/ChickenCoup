// Author: Liam Bansal
// Collaborator: N/A
// Created On: 9/5/2023 

using UnityEngine;

/// <summary>
/// Controls a trigger collider used for detecting geometry when the farmer AI 
/// is executing their wondering action.
/// </summary>
public class WonderPoint : MonoBehaviour {
    public bool IsTouching {
        get { return isTouching; }
        private set { isTouching = value; }
    }

    private bool isTouching = false;
    [SerializeField, Tooltip("How quickly the object moves away from or towards " +
        "the farmer.")]
    private int moveSpeed = 3;
    [SerializeField, Tooltip("The maximum range between the wonder point and " +
        "farmer.")]
    private int displacement = 3;
    private float distanceFromFarmerToFurthestPoint = 0.0f;
    private float distanceToFurthestPoint = 0.0f;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 furthestPosition = Vector3.zero;
    private Vector3 verticalOffset = Vector3.zero;
    [SerializeField, Tooltip("Mark any layers that should trigger collisions " +
        "with this game-object.")]
    private LayerMask collisionLayers = default;
    private Rigidbody rigidBody = null;
    private BoxCollider boxCollider = null;
    private Farmer farmer = null;

    private void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        verticalOffset = Vector3.up * boxCollider.bounds.size.y;
    }

    private void Start() {
        farmer = GameObject.FindGameObjectWithTag("Farmer").GetComponent<Farmer>();
        moveDirection = new Vector3(transform.position.x - farmer.transform.position.x,
            0,
            transform.position.z - farmer.transform.position.z).normalized;
        furthestPosition = farmer.transform.position + moveDirection * displacement + verticalOffset;
        distanceFromFarmerToFurthestPoint = Mathf.Clamp(Vector3.Distance(farmer.transform.position + verticalOffset, furthestPosition),
            0.0f,
            displacement);
        transform.position = farmer.transform.position + verticalOffset;
        distanceToFurthestPoint = Mathf.Clamp(Vector3.Distance(transform.position, furthestPosition),
            0.0f,
            displacement);
    }

    private void Update() {
        furthestPosition = farmer.transform.position + moveDirection * displacement + verticalOffset;
        distanceFromFarmerToFurthestPoint = Mathf.Clamp(Vector3.Distance(farmer.transform.position + verticalOffset, furthestPosition),
            0.0f,
            displacement);
        distanceToFurthestPoint = Mathf.Clamp(Vector3.Distance(transform.position, furthestPosition),
            0.0f,
            displacement);
        float moveDistance = distanceToFurthestPoint / distanceFromFarmerToFurthestPoint;
        moveDistance = Mathf.Clamp(moveDistance, 0.0f, 1.0f);

        transform.position = new Vector3(Mathf.Lerp(farmer.transform.position.x,
                furthestPosition.x,
                moveDistance),
            farmer.transform.position.y + boxCollider.bounds.size.y,
            Mathf.Lerp(farmer.transform.position.z,
                furthestPosition.z,
                moveDistance));
        
        distanceToFurthestPoint = Mathf.Clamp(Vector3.Distance(transform.position, furthestPosition),
            0.0f,
            displacement);
    }

    private void OnTriggerEnter(Collider other) {
        TriggeredCollision(other, true);
    }

    private void OnTriggerStay(Collider other) {
        TriggeredCollision(other, true);
    }

    private void OnTriggerExit(Collider other) {
        TriggeredCollision(other, false);
    }

    private void TriggeredCollision(Collider collision, bool enteringCollision) {
        int bitshiftedLayerValue = 1 << collision.gameObject.layer;

        if ((bitshiftedLayerValue & collisionLayers) == 0) {
            return;
        }

        if (enteringCollision) {
            isTouching = true;
        } else {
            isTouching = false;
        }
    }
}
