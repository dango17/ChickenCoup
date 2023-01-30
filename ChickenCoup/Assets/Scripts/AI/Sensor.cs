// Written by Liam Bansal
// Date Created: 30/01/2023

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gathers data about an agents surrounding environment.
/// </summary>
public abstract class Sensor : MonoBehaviour {
	[SerializeField, Tooltip("Add a layer to the list to detect objects on that layer.")]
	protected LayerMask[] detectionLayers = default;
	/// <summary>
	/// Stores references to the game objects the agent has gathered data about.
	/// </summary>
	protected LinkedList<GameObject> data = new LinkedList<GameObject>();

	[SerializeField, Tooltip("Data will only be gathered within this range " +
		"i.e. it's the view distance, hearing range etc.")]
	protected int detectionRange = 5;

	private void Awake() {
		SphereCollider sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.radius = detectionRange;
	}

	private void OnTriggerEnter(Collider other) {
		ObjectDetected(other);
	}

	private void OnTriggerStay(Collider other) {
		ObjectDetected(other);
	}

	private void OnTriggerExit(Collider other) {
		if (data.Contains(other.gameObject)) {
			data.Remove(other.gameObject);
		}
	}

	private void ObjectDetected(Collider colliderDetected) {
		// Check if object is placed within one of the detection layers.
		if (!detectionLayers.Contains(1 << colliderDetected.gameObject.layer) ||
			data.Contains(colliderDetected.gameObject)) {
			return;
		}

		data.AddLast(colliderDetected.gameObject);
	}
}
