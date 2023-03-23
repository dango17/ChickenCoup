// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Utilizes the sensor class to create an audio sensor that can only detect 
/// nearby objects that play sounds.
/// Executes prior to SoundCue so components are found prior to script being 
/// used.
/// </summary>
public class AudioSensor : Sensor {
	private NavMeshAgent navMeshAgent = null;
	private NavMeshPath pathToSound = null;

	/// <summary>
	/// Notifies all nearby audio sensors that a sound was played.
	/// Always call anytime a sound is played!
	/// </summary>
	public static void NotifyNearbyAudioSensors(AudioSource audioSource, Vector3 soundPosition) {
		if (!audioSource) {
			return;
		}

		const float half = 0.5f;
		float sphereRadius = audioSource.maxDistance * half;
		int farmerLayer = 1 << LayerMask.NameToLayer("Farmer");
		Collider[] intersectingColliders = Physics.OverlapSphere(soundPosition,
			sphereRadius,
			farmerLayer,
			QueryTriggerInteraction.Collide);

		foreach (Collider collider in intersectingColliders) {
			AudioSensor audioSensor = collider.GetComponent<AudioSensor>();

			if (audioSensor) {
				audioSensor.ObjectDetected(audioSource.gameObject);
			}
		}
	}

	protected override Visibility VerifyDetection(GameObject gameobject) {
		AudioSource soundSource = gameobject.GetComponent<AudioSource>();
		return soundSource && TracePathToSound(gameobject.transform.position,
			soundSource.maxDistance) ? Visibility.Visible : Visibility.NotVisible;
	}

	private void Awake() {
		pathToSound = new NavMeshPath();
	}

	private void Start() {
		navMeshAgent = transform.parent.GetComponent<NavMeshAgent>();
	}

	private bool TracePathToSound(Vector3 soundsPosition, float maximumPathLength) {
		if (!navMeshAgent ||
			Vector3.Distance(navMeshAgent.gameObject.transform.position, soundsPosition) > maximumPathLength) {
			return false;
		}

		float doubleAgentsHeight = navMeshAgent.height * 2;
		
		if (!NavMesh.SamplePosition(soundsPosition,
			out NavMeshHit navMeshHit,
			doubleAgentsHeight,
			NavMesh.AllAreas) &&
			navMeshHit.hit) {
			// Return false because no nearby position on the nav mesh was found.
			return false;
		}

		// Clear any previous paths prior to every calculation.
		pathToSound.ClearCorners();
		// Calculates a path from the agent to the sounds closest nav mesh
		// position.
		navMeshAgent.CalculatePath(navMeshHit.position, pathToSound);

		if (Farmer.PathLength(pathToSound) > maximumPathLength ||
			pathToSound.status != NavMeshPathStatus.PathComplete) {
			return false;
		}

		return true;
	}
}
