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
	/// Checks if the object is visible to the detector.
	/// </summary>
	/// <returns> True if the game object should be detected. </returns>
	protected override bool VerifyDetection(GameObject detectedGameObject) {
		return IsWithinFieldOfView(detectedGameObject) && IsWithinLineOfSight(detectedGameObject);
	}

	private bool IsWithinLineOfSight(GameObject gameobject) {
		Physics.Raycast(sensorOrigin.position,
			gameobject.transform.position - sensorOrigin.position,
			out RaycastHit raycastHit,
			detectionRange);

		return raycastHit.collider && raycastHit.collider.gameObject == gameobject ? true : false;
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
