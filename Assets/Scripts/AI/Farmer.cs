// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

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
	private bool canVisitPointOfInterest = true;
	private bool isBlind = false;
	private bool isStunned = false;
	private int pointOfInterestIndex = 0;
	private float catchRange = 1.5f;
	private float containPlayerInsitence = 0.0f;
	private float maximumContainChickenInsitence = 100.0f;
	private float timeToSpendSearchingForPlayer = 0.0f;
	[SerializeField, Tooltip("The length of time the farmer will spend " +
		"searching for the player. Measured in seconds.")]
	private float maximumTimeToSpendSearchingForPlayer = 15.0f;
	/// <summary>
	/// How much time should pass by before the farmer can see/hear again.
	/// </summary>
	private float blindTime = 0.0f;
	private float maximumBlindTime = 5.0f;
	/// <summary>
	/// How much time should pass by before the farmer can move again.
	/// </summary>
	private float stunTime = 0.0f;
	private float visitPointOfInterestTime = 0.0f;
	private float maximumVisitPointOfInterestTime = 25.0f;

	private Vector3 lastKnownPlayerPosition = Vector3.zero;

	private VisualSensor visualSensor = null;
	private AudioSensor audioSensor = null;
	private BoxCollider catchCollider = null;
	private UtilityScript utilityScript = null;
	private NavMeshAgent navMeshAgent = null;
	private GameObject playersParent = null;
	private PlayerController player = null;
	private Transform carryPosition = null;
	/// <summary>
	/// The position where the farmer will attempt the place the player after 
	/// they've been caught.
	/// </summary>
	private Transform releasePosition = null;
	private Flashlight flashlight = null;
	private PointOfInterest[] pointsOfInterest = null;
	private Animator animator = null;

	/// <summary>
	/// Stuns the farmer by freezing their position and disabling sensors for a set time.
	/// </summary>
	/// <param name="stunLength"> Amount of time (in seconds) the farmer is immobile for. </param>
	public void StunFarmer(float stunLength) {
		isStunned = true;
		navMeshAgent.enabled = false;
		BlindFarmer(stunLength);
		stunTime = stunLength;
		animator.SetBool("Stunned", true);
		animator.SetTrigger("StunnedTrigger");
	}

	/// <summary>
	/// Disables the farmers sensors for a set time, so they can't process or 
	/// gather information.
	/// </summary>
	/// <param name="blindLength"> How long (in seconds) the farmer's sensors are turned off for. </param>
	public void BlindFarmer(float blindLength) {
		isBlind = true;
		visualSensor.gameObject.SetActive(false);
		audioSensor.gameObject.SetActive(false);
		blindTime = blindLength;
	}

	public static float PathLength(NavMeshPath path) {
		float pathLength = 0.0f;

		for (int i = 0; i < path.corners.Length - 1; ++i) {
			int nextElementsIndex = i + 1;
			pathLength += Vector3.Distance(path.corners[i], path.corners[nextElementsIndex]);
		}

		return pathLength;
	}

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
		GetComponents();
		CreateUtilityInstance();
	}

	private void Start() {
		playersParent = GameObject.FindGameObjectWithTag("Player").transform.root.gameObject;
		player = playersParent.GetComponentInChildren<PlayerController>();
		carryPosition = GameObject.FindGameObjectWithTag("Carry Position").transform;
		releasePosition = GameObject.FindGameObjectWithTag("Release Position").transform;
		pointsOfInterest = FindObjectsOfType<PointOfInterest>();
	}

	private void Update() {
		Blind();
		Stunned();

		if (!isBlind) {
			HandlePlayerVisibility();
		}

		if (isStunned) {
			return;
		}

		utilityScript.Update();
	}

	private void FixedUpdate() {
		float percentageOfVisiblePoints = player.IsHiding && !canSeePlayer ? 0.0f : (float)player.VisibleDetectionPoints / (float)player.NumberOfDetectionPoints;
		flashlight.ChangeColour(percentageOfVisiblePoints);
	}

	private void OnTriggerEnter(Collider other) {
		if (other.transform.root.gameObject.CompareTag("Player")) {
			catchColliderTouchingPlayer = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.transform.root.gameObject.CompareTag("Player")) {
			catchColliderTouchingPlayer = false;
		}
	}

	/// <summary>
	/// Finds the components that are used by this script, and attached to the 
	/// scripts game object, or children of.
	/// </summary>
	private void GetComponents() {
		visualSensor = GetComponentInChildren<VisualSensor>();
		audioSensor = GetComponentInChildren<AudioSensor>();
		catchCollider = GetComponentInChildren<BoxCollider>();
		navMeshAgent = GetComponent<NavMeshAgent>();
		flashlight = GetComponentInChildren<Flashlight>();
		animator = GetComponent<Animator>();
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

	private void HandlePlayerVisibility() {
		// Handles seeing the player.
		if (!canSeePlayer && !player.IsHiding && visualSensor.Data.Contains(playersParent)) {
			containPlayerInsitence = maximumContainChickenInsitence;
			canSeePlayer = true;
			seenPlayerRecently = true;
			// Stops the current action so the AI can react to seeing the
			// player for the first time in a while.
			StopAction();
		}

		// Handles losing sight of the player.
		if (canSeePlayer && !visualSensor.Data.Contains(playersParent)) {
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
			animator.SetBool("Walking", false);
			return true;
		}

		if (visitPointOfInterestTime > 0) {
			visitPointOfInterestTime -= Time.deltaTime;

			if (visitPointOfInterestTime <= 0) {
				canVisitPointOfInterest = true;
			}
		}

		if (!destinationSet) {
			if (canVisitPointOfInterest) {
				GoToPointOfInterest();

				// Check if the farmer is walking to a point of interest.
				if (destinationSet) {
					return false;
                }
            }

			const int maxRotation = 60;
			int rotationAroundYAxis = Random.Range(-maxRotation, maxRotation);
			// Gets a new direction to move in.
			Quaternion newMoveDirection = Quaternion.Euler(transform.rotation.eulerAngles.x,
				transform.rotation.eulerAngles.y + rotationAroundYAxis,
				transform.rotation.eulerAngles.z);
			const int moveDistance = 3;
			// Get a position ahead of the agent.
			Vector3 wonderDestination = transform.position + newMoveDirection * Vector3.forward * moveDistance;
			NavMesh.SamplePosition(wonderDestination, out NavMeshHit navMeshHit, moveDistance, NavMesh.AllAreas);
			const float turnAroundThreshold = 2.8f;

			if (navMeshHit.hit) {
				Debug.DrawLine(navMeshHit.position, navMeshHit.position + Vector3.up, Color.green, 5);
			}

			// Check if the farmer is at the edge of the nav mesh.
			if (navMeshHit.hit && Vector3.Distance(wonderDestination, navMeshHit.position) >= turnAroundThreshold) {
				Debug.DrawLine(wonderDestination, wonderDestination + Vector3.up, Color.red, 5);
				// Set the farmer's destination behind them.
				wonderDestination = transform.position + newMoveDirection * Vector3.back * moveDistance;
				Debug.Log("Turned Around.");
			}

			destinationSet = navMeshAgent.SetDestination(wonderDestination);
			animator.SetBool("Walking", true);
		}

		return false;

		void GoToPointOfInterest() {
			if (pointsOfInterest.Length == 0) {
				return;
            }

			if (pointOfInterestIndex > pointsOfInterest.Length - 1) {
				pointOfInterestIndex = 0;
            }

			Vector3 wonderDestination = pointsOfInterest[pointOfInterestIndex++].GetComponent<PointOfInterest>().StandPosition;
			destinationSet = navMeshAgent.SetDestination(wonderDestination);
			animator.SetBool("Walking", true);
			visitPointOfInterestTime = maximumVisitPointOfInterestTime;
			canVisitPointOfInterest = false;
		}
	}

	/// <summary>
	/// Makes the farmer search for the player if their whereabouts are unknown,
	/// by moving to the players last known position and wondering around.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool Search() {
		if (canSeePlayer || !seenPlayerRecently) {
			StopAction();
			animator.SetBool("Walking", false);
			return true;
		}

		// Check if the farmer has reached the player's last known position.
		if (HasArrivedAtDestination()) {
			Wonder();
		}

		if (!destinationSet) {
			// Move to the player's last known position.
			destinationSet = navMeshAgent.SetDestination(lastKnownPlayerPosition);
			animator.SetBool("Walking", true);
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
			animator.SetBool("Walking", false);
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
			animator.SetBool("Walking", true);
			navMeshAgent.autoBraking = false;
			destinationChanged = false;
		}

		return false;
	}

	// TODO: disable relevant aniamtions properties by keyframing an event in the animations.
	/// <summary>
	/// The farmer will try to catch the chicken if it's close enough.
	/// </summary>
	/// <returns> True if the action completed successfully. </returns>
	private bool CatchPlayer() {
		if (!CanCatchPlayer()) {
			StopAction();
			return true;
		}

		catchCollider.enabled = true;

		if (catchColliderTouchingPlayer) {
			HoldOntoPlayer(true);
			catchCollider.enabled = false;
			player.transform.position = carryPosition.position;
			animator.SetBool("Catching", true);
			return caughtPlayer = true;
		}

		StopAction();
		return false;
	}

	/// <summary>
	/// Carries the player back to the most recent checkpoint.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool CarryPlayer() {
		if (HasArrivedAtDestination()) {
			HoldOntoPlayer(false);
			CageController cageController = GameObject.FindGameObjectWithTag("ChickenCage").GetComponent<CageController>();
			cageController.LockPlayer(player.transform);
			caughtPlayer = false;
			// Set to zer so the farmer doesn't instantly chase the chicken
			// after letting them go.
			containPlayerInsitence = 0.0f;
			visualSensor.ForgetObject(player.transform.root.gameObject);
			audioSensor.ForgetObject(player.transform.root.gameObject);
			BlindFarmer(maximumBlindTime);
			animator.SetBool("Carrying", false);
			return true;
		}

		player.transform.position = carryPosition.position;

		if (!destinationSet) {
			destinationSet = navMeshAgent.SetDestination(releasePosition.position);
			animator.SetBool("Walking", true);
			animator.SetBool("Carrying", true);
		}

		return false;
	}
	#endregion

	private void Blind() {
		if (blindTime > 0) {
			blindTime -= Time.deltaTime;

			if (blindTime <= 0) {
				visualSensor.gameObject.SetActive(true);
				audioSensor.gameObject.SetActive(true);
				isBlind = false;
			}
		}
	}

	private void Stunned() {
		if (stunTime > 0) {
			stunTime -= Time.deltaTime;

			if (stunTime <= 0) {
				navMeshAgent.enabled = true;
				isStunned = false;
				animator.SetBool("Stunned", false);
			}
		}
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

	/// <summary>
	/// Restricts the players movement and keeps them in place in front of the 
	/// farmer.
	/// </summary>
	/// <param name="holdOntoPlayer"> True if the farmer should hold the player 
	/// in place. </param>
	private void HoldOntoPlayer(bool holdOntoPlayer) {
		holdOntoPlayer = !holdOntoPlayer;
		player.enabled = holdOntoPlayer;
		playersParent.GetComponentInChildren<InputHandler>().enabled = holdOntoPlayer;
		playersParent.GetComponentInChildren<Rigidbody>().useGravity = holdOntoPlayer;
	}
}
