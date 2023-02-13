// Written by Liam Bansal
// Date Created: 30/01/2023

using DO;
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
	private bool destinationChanged = false;
	private bool canSeePlayer = false;
	private bool catchColliderTouchingPlayer = false;
	private bool caughtPlayer = false;
	private float catchRange = 1.5f;
	private float containChickenInsitence = 0.0f;
	private float maximumContainChickenInsitence = 100.0f;

	private Vector3 lastKnownPlayerPosition = Vector3.zero;

	private VisualSensor visualSensor = null;
	private AudioSensor audioSensor = null;
	private BoxCollider catchCollider = null;
	private UtilityScript utilityScript = null;
	private NavMeshAgent navMeshAgent = null;
	private GameObject player = null;
	private Transform carryPosition = null;

	#region Conditions
	public bool CanWonder() {
		return true;
	}

	public bool CanSeePlayer() {
		return canSeePlayer;
	}

	public bool CanCatchPlayer() {
		return Vector3.Distance(transform.position, player.transform.position) < catchRange ? true : false;
	}
	#endregion

	private void Awake() {
		FindComponents();
		CreateUtilityInstance();
	}

	private void Start() {
		player = GameObject.FindGameObjectWithTag("Player");
		carryPosition = GameObject.FindGameObjectWithTag("Carry Position").transform;
	}

	private void Update() {
		if (!canSeePlayer && visualSensor.Data.Contains(player)) {
			containChickenInsitence = maximumContainChickenInsitence;
			canSeePlayer = true;
			StopAction();
		}

		utilityScript.Update();
	}

	private void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Player")) {
			catchColliderTouchingPlayer = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player")) {
			catchColliderTouchingPlayer = false;
		}
	}

	private void FindComponents() {
		visualSensor = GetComponentInChildren<VisualSensor>();
		audioSensor = GetComponentInChildren<AudioSensor>();
		catchCollider = GetComponentInChildren<BoxCollider>();
		navMeshAgent = GetComponent<NavMeshAgent>();
	}

	private void CreateUtilityInstance() {
		if (utilityScript == null) {
			const float initialInsistence = 0.0f;
			#region Wonder Motive
			Motive wonderMotive = new Motive("Wonder", delegate {
				return initialInsistence;
			});
			#endregion
			#region Contain Player Motive
			Motive containPlayerMotive = new Motive("ContainPlayer", delegate {
				return containChickenInsitence;
			});
			#endregion
			Motive[] motives = new Motive[] {
				wonderMotive,
				containPlayerMotive
			};

			int satisfactionAmount = 1;
			#region Wonder Action
			Action wonderAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Idling", CanWonder)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(wonderMotive, satisfactionAmount)
			},
			Wonder);
			#endregion
			#region Chase Action
			satisfactionAmount = 25;
			Action chaseChickenAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Can See Player", CanSeePlayer)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(containPlayerMotive, satisfactionAmount)
			},
			ChasePlayer);
			#endregion
			#region Catch Action
			satisfactionAmount = 50;
			Action catchChickenAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Can See Player", CanSeePlayer),
				new KeyValuePair<string, Action.Bool>("Can Catch Player", CanCatchPlayer)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(containPlayerMotive, satisfactionAmount)
			},
			CatchPlayer);
			#endregion
			Action[] actions = new Action[] {
				wonderAction,
				chaseChickenAction,
				catchChickenAction
			};

			utilityScript = new UtilityScript(motives, actions);
		}
	}

	/// <summary>
	/// Stops the current action from executing.
	/// </summary>
	private void StopAction() {
		navMeshAgent.autoBraking = true;
		utilityScript.Reset();
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
			// Get a position ahead of the agent.
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
	private bool ChasePlayer() {
		if (HasArrivedAtDestination()) {
			navMeshAgent.autoBraking = true;
			return true;
		}

		if (player.transform.position != lastKnownPlayerPosition) {
			destinationChanged = true;
		}

		if (!destinationSet || destinationChanged) {
			Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
			lastKnownPlayerPosition = player.transform.position;
			// Must be lower than the catch range.
			const float spaceBetweenAgentAndPlayer = 1.25f;
			destinationSet = navMeshAgent.SetDestination(lastKnownPlayerPosition - directionToPlayer * spaceBetweenAgentAndPlayer);
			navMeshAgent.autoBraking = false;
		}

		return false;
	}

	/// <summary>
	/// The farmer will try to catch the chicken if it's close enough.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool CatchPlayer() {
		// TODO: remove this once the carry player action is implemented.
		if (caughtPlayer) {
			return true;
		}

		if (!CanCatchPlayer()) {
			StopAction();
			return false;
		}

		catchCollider.enabled = true;

		if (catchColliderTouchingPlayer) {
			player.GetComponent<PlayerController>().enabled = false;
			player.GetComponent<InputHandler>().enabled = false;
			player.GetComponentInChildren<Rigidbody>().useGravity = false;
			player.transform.position = carryPosition.position;
			return caughtPlayer = true;
		}
		
		return false;
	}

	/// <summary>
	/// Carries the player back to the coop, or latest checkpoint.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool CarryChicken() {
		return false;
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
