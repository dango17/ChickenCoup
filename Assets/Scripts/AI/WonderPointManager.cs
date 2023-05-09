// Author: Liam Bansal
// Collaborator: N/A
// Created On: 9/5/2023

using UnityEngine;

/// <summary>
/// Collects data about the wonder points.
/// </summary>
public class WonderPointManager : MonoBehaviour {
    /// <summary>
    /// The sum of directions to all wonder points that are not triggering a 
    /// collision.
    /// </summary>
    public Vector3 AverageDirectionToOpenSpace {
        get;
        private set;
    }

    private WonderPoint[] wonderPoints = new WonderPoint[8];

    private void Awake() {
        wonderPoints = GetComponentsInChildren<WonderPoint>();
    }

    private void FixedUpdate() {
        AverageDirectionToOpenSpace = Vector3.zero;

        foreach (WonderPoint wonderPoint in wonderPoints) {
            if (!wonderPoint.IsTouching) {
                AverageDirectionToOpenSpace += (wonderPoint.transform.position - transform.position).normalized;
            }
        }
    }
}
