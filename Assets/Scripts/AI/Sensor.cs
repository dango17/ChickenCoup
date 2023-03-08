// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uses a trigger collider to detect game-objects of a certain type.
/// </summary>
public abstract class Sensor : MonoBehaviour {
	public enum Visibility {
		Visible,
		PartiallyVisible,
		NotVisible
	}

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
	protected LinkedList<GameObject> partiallyDiscoveredData = new LinkedList<GameObject>();
	[SerializeField, Tooltip("The location where raycasts will originate from, " +
		"when checking for line of sight on objects.")]
	protected Transform sensorOrigin = null;

	/// <summary>
	/// Notifies the sensor that a game-object was detected.
	/// </summary>
	/// <param name="detectedGameObject"> The detected game-object. </param>
	public void ObjectDetected(GameObject detectedGameObject) {
		int detectedGameObjectsLayer = 1 << detectedGameObject.gameObject.layer;

		// Check if the data should be saved.
		if ((detectionLayer & detectedGameObjectsLayer) != 0) {
			bool discovered = data.Contains(detectedGameObject.transform.root.gameObject);
			bool partiallyDiscovered = partiallyDiscoveredData.Contains(detectedGameObject.transform.root.gameObject);

			// Only verify detection if the data is not already discovered or
			// is only partilly discovered.
			if (!discovered || (!discovered && !partiallyDiscovered)) {
				switch (VerifyDetection(detectedGameObject.gameObject)) {
					case Visibility.Visible: {
						if (!discovered) {
							data.AddLast(detectedGameObject.transform.root.gameObject);
						}

						if (partiallyDiscovered) {
							partiallyDiscoveredData.Remove(detectedGameObject.transform.root.gameObject);
						}

						break;
					}
					case Visibility.PartiallyVisible: {
						if (!partiallyDiscovered) {
							partiallyDiscoveredData.AddLast(detectedGameObject.transform.root.gameObject);
						}

						break;
					}
					case Visibility.NotVisible: {
						return;
					}
					default: {
						return;
					}
				}
			}
		}
	}

	/// <summary>
	/// Checks if the gathered data is valid and should be saved.
	/// </summary>
	/// <param name="gameobject"> Gathered data that needs verifying. </param>
	/// <returns> True if the data is valid and should be saved. </returns>
	protected abstract Visibility VerifyDetection(GameObject gameobject);

	private void Awake() {
		SphereCollider sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.radius = detectionRange;
	}

	private void OnTriggerEnter(Collider other) {
		ObjectDetected(other.gameObject);
	}

	private void OnTriggerStay(Collider other) {
		ObjectDetected(other.gameObject);
	}

	private void OnTriggerExit(Collider other) {
		if (data.Contains(other.gameObject)) {
			data.Remove(other.gameObject);
		}
	}
}
