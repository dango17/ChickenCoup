// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

using UnityEngine;
using System.Linq;

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
				if (RaycastHit(sensorOrigin.position,
					detectionLayer,
					detectionPoint.gameObject,
					detectedGameObject)) {
					++pointsDetected;
				}
			}

			const int oneHundred = 100;
			float percentOfPointsDetected = pointsDetected / detectionPoints.Length * oneHundred;
			return percentOfPointsDetected >= detectionPercentage ? true : false;
		} else if (RaycastHit(sensorOrigin.position,
			detectionLayer,
			detectedGameObject,
			null)) {
			return true;
		}

		return false;

		/// <summary>
		/// Sends a ray along a desired direction to check for a clear line of 
		/// sight to a specific game object.
		/// </summary>
		/// <param name="raycastOrigin"> Position where the raycast spawns from. </param>
		/// <param name="raycastLayer"> Only objects on this layer will trigger a raycast hit. </param>
		/// <param name="targetObject"> The raycast checks for a clear line of sight to this game object. </param>
		/// <param name="gameObjectToIgnore"> A singular game object that should be ignored by the raycast. </param>
		/// <returns> True if there's a clear line of sight to the target game object. </returns>
		bool RaycastHit(Vector3 raycastOrigin,
			LayerMask raycastLayer,
			GameObject targetObject,
			GameObject gameObjectToIgnore) {
			RaycastHit[] raycastHit = new RaycastHit[2];
			Physics.RaycastNonAlloc(raycastOrigin,
			targetObject.transform.position - sensorOrigin.position,
			raycastHit,
			detectionRange);
			raycastHit.OrderBy(hit => hit.distance);

			for (int i = 0; i < raycastHit.Length; ++i) {
				if (raycastHit[i].collider) {
					if (targetObject && raycastHit[i].collider.gameObject == targetObject) {
						return true;
					} else if (gameObjectToIgnore && raycastHit[i].collider.gameObject == gameObjectToIgnore) {
						// Continue searching for the target game object
						// because the current raycast hit's game object is
						// set to be ignored.
						continue;
					} else if (raycastHit[i].collider.gameObject.GetComponent<DetectionPoint>()) {
						// Continue raycasting for the target game object from the
						// intersected detection point's position because
						// detection points shouldn't prevent raycasts from
						// passing through.
						return RaycastHit(raycastHit[i].collider.transform.position,
							raycastLayer,
							targetObject,
							gameObjectToIgnore);
					} else {
						// Return false because an object that is not see through was hit.
						return false;
					}
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
