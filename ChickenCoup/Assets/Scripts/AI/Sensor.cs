// Written by Liam Bansal
// Date Created: 30/01/2023

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gathers data about an agents surrounding environment.
/// </summary>
public abstract class Sensor : MonoBehaviour {
	public LinkedList<GameObject> Data {
		get {
			return data;
		}
		private set {
			data = value;
		}
	}

	[SerializeField, Tooltip("Data will only be gathered within this range " +
		"i.e. it's the view distance, hearing range etc.")]
	protected int detectionRange = 5;
	[SerializeField, Tooltip("Select additional layers to detect objects on that layer.")]
	protected LayerMask detectionLayer = default;
	/// <summary>
	/// Stores references to the game objects the agent has gathered data about.
	/// </summary>
	protected LinkedList<GameObject> data = new LinkedList<GameObject>();
	[SerializeField, Tooltip("The location where raycasts will originate from, " +
		"when checking for line of sight on objects.")]
	protected Transform sensorOrigin = null;

	/// <summary>
	/// Checks if the gathered data is valid and should be saved.
	/// </summary>
	/// <param name="gameobject"> Gathered data that needs verifying. </param>
	/// <returns> True if the data is valid and should be saved. </returns>
	protected abstract bool VerifyDetection(GameObject gameobject);

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
		int bitshiftedLayer = 1 << colliderDetected.gameObject.layer;

		// Check if the data should be saved.
		if (detectionLayer != bitshiftedLayer ||
			data.Contains(colliderDetected.gameObject) ||
			!VerifyDetection(colliderDetected.gameObject)) {
			return;
		}

		data.AddLast(colliderDetected.gameObject);
	}
}
