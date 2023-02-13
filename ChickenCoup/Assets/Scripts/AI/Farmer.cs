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
	private bool canSeePlayer = false;
	private float containChickenInsitence = 0.0f;
	private float maximumContainChickenInsitence = 100.0f;

	private VisualSensor visualSensor = null;
	private AudioSensor audioSensor = null;
	private UtilityScript utilityScript = null;
	private NavMeshAgent navMeshAgent = null;
	private GameObject player = null;

	#region Conditions
	public bool CanWonder() {
		return true;
	}

	public bool CanSeePlayer() {
		return canSeePlayer;
	}
	#endregion

	private void Awake() {
		FindComponents();
		CreateUtilityInstance();
	}

	private void Start() {
		player = GameObject.FindGameObjectWithTag("Player");
	}

	private void Update() {
		if (!canSeePlayer && visualSensor.Data.Contains(player)) {
			containChickenInsitence = maximumContainChickenInsitence;
			canSeePlayer = true;
			// Stop the current action to start chasing player.
			utilityScript.Reset();
		}

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
			Motive containPlayerMotive = new Motive("ContainPlayer", delegate {
				return containChickenInsitence;
			});
			Motive[] motives = new Motive[] {
				wonderMotive,
				containPlayerMotive
			};

			int satisfactionAmount = 1;
			Action wonderAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Idling", CanWonder)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(wonderMotive, satisfactionAmount)
			},
			Wonder);
			satisfactionAmount = 5;
			Action chaseChickenAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Can See Player", CanSeePlayer)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(containPlayerMotive, satisfactionAmount)
			},
			ChaseChicken);
			Action[] actions = new Action[] {
				wonderAction,
				chaseChickenAction
			};

			utilityScript = new UtilityScript(motives, actions);
		}
	}

	/// <summary>
	/// Makes the farmer wonder around the environment.
	/// </summary>
	/// <returns> True if Ai has completed the action. </returns>
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
	/// <returns> True if AI has completed the action. </returns>
	private bool ChaseChicken() {
		if (HasArrivedAtDestination()) {
			return true;
		}

		if (!destinationSet) {
			Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
			const float spaceBetweenAgentAndPlayer = 1.5f;
			destinationSet = navMeshAgent.SetDestination(player.transform.position - directionToPlayer * spaceBetweenAgentAndPlayer);
		}

		return false;
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
