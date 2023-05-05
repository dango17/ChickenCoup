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
	public float FieldOfView {
		get { return fieldOfView; }
		private set { fieldOfView = value; }
	}

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
	protected override Visibility VerifyDetection(GameObject detectedGameObject) {
		if (IsWithinFieldOfView(detectedGameObject)) {
			return IsWithinLineOfSight(detectedGameObject);
		}

		return Visibility.NotVisible;
	}

	private Visibility IsWithinLineOfSight(GameObject detectedGameObject) {
		DetectionPoint[] detectionPoints = detectedGameObject.GetComponentsInChildren<DetectionPoint>();

		/*if (detectionPoints.Length > 0) {
			float pointsDetected = 0;

			// Check how many of the object's detection points are visible.
			foreach (DetectionPoint detectionPoint in detectionPoints) {
				if (RaycastHit(sensorOrigin.position,
					// Append the detection point layer onto the raycast,
					// so they can be found.
					visibleLayer | LayerMask.GetMask("DetectionPoints"),
					detectionPoint.gameObject,
					detectedGameObject)) {
					detectionPoint.IsVisible(true);
					++pointsDetected;
				} else {
					detectionPoint.IsVisible(false);
				}
			}

			const int oneHundred = 100;
			float percentOfPointsDetected = pointsDetected / detectionPoints.Length * oneHundred;

			if (percentOfPointsDetected >= detectionPercentage) {
				return Visibility.Visible;
			} else if (percentOfPointsDetected > 0) {
				return Visibility.PartiallyVisible;
			} else {
				return Visibility.NotVisible;
			}
		} else */if (RaycastHit(sensorOrigin.position,
			visibleLayer,
			detectedGameObject,
			null)) {
			return Visibility.Visible;
		}

		return Visibility.NotVisible;

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
			RaycastHit[] raycastHits = new RaycastHit[5];
			Physics.RaycastNonAlloc(raycastOrigin,
			targetObject.transform.position - sensorOrigin.position,
			raycastHits,
			detectionRange,
			raycastLayer);
			raycastHits.OrderBy(hit => hit.distance);

			foreach (RaycastHit hit in raycastHits) {
				Debug.DrawLine(raycastOrigin, hit.point, Color.cyan, 5.0f);
			}

			for (int i = 0; i < raycastHits.Length; ++i) {
				if (raycastHits[i].collider) {
					if (targetObject && raycastHits[i].collider.gameObject == targetObject) {
						return true;
					} else if (gameObjectToIgnore && raycastHits[i].collider.gameObject == gameObjectToIgnore) {
						// Continue searching for the target game object
						// because the current raycast hit's game object is
						// set to be ignored.
						continue;
					} else if (raycastHits[i].collider.gameObject.GetComponent<DetectionPoint>()) {
						// Continue raycasting for the target game object from the
						// intersected detection point's position because
						// detection points shouldn't prevent raycasts from
						// passing through.
						return RaycastHit(raycastHits[i].point,
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
