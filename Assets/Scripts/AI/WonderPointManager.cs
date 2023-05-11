// Author: Liam Bansal
// Collaborator: N/A
// Created On: 9/5/2023

using UnityEngine;

/// <summary>
/// Collects data about the wonder points.
/// </summary>
public class WonderPointManager : MonoBehaviour {
    /// <summary>
    /// The average direction towards the largest set of consecutive wonder 
    /// points that aren't triggering a collisions.
    /// </summary>
    public Vector3 AverageDirectionToOpenSpace {
        get;
        private set;
    }

	[SerializeField, Tooltip("Set the order of wonder points starting with " +
		"North and going clockwise.")]
    private WonderPoint[] wonderPoints = new WonderPoint[16];

    private GameObject farmer = null;

    private void Awake() {
        wonderPoints = GetComponentsInChildren<WonderPoint>();
    }

	private void Start() {
        farmer = GameObject.FindGameObjectWithTag("Farmer");
    }

	private void FixedUpdate() {
        AverageDirectionToOpenSpace = Vector3.zero;
        int consecutiveEmptyPoints = 0;
        int highestConsecutiveEmptyPoints = 0;
        Vector3 averageDirectionToOpenSpace = Vector3.zero;

        foreach (WonderPoint wonderPoint in wonderPoints) {
            if (wonderPoint.IsTouching) {
                consecutiveEmptyPoints = 0;
                averageDirectionToOpenSpace = Vector3.zero;
                continue;
            }

            ++consecutiveEmptyPoints;
            averageDirectionToOpenSpace += (wonderPoint.transform.position - transform.position).normalized;

            // Checks if the new set of consecutive points are greater in
            // length than the previous set.
            if (consecutiveEmptyPoints > highestConsecutiveEmptyPoints) {
                highestConsecutiveEmptyPoints = consecutiveEmptyPoints;
                AverageDirectionToOpenSpace = averageDirectionToOpenSpace;
            } else if (consecutiveEmptyPoints == highestConsecutiveEmptyPoints) {
                float dotProductOfNewDirection = Vector3.Dot(farmer.transform.forward, averageDirectionToOpenSpace);
                float dotProductOfCurrentDirection = Vector3.Dot(farmer.transform.forward, AverageDirectionToOpenSpace);

                // Finds which set of consecuutive wonder points are closest
                // in direction to the farmer's forward direction.
                if (dotProductOfCurrentDirection < dotProductOfNewDirection) {
                    AverageDirectionToOpenSpace = averageDirectionToOpenSpace;
				}
			}
        }

        Debug.DrawLine(transform.position, transform.position + AverageDirectionToOpenSpace * 2, Color.red, 2.0f);
    }
}
