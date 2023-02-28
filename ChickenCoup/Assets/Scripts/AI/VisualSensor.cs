// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

using UnityEngine;

/// <summary>
/// Utilizes the sensor class to create a visual sensor that checks if an game
/// object is within an agent's field of view and there's a clear line of sight.
/// </summary>
public class VisualSensor : Sensor {
	[SerializeField, Range(0, maxFieldOfView), Tooltip("The viewing angle for the agent.")]
	private float fieldOfView = 200.0f;
	private const float maxFieldOfView = 360.0f;
	/// <summary>
	/// The percentage of detection points on an object that need to be 
	/// visible for it to be detected.
	/// </summary>
	private int detectionPercentage = 70;

	/// <summary>
	/// Checks if the object is visible to the detector.
	/// </summary>
	/// <returns> True if the game object should be detected. </returns>
	protected override bool VerifyDetection(GameObject detectedGameObject) {
		return IsWithinFieldOfView(detectedGameObject) && IsWithinLineOfSight(detectedGameObject);
	}

	private bool IsWithinLineOfSight(GameObject detectedGameObject) {
		DetectionPoint[] detectionPoints = detectedGameObject.GetComponentsInChildren<DetectionPoint>();

		if (detectionPoints.Length > 0) {
			float pointsDetected = 0;

			// Check how many of the object's detection points are visible.
			foreach (DetectionPoint detectionPoint in detectionPoints) {
				// TODO: Exclude the target object from any subsuquent raycasts after detecting it.
				if (RaycastHit(sensorOrigin.position, detectionLayer, detectionPoint.gameObject)) {
					++pointsDetected;
				}
			}

			const int oneHundred = 100;
			float percentOfPointsDetected = pointsDetected / detectionPoints.Length * oneHundred;
			return percentOfPointsDetected >= detectionPercentage ? true : false;
		} else if (RaycastHit(sensorOrigin.position, detectionLayer, detectedGameObject)) {
			return true;
		}

		return false;

		bool RaycastHit(Vector3 raycastOrigin, LayerMask raycastLayer, GameObject targetObject) {
			Physics.Raycast(raycastOrigin,
			targetObject.transform.position - sensorOrigin.position,
			out RaycastHit raycastHit,
			detectionRange);

			if (raycastHit.collider) {
				if (raycastHit.collider.gameObject == targetObject) {
					return true;
				} else if (raycastHit.collider.gameObject.GetComponent<DetectionPoint>()) {
					// Continue raycasting for the target game object from the
					// intersected detection point's position because a targeted
					// detection point could be obscured by others.
					return RaycastHit(raycastHit.collider.transform.position,
						raycastLayer,
						targetObject);
				}
			}

			return false;
		}
	}

	private bool IsWithinFieldOfView(GameObject detectedGameObject) {
		Vector3 directionToGameObject = (detectedGameObject.transform.position - transform.position).normalized;
		float dotProduct = Vector3.Dot(transform.forward, directionToGameObject);
		// Represents the agent's field of view between a value of -1 and 1.
		float scaledFieldOfView = -((fieldOfView / maxFieldOfView) - ((maxFieldOfView - fieldOfView) / maxFieldOfView));

		if (dotProduct >= scaledFieldOfView) {
			return true;
		}

		return false;
	}
}
