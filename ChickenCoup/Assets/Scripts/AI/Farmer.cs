// Written by Liam Bansal
// Date Created: 30/01/2023

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the navigation and actions of the farmer AI agent, and maintains 
/// an instance of their utility script for decision making purposes.
/// </summary>
public class Farmer : MonoBehaviour {
	private bool destinationSet = false;
	private bool arrivedAtDestination = false;

	private VisualSensor visualSensor = null;
	private AudioSensor audioSensor = null;
	private UtilityScript utilityScript = null;
	private NavMeshAgent navMeshAgent = null;

	private void Awake() {
		FindComponents();
		CreateUtilityInstance();
	}

	private void Update() {
		// Initiates the wonder action when starting the scene.
		if ((!destinationSet && !arrivedAtDestination) ||
			HasArrivedAtDestination()) {
			Wonder();
		}
	}

	private void FindComponents() {
		visualSensor = GetComponentInChildren<VisualSensor>();
		audioSensor = GetComponentInChildren<AudioSensor>();
		navMeshAgent = GetComponent<NavMeshAgent>();
	}

	private void CreateUtilityInstance() {
		if (utilityScript == null) {
			// TODO: Create new utility script instance here.
		}
	}

	/// <summary>
	/// Makes the farmer wonder around the environment.
	/// </summary>
	private void Wonder() {
		const int maxRotation = 25;
		int rotationAroundYAxis = Random.Range(maxRotation, maxRotation);
		// Gets a new direction to move in.
		Quaternion newMoveDirection = Quaternion.Euler(transform.rotation.eulerAngles.x,
			transform.rotation.eulerAngles.y + rotationAroundYAxis,
			transform.rotation.eulerAngles.z);
		const int moveDistance = 3;
		// get distance ahead of agent
		Vector3 wonderDestination = transform.position + newMoveDirection * Vector3.forward * moveDistance;
		destinationSet = navMeshAgent.SetDestination(wonderDestination);
		arrivedAtDestination = false;
	}

	/// <summary>
	/// Makes the farmer search for the chicken, if their whereabouts are unknown.
	/// </summary>
	private void Search() {

	}

	/// <summary>
	/// Makes the farmer pursue the chicken and get within range to catch them.
	/// </summary>
	private void Chase() {

	}

	/// <summary>
	/// The farmer will try to catch the chicken if it's close enough.
	/// </summary>
	private void Catch() {

	}

	/// <summary>
	/// Checks if the agent has arrived at their destination.
	/// </summary>
	/// <returns> True if the agent is standing close enough to their destination. </returns>
	private bool HasArrivedAtDestination() {
		// Once a destination is reached, the path is automatically removed from the nav mesh agent.
		if (destinationSet && !navMeshAgent.hasPath) {
			destinationSet = false;
			return arrivedAtDestination = true;
		}

		return arrivedAtDestination = false;
	}
}
