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
    /// <summary>
    /// A direction towards the point, among the largest consecutive set of 
    /// points in open space, that's the furthest away from the farmer.
    /// Sometimes provides a better direction to move towards to reach open 
    /// space than the set's average direction.
    /// </summary>
    public Vector3 DirectionToFurthestPointInOpenSpace {
        get;
        private set;
    }

	[SerializeField, Tooltip("Set the order of wonder points starting with " +
		"North and going clockwise.")]
    private WonderPoint[] wonderPoints = new WonderPoint[8];
    private GameObject farmer = null;

    private void Awake() {
        wonderPoints = GetComponentsInChildren<WonderPoint>();
        DuplicateWonderPoints();
    }

	private void Start() {
        farmer = GameObject.FindGameObjectWithTag("Farmer");
    }

	private void FixedUpdate() {
        AverageDirectionToOpenSpace = Vector3.zero;
        int consecutiveEmptyPoints = 0;
        int highestConsecutiveEmptyPoints = 0;
        float furthestDistance = 0;
        GameObject furthestPoint = null;
        Vector3 averageDirectionToOpenSpace = Vector3.zero;

        foreach (WonderPoint wonderPoint in wonderPoints) {
            if (wonderPoint.IsTouching) {
                consecutiveEmptyPoints = 0;
                furthestDistance = 0;
                averageDirectionToOpenSpace = Vector3.zero;
                furthestPoint = null;
                continue;
            }

            ++consecutiveEmptyPoints;
            averageDirectionToOpenSpace += (wonderPoint.transform.position - transform.position).normalized;

            // Checks if the new set of consecutive points are greater in
            // length than the previous set.
            if (consecutiveEmptyPoints > highestConsecutiveEmptyPoints) {
                highestConsecutiveEmptyPoints = consecutiveEmptyPoints;
                AverageDirectionToOpenSpace = averageDirectionToOpenSpace.normalized;
                GetFurthestPoint(wonderPoint.gameObject);

                if (furthestPoint) {
                    DirectionToFurthestPointInOpenSpace = (furthestPoint.transform.position - transform.position).normalized;
                }
            } else if (consecutiveEmptyPoints == highestConsecutiveEmptyPoints) {
                float dotProductOfNewDirection = Vector3.Dot(farmer.transform.forward, averageDirectionToOpenSpace);
                float dotProductOfCurrentDirection = Vector3.Dot(farmer.transform.forward, AverageDirectionToOpenSpace);

                // Finds which set of consecuutive wonder points are closest
                // in direction to the farmer's forward direction.
                if (dotProductOfCurrentDirection < dotProductOfNewDirection) {
                    AverageDirectionToOpenSpace = averageDirectionToOpenSpace.normalized;
                    GetFurthestPoint(wonderPoint.gameObject);

                    if (furthestPoint) {
                        DirectionToFurthestPointInOpenSpace = (furthestPoint.transform.position - transform.position).normalized;
                    }
                }
			}
        }

        Debug.DrawLine(transform.position, transform.position + AverageDirectionToOpenSpace * 2);
        Debug.DrawLine(transform.position, transform.position + DirectionToFurthestPointInOpenSpace * 2);

        void GetFurthestPoint(GameObject point) {
            float distanceToPoint = Vector3.Distance(transform.position, point.transform.position);

            if (distanceToPoint > furthestDistance) {
                furthestDistance = distanceToPoint;
                furthestPoint = point;
            }
        }
    }

    /// <summary>
    /// Duplicates all elements contained within the wonder points array.
    /// </summary>
    private void DuplicateWonderPoints() {
        WonderPoint[] wonderPointsCopy = wonderPoints;
        wonderPoints = new WonderPoint[wonderPoints.Length * 2];
        wonderPointsCopy.CopyTo(wonderPoints, 0);
        wonderPointsCopy.CopyTo(wonderPoints, wonderPointsCopy.Length);
    }
}
