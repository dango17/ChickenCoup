// Written by Liam Bansal
// Date Created: 30/01/2023

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UtilityScript;

/// <summary>
/// Controls the navigation and actions of the farmer AI agent, and maintains 
/// an instance of their utility script for decision making purposes.
/// </summary>
public class Farmer : MonoBehaviour {
	private bool destinationSet = false;

	private VisualSensor visualSensor = null;
	private AudioSensor audioSensor = null;
	private UtilityScript utilityScript = null;
	private NavMeshAgent navMeshAgent = null;

	#region Conditions
	public bool CanWonder() {
		return true;
	}
	#endregion

	private void Awake() {
		FindComponents();
		CreateUtilityInstance();
	}

	private void Update() {
		utilityScript.Update();
	}

	private void FindComponents() {
		visualSensor = GetComponentInChildren<VisualSensor>();
		audioSensor = GetComponentInChildren<AudioSensor>();
		navMeshAgent = GetComponent<NavMeshAgent>();
	}

	private void CreateUtilityInstance() {
		if (utilityScript == null) {
			const float initialInsistence = 0.0f;
			Motive wonderMotive = new Motive("Wonder", delegate {
				return initialInsistence;
			});
			Motive[] motives = new Motive[] {
				wonderMotive
			};

			const float satisfactionAmount = 1.0f;
			Action wonderAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Idling", CanWonder)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(wonderMotive, satisfactionAmount)
			},
			Wonder);
			Action[] actions = new Action[] {
				wonderAction
			};

			utilityScript = new UtilityScript(motives, actions);
		}
	}	

	/// <summary>
	/// Makes the farmer wonder around the environment.
	/// </summary>
	private bool Wonder() {
		if (HasArrivedAtDestination()) {
			return true;
		}

		if (!destinationSet) {
			const int maxRotation = 60;
			int rotationAroundYAxis = Random.Range(-maxRotation, maxRotation);
			// Gets a new direction to move in.
			Quaternion newMoveDirection = Quaternion.Euler(transform.rotation.eulerAngles.x,
				transform.rotation.eulerAngles.y + rotationAroundYAxis,
				transform.rotation.eulerAngles.z);
			const int moveDistance = 3;
			// get distance ahead of agent
			Vector3 wonderDestination = transform.position + newMoveDirection * Vector3.forward * moveDistance;
			destinationSet = navMeshAgent.SetDestination(wonderDestination);
		}

		return false;
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
			return true;
		}

		return false;
	}
}
