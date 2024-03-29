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
	/// <summary>
	/// Directions are stored in order of left then right.
	/// </summary>
	public Vector3[] FieldOfViewExtentsDirection {
		get { return fieldOfViewExtents; }
		private set { fieldOfViewExtents = value; }
	}

	[SerializeField, Range(0, maxFieldOfView), Tooltip("The viewing angle for the agent.")]
	private float fieldOfView = 200.0f;
	private const float maxFieldOfView = 360.0f;
	private Vector3[] fieldOfViewExtents = new Vector3[2];

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

	protected override void Update() {
		base.Update();
		DrawFieldOfViewExtents();
	}

	protected override void FixedUpdate() {
		VerifyDataIsValid();
	}

	private Visibility IsWithinLineOfSight(GameObject detectedGameObject) {
		DetectionPoint[] detectionPoints = detectedGameObject.GetComponentsInChildren<DetectionPoint>();

		if (detectionPoints.Length > 0) {
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
			// The percentage of detection points on an object that need to be 
			// visible for it to be detected.
			int detectionPercentage = 70;

			if (percentOfPointsDetected >= detectionPercentage) {
				return Visibility.Visible;
			} else if (percentOfPointsDetected > 0) {
				return Visibility.PartiallyVisible;
			} else {
				return Visibility.NotVisible;
			}
		} else if (RaycastHit(sensorOrigin.position,
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
			RaycastHit[] raycastHits = Physics.RaycastAll(raycastOrigin,
			(targetObject.transform.position - sensorOrigin.position).normalized,
			detectionRange,
			raycastLayer);
			raycastHits = raycastHits.OrderBy(hit => hit.distance).ToArray();

			foreach (RaycastHit hit in raycastHits) {
				Debug.DrawLine(raycastOrigin, hit.point, Color.cyan, 5.0f);
			}

			for (int i = 0; i < raycastHits.Length; ++i) {
				if (raycastHits[i].collider) {
					if (targetObject && raycastHits[i].collider.gameObject == targetObject) {
						return true;
					} else if (gameObjectToIgnore &&
						(raycastHits[i].collider.gameObject == gameObjectToIgnore)) {
						// Continue searching for the target game object
						// because the current raycast hit's game object is
						// set to be ignored.
						continue;
					} else if (raycastHits[i].collider.gameObject.GetComponent<DetectionPoint>()) {
						continue;
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

	/// <summary>
	/// Checks that all the collected data is still visible to the sensor.
	/// </summary>
	private void VerifyDataIsValid() {
		foreach (CollectedData collectedData in data) {
			if (VerifyDetection(collectedData.gameobject) == Visibility.NotVisible) {
				data.Remove(GetCollectedData(data, collectedData.gameobject));
				break;
			}

		}

		foreach (CollectedData collectedData in partiallyDiscoveredData) {
			if (VerifyDetection(collectedData.gameobject) == Visibility.NotVisible) {
				partiallyDiscoveredData.Remove(GetCollectedData(partiallyDiscoveredData, collectedData.gameobject));
				break;
			}
		}
	}

	private void DrawFieldOfViewExtents() {
		const float half = 0.5f;
		// Gets the desired angle to relative to the sensor's y axis.
		float rotationAmount = half * fieldOfView - transform.rotation.eulerAngles.y;
		// Gets the forward direction of the quaternion to rotate by.
		fieldOfViewExtents[0] = Quaternion.Euler(0, -rotationAmount, 0) * Vector3.forward;
		// Finds the direction, relative to the sensor's position, where the
		// field of view extents will be drawn along.
		// The detection range controls how far out the extents will be drawn.
		Vector3 fieldOfViewExtentPosition = transform.position + fieldOfViewExtents[0] * detectionRange;
		Debug.DrawLine(transform.position, fieldOfViewExtentPosition);
		rotationAmount = half * fieldOfView + transform.rotation.eulerAngles.y;
		fieldOfViewExtents[1] = Quaternion.Euler(0, rotationAmount, 0) * Vector3.forward;
		fieldOfViewExtentPosition = transform.position + fieldOfViewExtents[1] * detectionRange;
		Debug.DrawLine(transform.position, fieldOfViewExtentPosition);
	}
}
