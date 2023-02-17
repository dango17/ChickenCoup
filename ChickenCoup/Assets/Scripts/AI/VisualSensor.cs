// Written by Liam Bansal
// Date Created: 30/01/2023

using UnityEngine;

/// <summary>
/// Utilizes the sensor class to create a visual sensor that checks for a clear 
/// line of sight to game objects after detecting them.
/// </summary>
public class VisualSensor : Sensor {
	/// <summary>
	/// Effectively checks for a line of sight to the object.
	/// </summary>
	/// <returns> True if there's a clear line of sight to the object. </returns>
	protected override bool VerifyDetection(GameObject gameobject) {
		return CheckLineOfSight(gameobject);
	}

	private bool CheckLineOfSight(GameObject gameobject) {
		Physics.Raycast(sensorOrigin.position,
			gameobject.transform.position - sensorOrigin.position,
			out RaycastHit raycastHit,
			detectionRange);

		return raycastHit.collider && raycastHit.collider.gameObject == gameobject ? true : false;
	}
}
