// Written by Liam Bansal
// Date Created: 30/01/2023

using DO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
// Added to access structs within the class more easily.
using static UtilityScript;

/// <summary>
/// Controls the navigation and actions of the farmer AI agent, and maintains 
/// an instance of their utility script for decision making purposes.
/// </summary>
public class Farmer : MonoBehaviour {
	private bool destinationSet = false;
	private bool destinationChanged = false;
	private bool canSeePlayer = false;
	private bool seenPlayerRecently = false;
	private bool catchColliderTouchingPlayer = false;
	private bool caughtPlayer = false;
	private float catchRange = 1.5f;
	private float containPlayerInsitence = 0.0f;
	private float maximumContainChickenInsitence = 100.0f;
	private float timeToSpendSearchingForPlayer = 0.0f;
	[SerializeField, Tooltip("The length of time the farmer will spend " +
		"searching for the player.")]
	private float maximumTimeToSpendSearchingForPlayer = 30.0f;

	private Vector3 lastKnownPlayerPosition = Vector3.zero;

	private VisualSensor visualSensor = null;
	private AudioSensor audioSensor = null;
	private BoxCollider catchCollider = null;
	private UtilityScript utilityScript = null;
	private NavMeshAgent navMeshAgent = null;
	private GameObject player = null;
	private Transform carryPosition = null;
	/// <summary>
	/// The position where the farmer will attempt the place the player after 
	/// they've been caught.
	/// </summary>
	private Transform releasePosition = null;

	#region Conditions
	public bool CanWonder() {
		return true;
	}

	public bool CanSeePlayer() {
		return canSeePlayer;
	}

	public bool CannotSeePlayer() {
		return canSeePlayer == false ? true : false;
	}

	public bool CanCatchPlayer() {
		return Vector3.Distance(transform.position, player.transform.position) < catchRange ? true : false;
	}

	public bool CanCarryPlayer() {
		return caughtPlayer;
	}

	public bool SeenPlayerRecently() {
		return seenPlayerRecently;
	}
	#endregion

	private void Awake() {
		FindComponents();
		CreateUtilityInstance();
	}

	private void Start() {
		player = GameObject.FindGameObjectWithTag("Player");
		carryPosition = GameObject.FindGameObjectWithTag("Carry Position").transform;
		releasePosition = GameObject.FindGameObjectWithTag("Release Position").transform;
	}

	private void Update() {
		// Handles seeing the player.
		if (!canSeePlayer && visualSensor.Data.Contains(player)) {
			containPlayerInsitence = maximumContainChickenInsitence;
			canSeePlayer = true;
			seenPlayerRecently = true;
			// Stops the current action so the AI can react to seeing the
			// player for the first time in a while.
			StopAction();
		}

		// Handles losing sight of the player.
		if (canSeePlayer && !visualSensor.Data.Contains(player)) {
			canSeePlayer = false;
			timeToSpendSearchingForPlayer = maximumTimeToSpendSearchingForPlayer;
		}

		// Handles what happens whilst the player is visible to the farmer.
		if (canSeePlayer) {
			lastKnownPlayerPosition = player.transform.position;
		}

		// Handles what happens if the player isn't visible to the farmer,
		// but was seen recently.
		if (!canSeePlayer && seenPlayerRecently) {
			timeToSpendSearchingForPlayer -= Time.deltaTime;

			if (timeToSpendSearchingForPlayer <= 0.0f) {
				containPlayerInsitence = 0.0f;
				seenPlayerRecently = false;
			}
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

	/// <summary>
	/// Finds the components that are used by this script, and attached to the 
	/// scripts game object, or children of.
	/// </summary>
	private void FindComponents() {
		visualSensor = GetComponentInChildren<VisualSensor>();
		audioSensor = GetComponentInChildren<AudioSensor>();
		catchCollider = GetComponentInChildren<BoxCollider>();
		navMeshAgent = GetComponent<NavMeshAgent>();
	}

	/// <summary>
	/// Creates an instance of the utility script for the AI to use, and 
	/// populates it with motives and actions.
	/// </summary>
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
				return containPlayerInsitence;
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
			Action chasePlayerAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Can See Player", CanSeePlayer)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(containPlayerMotive, satisfactionAmount)
			},
			ChasePlayer);
			#endregion
			#region Catch Action
			satisfactionAmount = 50;
			Action catchPlayerAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Can See Player", CanSeePlayer),
				new KeyValuePair<string, Action.Bool>("Can Catch Player", CanCatchPlayer)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(containPlayerMotive, satisfactionAmount)
			},
			CatchPlayer);
			#endregion
			#region Carry Action
			satisfactionAmount = 75;
			Action carryPlayerAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Can See Player", CanSeePlayer),
				new KeyValuePair<string, Action.Bool>("Can Carry Player", CanCarryPlayer)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(containPlayerMotive, satisfactionAmount)
			},
			CarryPlayer);
			#endregion
			#region Search Action
			satisfactionAmount = 15;
			Action searchAction = new Action(new KeyValuePair<string, Action.Bool>[] {
				new KeyValuePair<string, Action.Bool>("Cannot See Player", CannotSeePlayer),
				new KeyValuePair<string, Action.Bool>("Seen Player Recently", SeenPlayerRecently)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(containPlayerMotive, satisfactionAmount)
			},
			Search);
			#endregion
			Action[] actions = new Action[] {
				wonderAction,
				chasePlayerAction,
				catchPlayerAction,
				carryPlayerAction,
				searchAction
			};

			utilityScript = new UtilityScript(motives, actions);
		}
	}

	#region Farmer's Actions
	/// <summary>
	/// Stops the current action from executing.
	/// Resets various variables used throughout the farmer's actions, to 
	/// prevent the data being misread.
	/// </summary>
	private void StopAction() {
		destinationSet = false;
		destinationChanged = false;
		navMeshAgent.autoBraking = true;
		navMeshAgent.ResetPath();
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
	/// Makes the farmer search for the player if their whereabouts are unknown,
	/// by moving to the players last known position and wondering around.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool Search() {
		if (canSeePlayer || !seenPlayerRecently) {
			StopAction();
			return true;
		}

		// Check if the farmer has reached the player's last known position.
		if (HasArrivedAtDestination()) {
			Wonder();
		}

		if (!destinationSet) {
			// Move to the player's last known position.
			destinationSet = navMeshAgent.SetDestination(lastKnownPlayerPosition);
		}
		
		return false;
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

		if (navMeshAgent.destination != lastKnownPlayerPosition) {
			destinationChanged = true;
		}

		if (!destinationSet || destinationChanged) {
			Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
			// Must be lower than the catch range.
			const float spaceBetweenAgentAndPlayer = 1.25f;
			destinationSet = navMeshAgent.SetDestination(lastKnownPlayerPosition - directionToPlayer * spaceBetweenAgentAndPlayer);
			navMeshAgent.autoBraking = false;
			destinationChanged = false;
		}

		return false;
	}

	/// <summary>
	/// The farmer will try to catch the chicken if it's close enough.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool CatchPlayer() {
		if (!CanCatchPlayer()) {
			StopAction();
			return true;
		}

		catchCollider.enabled = true;

		if (catchColliderTouchingPlayer) {
			HoldOntoPlayer(false);
			catchCollider.enabled = false;
			player.transform.position = carryPosition.position;
			return caughtPlayer = true;
		}
		
		return false;
	}

	/// <summary>
	/// Carries the player back to the most recent checkpoint.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool CarryPlayer() {
		if (HasArrivedAtDestination()) {
			HoldOntoPlayer(false);
			caughtPlayer = false;
			// Set to zer so the farmer doesn't instantly chase the chicken
			// after letting them go.
			containPlayerInsitence = 0.0f;
			return true;
		}

		player.transform.position = carryPosition.position;

		if (!destinationSet) {
			destinationSet = navMeshAgent.SetDestination(releasePosition.position);
		}

		return false;
	}
	#endregion

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

	/// <summary>
	/// Restricts the players movement and keeps them in place in front of the 
	/// farmer.
	/// </summary>
	/// <param name="holdOntoPlayer"> True if the farmer should hold the player 
	/// in place. </param>
	private void HoldOntoPlayer(bool holdOntoPlayer) {
		holdOntoPlayer = !holdOntoPlayer;
		player.GetComponent<PlayerController>().enabled = holdOntoPlayer;
		player.GetComponent<InputHandler>().enabled = holdOntoPlayer;
		player.GetComponentInChildren<Rigidbody>().useGravity = holdOntoPlayer;
	}
}
