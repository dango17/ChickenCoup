// Written by Liam Bansal
// Date Created: 30/01/2023

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 
/// </summary>
public class AudioSensor : Sensor {
	private NavMeshAgent navMeshAgent = null;

	protected override bool VerifyDetection(GameObject gameobject) {
		return TracePathToSound(gameobject.transform.position) ? true : false;
	}

	private void Start() {
		navMeshAgent = GameObject.FindGameObjectWithTag("Farmer").GetComponent<NavMeshAgent>();
	}

	private bool TracePathToSound(Vector3 soundsPosition) {
		// create max length for a path using the sounds volume
		float maximumPathLength = 0.0f;
		
		// is agent too far away
		if (Vector3.Distance(transform.position, soundsPosition) > maximumPathLength) {
			return false;
		}

		NavMeshPath pathToSound = null;
		// create path to sound
		navMeshAgent.CalculatePath(soundsPosition, pathToSound);

		// if path is too long...
		if (Farmer.PathLength(pathToSound) > maximumPathLength ||
			// ...or invalid.
			pathToSound.status != NavMeshPathStatus.PathComplete) {
			return false;
		}

		return true;
	}
}
