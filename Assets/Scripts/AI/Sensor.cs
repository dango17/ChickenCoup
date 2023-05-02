// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Uses a trigger collider to detect game-objects of a certain type.
/// </summary>
public abstract class Sensor : MonoBehaviour {
	public struct CollectedData {
		public static float defaultLifespan = 10.0f;
		public float lifespan;
		public GameObject gameobject;

		public CollectedData (GameObject gameobject, float lifespan) {
			this.gameobject = gameobject;
			this.lifespan = lifespan;
        }
	}

	public enum Visibility {
		Visible,
		PartiallyVisible,
		NotVisible
	}

	public LinkedList<CollectedData> Data {
		get {
			return data;
		}
		private set {
			data = value;
		}
	}

    [SerializeField, Tooltip("Toggle true if the gathered data should " +
        "automatically be forgotten after 10 seconds.")]
	protected bool forgetDataAfterDelay = false;
	[SerializeField, Tooltip("Data will only be gathered within this range " +
		"i.e. it's the view distance, hearing range etc.")]
	protected int detectionRange = 5;
	[SerializeField, Tooltip("The AI can only see objects on this layer.")]
	protected LayerMask visibleLayer = default;
	[SerializeField, Tooltip("The AI will only remember objects on this layer.")]
	protected LayerMask detectionLayer = default;
	/// <summary>
	/// Stores references to the game objects the agent has gathered data about.
	/// </summary>
	protected LinkedList<CollectedData> data = new LinkedList<CollectedData>();
	protected LinkedList<CollectedData> partiallyDiscoveredData = new LinkedList<CollectedData>();
	[SerializeField, Tooltip("The location where raycasts will originate from, " +
		"when checking for line of sight on objects.")]
	protected Transform sensorOrigin = null;

    #region Helper Methods
    public bool Contains(LinkedList<CollectedData> collection, GameObject gameobjectToSelect) {
		return collection.Count == 0 ? false : collection.Contains(GetCollectedData(collection, gameobjectToSelect));
	}

	public void AddObject(ref LinkedList<CollectedData> collection, GameObject objectToAdd) {
		if (!Contains(collection, objectToAdd)) {
			collection.AddLast(new CollectedData(objectToAdd, CollectedData.defaultLifespan));
		}
	}

	public void RemoveObject(ref LinkedList<CollectedData> collection, GameObject objectToAdd) {
		if (Contains(collection, objectToAdd)) {
			collection.Remove(GetCollectedData(collection, objectToAdd));
		}
	}
	#endregion

	/// <summary>
	/// Notifies the sensor that a game-object was detected.
	/// </summary>
	/// <param name="detectedGameObject"> The detected game-object. </param>
	public void ObjectDetected(GameObject detectedGameObject) {
		int detectedGameObjectsLayer = 1 << detectedGameObject.layer;

		// Check if the game-object exists on the layers for visible and
		// memorable game-objects to the farmer.
		if (((visibleLayer & detectedGameObjectsLayer) == 0) &&
			((detectionLayer & detectedGameObjectsLayer) == 0)) {
			return;
		}

		bool discovered = Contains(data, detectedGameObject.transform.root.gameObject);
		bool partiallyDiscovered = Contains(partiallyDiscoveredData, detectedGameObject.transform.root.gameObject);

		// Don't verify detection if the data is already discovered or
		// partially discovered. Include "discovered &&" because the data
		// first needs to change to fully discovered before being ignored, and
		// using "partiallyDiscovered" by itself doesn't work.
		if (discovered || (discovered && partiallyDiscovered)) {
			return;
		}

		switch (VerifyDetection(detectedGameObject)) {
			case Visibility.Visible: {
				if (!discovered) {
					AddObject(ref data, detectedGameObject.transform.root.gameObject);
				}

				if (partiallyDiscovered) {
					RemoveObject(ref partiallyDiscoveredData, detectedGameObject.transform.root.gameObject);
				}

				break;
			}
			case Visibility.PartiallyVisible: {
				if (!partiallyDiscovered) {
					AddObject(ref partiallyDiscoveredData, detectedGameObject.transform.root.gameObject);
				}

				break;
			}
			case Visibility.NotVisible: {
				ForgetObject(detectedGameObject.transform.root.gameObject);
				return;
			}
			default: {
				ForgetObject(detectedGameObject.transform.root.gameObject);
				return;
			}
		}
	}

	/// <summary>
	/// Makes the sensor forget about a specific bit of data.
	/// </summary>
	/// <param name="gameObjectToForget"> The game-object to forget. Must be the root game-object. </param>
	public void ForgetObject(GameObject gameObjectToForget) {
		RemoveObject(ref data, gameObjectToForget);
		RemoveObject(ref partiallyDiscoveredData, gameObjectToForget);
		DisableObjectsDetectionPoints(gameObjectToForget);
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

    private void Update() {
		if (forgetDataAfterDelay) {
			for (LinkedListNode<CollectedData> iterator = data.First;
				iterator != data.Last;
				iterator = iterator.Next) {
				CollectedData collectedData = iterator.Value;
				collectedData.lifespan -= Time.deltaTime;

				if (collectedData.lifespan <= 0) {
					LinkedListNode<CollectedData> previousValue = iterator.Previous != null ? iterator.Previous : default;
					data.Remove(iterator);
					iterator = previousValue;
				} else {
					iterator.Value = collectedData;
                }
			}

			for (LinkedListNode<CollectedData> iterator = partiallyDiscoveredData.First;
				iterator != partiallyDiscoveredData.Last;
				iterator = iterator.Next) {
				CollectedData collectedData = iterator.Value;
				collectedData.lifespan -= Time.deltaTime;

				if (collectedData.lifespan <= 0) {
					LinkedListNode<CollectedData> previousValue = iterator.Previous != null ? iterator.Previous : default;
					partiallyDiscoveredData.Remove(iterator);
					iterator = previousValue;
				} else {
					iterator.Value = collectedData;
				}
			}
		}
    }

    private void OnTriggerEnter(Collider other) {
		ObjectDetected(other.gameObject);
	}

	private void OnTriggerStay(Collider other) {
		ObjectDetected(other.gameObject);
	}

	private void OnTriggerExit(Collider other) {
		GameObject othersRootGameObject = other.transform.root.gameObject;

		// Check the root game-object because that's what the sensor saves a
		// reference to.
		if (Contains(data, othersRootGameObject)) {
			DisableObjectsDetectionPoints(othersRootGameObject);
			RemoveObject(ref data, othersRootGameObject);
		}

		if (Contains(partiallyDiscoveredData, othersRootGameObject)) {
			DisableObjectsDetectionPoints(othersRootGameObject);
			RemoveObject(ref partiallyDiscoveredData, othersRootGameObject);
		}
	}

	private CollectedData GetCollectedData(LinkedList<CollectedData> collection, GameObject gameobjectToSelect) {
		return collection.Where(element => element.gameobject == gameobjectToSelect).First();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="gameobject"> ...Must be the root game-object. </param>
	private void DisableObjectsDetectionPoints(GameObject gameobject) {
		DetectionPoint[] detectionPoints = gameobject.GetComponentsInChildren<DetectionPoint>();

		foreach (DetectionPoint detectionPoint in detectionPoints) {
			detectionPoint.IsVisible(false);
		}
	}
}
