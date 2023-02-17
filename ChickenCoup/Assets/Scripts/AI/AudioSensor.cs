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
		AudioSource soundSource = gameobject.GetComponent<AudioSource>();
		return soundSource && TracePathToSound(gameobject.transform.position, soundSource.maxDistance) ? true : false;
	}

	private void Start() {
		navMeshAgent = GameObject.FindGameObjectWithTag("Farmer").GetComponent<NavMeshAgent>();
	}

	private bool TracePathToSound(Vector3 soundsPosition, float maximumPathLength) {
		if (Vector3.Distance(navMeshAgent.gameObject.transform.position, soundsPosition) > maximumPathLength) {
			return false;
		}

		NavMeshPath pathToSound = null;
		// Calculates a path from the agent to the sound.
		navMeshAgent.CalculatePath(soundsPosition, pathToSound);

		if (Farmer.PathLength(pathToSound) > maximumPathLength ||
			pathToSound.status != NavMeshPathStatus.PathComplete) {
			return false;
		}

		return true;
	}
}
