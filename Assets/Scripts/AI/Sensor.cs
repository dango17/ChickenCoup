// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

using DO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Uses a trigger collider to detect game-objects of a certain type.
/// </summary>
public abstract class Sensor : MonoBehaviour {
	public struct CollectedData {
		/// <summary>
		/// True if this object has been identified by the sensor.
		/// </summary>
		public bool acknowledged;
		/// <summary>
		/// True if an object should be forgotten once it leaves the sensor's 
		/// detection zone.
		/// </summary>
		public bool forgettable;
		public float lifespan;
		public GameObject gameobject;

		public CollectedData (GameObject gameobject, float lifespan) {
			acknowledged = false;
			forgettable = true;
			this.gameobject = gameobject;
			this.lifespan = lifespan;
        }
	}

	public enum Visibility {
		Visible,
		PartiallyVisible,
		NotVisible
	}

	public int DetectionRange {
		get { return detectionRange; }
		private set { detectionRange = value; }
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
        "be forgotten after 10 seconds.")]
	protected bool deleteDataAfterDelay = false;
	[SerializeField, Tooltip("The length of time data will be saved for, " +
		"before being forgotten.")]
	protected float defaultDataLifespan = 10.0f;
	[SerializeField, Tooltip("Data will only be gathered within this radius" +
		"of the sensors origin.")]
	protected int detectionRange = 5;
	[SerializeField, Tooltip("The sensor will only 'see' data that exists " +
		"on this layer.")]
	protected LayerMask visibleLayer = default;
	[SerializeField, Tooltip("The sensor will only remember data that exists " +
		"on this layer.")]
	protected LayerMask detectionLayer = default;
	/// <summary>
	/// Stores references to the game objects the agent has gathered data about.
	/// </summary>
	protected LinkedList<CollectedData> data = new LinkedList<CollectedData>();
	protected LinkedList<CollectedData> partiallyDiscoveredData = new LinkedList<CollectedData>();
	[SerializeField, Tooltip("The central point of the sensor bounds.")]
	protected Transform sensorOrigin = null;

    #region Helper Methods
    /// <summary>
	/// Returns true if the parameter game-object exists within the parameter 
	/// collection.
	/// </summary>
	/// <param name="collection"> The collection to search. </param>
	/// <param name="gameobjectToSelect"> The game-object to search for. </param>
	/// <returns> True if the game-object exists within the collection. </returns>
	public bool Contains(LinkedList<CollectedData> collection, GameObject gameobjectToSelect) {
		if (collection.Count == 0) {
			return false;
		}

		IEnumerable<CollectedData> filteredCollection = collection.Where(element => element.gameobject == gameobjectToSelect);

		if (filteredCollection.Count() == 0) {
			return false;
		}

		return filteredCollection.First().gameobject;
	}

	public void AddObject(ref LinkedList<CollectedData> collection, GameObject objectToAdd) {
		if (!Contains(collection, objectToAdd)) {
			collection.AddLast(new CollectedData(objectToAdd, defaultDataLifespan));
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

		// A result of 0 means both layers do not share any of the same
		// flipped bits.
		if (((detectionLayer & detectedGameObjectsLayer) == 0) ||
			// Data that doesn't exist on the visible layer should also be
			// ignored, only if it's also not on the memorable layer.
			(((visibleLayer & detectedGameObjectsLayer) == 0) &&
			((detectionLayer & detectedGameObjectsLayer) == 0))) {
			return;
		}

		bool discovered = Contains(data, detectedGameObject);
		bool partiallyDiscovered = Contains(partiallyDiscoveredData, detectedGameObject);

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
					AddObject(ref data, detectedGameObject);
				}

				if (partiallyDiscovered) {
					RemoveObject(ref partiallyDiscoveredData, detectedGameObject);
				}

				break;
			}
			case Visibility.PartiallyVisible: {
				if (!partiallyDiscovered) {
					AddObject(ref partiallyDiscoveredData, detectedGameObject);
				}

				break;
			}
			case Visibility.NotVisible: {
				ForgetObject(detectedGameObject);
				return;
			}
			default: {
				ForgetObject(detectedGameObject);
				return;
			}
		}
	}

	/// <summary>
	/// Makes the sensor forget about a specific bit of data.
	/// </summary>
	/// <param name="gameObjectToForget"> The game-object to forget. </param>
	public void ForgetObject(GameObject gameObjectToForget) {
		RemoveObject(ref data, gameObjectToForget);
		RemoveObject(ref partiallyDiscoveredData, gameObjectToForget);
		DisableObjectsDetectionPoints(gameObjectToForget);
	}

	protected virtual void Awake() {
		SphereCollider sphereCollider = GetComponent<SphereCollider>();
		sphereCollider.radius = detectionRange;
	}

	protected virtual void Start() { }

    protected virtual void Update() {
		DeleteGatheredDataThatsExpired();
	}

	protected virtual void FixedUpdate() { }

	protected CollectedData GetCollectedData(LinkedList<CollectedData> collection, GameObject gameobjectToSelect) {
		return collection.Where(element => element.gameobject == gameobjectToSelect).First();
	}

	/// <summary>
	/// Checks if the gathered data is valid and should be saved.
	/// </summary>
	/// <param name="gameobject"> Gathered data that needs verifying. </param>
	/// <returns> True if the data is valid and should be saved. </returns>
	protected abstract Visibility VerifyDetection(GameObject gameobject);

    private void OnTriggerEnter(Collider other) {
		ObjectDetected(other.gameObject);
	}

	private void OnTriggerStay(Collider other) {
		ObjectDetected(other.gameObject);
	}

	private void OnTriggerExit(Collider other) {
		GameObject othersGameObject = other.gameObject;

		if (Contains(data, othersGameObject)) {
			DisableObjectsDetectionPoints(othersGameObject);
			RemoveObject(ref data, othersGameObject);
		}

		if (Contains(partiallyDiscoveredData, othersGameObject)) {
			DisableObjectsDetectionPoints(othersGameObject);
			RemoveObject(ref partiallyDiscoveredData, othersGameObject);
		}
	}

	private void DeleteGatheredDataThatsExpired() {
		if (!deleteDataAfterDelay) {
			return;
		}

		SearchCollectionForExpiredData(ref data);
		SearchCollectionForExpiredData(ref partiallyDiscoveredData);

		void SearchCollectionForExpiredData(ref LinkedList<CollectedData> collection) {
			if (collection.Count == 0) {
				return;
			}

			LinkedListNode<CollectedData> iterator = null;

			for (iterator = collection.First;
				iterator.Next != null;
				iterator = iterator.Next) {
				RemoveExpiredData(ref collection, ref iterator);
			}

			RemoveExpiredData(ref collection, ref iterator);
		}

		void RemoveExpiredData(ref LinkedList<CollectedData> collection, ref LinkedListNode<CollectedData> iterator) {
			CollectedData collectedData = iterator.Value;
			collectedData.lifespan -= Time.deltaTime;

			if (collectedData.lifespan <= 0) {
				LinkedListNode<CollectedData> previousValue = iterator.Previous != null ? iterator.Previous : default;
				collection.Remove(iterator);
				iterator = previousValue;
			} else {
				iterator.Value = collectedData;
			}
		}
	}

	private void DisableObjectsDetectionPoints(GameObject gameobject) {
		DetectionPoint[] detectionPoints = gameobject.GetComponentsInChildren<DetectionPoint>();

		foreach (DetectionPoint detectionPoint in detectionPoints) {
			detectionPoint.IsVisible(false);
		}
	}
}
