// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

using DO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;
// Added for ease of access when using certain structs contained within the class.
using static UtilityScript;

/// <summary>
/// Controls the navigation and actions of the farmer AI agent, and maintains 
/// an instance of their utility script for decision making purposes.
/// </summary>
public class Farmer : MonoBehaviour {
	public enum AnimationStates {
		/// <summary>
		/// The state prior to triggering an animation.
		/// </summary>
		NotStarted,
		/// <summary>
		/// The animation has been triggered but hasn't started playing.
		/// </summary>
		Started,
		/// <summary>
		/// The animation is actively playing.
		/// </summary>
		Playing,
		/// <summary>
		/// The animation has stopped playing.
		/// </summary>
		Ended
	}

	public enum DetectionRate {
		Standard = 1,
		Slow = 3,
		Medium = 6,
		Fast = 9,
		Instant = 15
	}

	#region Sensor Variables
	private bool canSeePlayer = false;
	private bool seenPlayerRecently = false;
	private float percentageOfVisiblePlayerPoints = 0.0f;
	private Vector3 playersLastKnownPosition = Vector3.zero;
	private VisualSensor visualSensor = null;

	private bool heardPlayerRecently = false;
	/// <summary>
	/// True if the farmer has responded to the most recent audio cue created 
	/// by the player, false if they haven't responded at all.
	/// </summary>
	private bool respondedToPlayerAudioCue = false;
	private float timeUntilPlayersLastSoundCudeIsForgotten = 0.0f;
	private float maximumTimeUntilPlayersLastSoundCudeIsForgotten = 5.0f;
	private Vector3 playersLastKnownSoundCuePosition = Vector3.zero;
	private AudioSensor audioSensor = null;

	/// <summary>
	/// True if the farmer's sensors are disabled.
	/// </summary>
	private bool isBlindAndDeaf = false;
	/// <summary>
	/// How much time should pass by before the farmer can see/hear again.
	/// </summary>
	private float blindAndDeafTime = 0.0f;
	private float maximumBlindAndDeafTime = 5.0f;

	/// <summary>
	/// A measrurement from 0 to 100 of how aware the farmer is of the 
	/// player's presence.
	/// </summary>
	private float awareness = 0.0f;
	private float maximumAwareness = 100.0f;
	/// <summary>
	/// The speed at which the farmer will become fully aware of the player 
	/// when they're visible.
	/// Affected by how much of an object is visible.
	/// </summary>
	private float playerDetectionRate = 50.0f;
	#endregion

	#region Movement Variables
	private bool moveDestinationSet = false;
	private bool moveDestinationChanged = false;
	/// <summary>
	/// True if the farmer can't move, but might be able to still see and/or hear.
	/// </summary>
	private bool isStunned = false;
	/// <summary>
	/// How much time should pass by before the farmer can move again.
	/// </summary>
	private float stunTime = 0.0f;
	/// <summary>
	/// True if the farmer is able to wonder to a point of interest somewhere 
	/// around the level.
	/// </summary>
	private bool canVisitPointOfInterest = true;
	/// <summary>
	/// Index of the current point of interest that the farmer is wondering to.
	/// </summary>
	private int pointOfInterestIndex = 0;
	/// <summary>
	/// The length of time that will elapse before the farmer visits their 
	/// next point of interest.
	/// </summary>
	private float visitPointOfInterestTime = 0.0f;
	private float maximumVisitPointOfInterestTime = 25.0f;
	private float timeToSpendSearchingForPlayer = 0.0f;
	[SerializeField, Tooltip("The length of time the farmer will spend " +
		"searching for the player. Measured in seconds.")]
	private float maximumTimeToSpendSearchingForPlayer = 15.0f;
	private NavMeshAgent navMeshAgent = null;
	private PointOfInterest[] pointsOfInterest = null;
	#endregion

	#region Catch Variables
	private bool catchColliderIsTouchingPlayer = false;
	private bool hasCaughtPlayer = false;
	/// <summary>
	/// The maximum range allowed between the farmer and player where the 
	/// player can still be caught.
	/// </summary>
	private float maximumRangeToCatchPlayer = 1.5f;
	private AnimationStates catchAnimationState = AnimationStates.NotStarted;
	/// <summary>
	/// The trigger collider used for detecting a collision with the player 
	/// when the farmer attempts to catch them.
	/// </summary>
	private BoxCollider catchCollider = null;
	private Transform carryPosition = null;
	/// <summary>
	/// The position where the farmer will attempt the place the player after 
	/// they've been caught.
	/// </summary>
	private Transform releasePosition = null;
	#endregion

	private float containPlayerInsitence = 0.0f;
	private float maximumContainChickenInsitence = 100.0f;
	private UtilityScript utilityScript = null;
	
	private bool holdingKeycard = true;

	private Flashlight flashlight = null;
	private Animator animator = null;

	private GameObject playersParent = null;
	private PlayerController player = null;
	private InputHandler inputHandler = null;

	private float rotationAmount = 0.0f;
	private Quaternion originalRotation = Quaternion.identity;
	private Coroutine lookAtCoroutine = null;

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
		return !hasCaughtPlayer && Vector3.Distance(transform.position, player.transform.position) < maximumRangeToCatchPlayer ? true : false;
	}

	public bool HasCaughtPlayer() {
		return hasCaughtPlayer;
	}

	public bool ShouldSearchForPlayer() {
		return seenPlayerRecently || heardPlayerRecently ? true : false;
	}
	#endregion

	#region Public Action Triggers
	/// <summary>
	/// Stuns the farmer by freezing their position and disabling sensors for 
	/// a set time.
	/// </summary>
	/// <param name="stunLength"> Amount of time (in seconds) the farmer is immobile for. </param>
	public void StunFarmer(float stunLength) {
		isStunned = true;
		DropKeycard();
		navMeshAgent.enabled = false;
		BlindAndDeafenFarmer(stunLength);
		stunTime = stunLength;
		animator.SetBool("Stunned", true);
		animator.SetTrigger("StunnedTrigger");
	}

	/// <summary>
	/// Disables the farmers sensors for a set time, so they can't process or 
	/// gather information.
	/// </summary>
	/// <param name="blindAndDeafLength"> How long (in seconds) the farmer's sensors are turned off for. </param>
	public void BlindAndDeafenFarmer(float blindAndDeafLength) {
		isBlindAndDeaf = true;
		visualSensor.gameObject.SetActive(false);
		audioSensor.gameObject.SetActive(false);
		blindAndDeafTime = blindAndDeafLength;
	}

	/// <summary>
	/// Unparents the keycard game-object from the farmer.
	/// </summary>
	public void DropKeycard() {
		if (!holdingKeycard) {
			return;
        }

		GameObject keycard = GameObject.FindGameObjectWithTag("Keycard");

		if (keycard.transform.parent.gameObject == gameObject) {
			keycard.transform.parent = null;
        }
		
		holdingKeycard = false;
	}

	/// <summary>
	/// Stops the current action from executing.
	/// Resets various variables used throughout the farmer's actions, to 
	/// prevent the data being misread.
	/// </summary>
	public void StopAction() {
		if (isBlindAndDeaf || isStunned) {
			return;
		}

		animator.SetTrigger("Idling");
		StopAllCoroutines();
		lookAtCoroutine = null;
		moveDestinationSet = false;
		moveDestinationChanged = false;
		navMeshAgent.autoBraking = true;
		navMeshAgent.ResetPath();
		utilityScript.Reset();
		Debug.Log("Action Stopped");
	}

	public void StopCatchingPlayer() {
		catchCollider.enabled = false;
		animator.SetBool("Catching", false);
		animator.ResetTrigger("CatchingTrigger");
		catchAnimationState = AnimationStates.NotStarted;
		StopAction();
	}
	#endregion

	#region Animation Events
	public void ToggleCatchCollider(bool enableCollider) {
		catchCollider.enabled = enableCollider;

		if (!enableCollider && catchColliderIsTouchingPlayer) {
			hasCaughtPlayer = true;
			catchColliderIsTouchingPlayer = false;
		}
	}

	public void CatchAnimationStarted() {
		Debug.Log("Catch Animation Started");
		catchAnimationState = AnimationStates.Playing;
	}

	public void CatchAnimationEnded() {
		Debug.Log("Catch Animation Ended");
		animator.ResetTrigger("CatchingTrigger");
		animator.SetBool("Catching", false);
		catchAnimationState = AnimationStates.Ended;
	}
	#endregion

	public static float PathLength(NavMeshPath path) {
		float pathLength = 0.0f;

		for (int i = 0; i < path.corners.Length - 1; ++i) {
			int nextElementsIndex = i + 1;
			pathLength += Vector3.Distance(path.corners[i], path.corners[nextElementsIndex]);
		}

		return pathLength;
	}

	private void Awake() {
		GetComponents();
		CreateUtilityInstance();
	}

	private void Start() {
		FindComponents();
	}

	private void Update() {
		animator.ResetTrigger("Idling");
		BlindAndDeaf();
		Stunned();

		if (!isBlindAndDeaf) {
			HandlePlayerVisibility();
			HandlePlayerAudioCues();
		}

		if (awareness > 0 && !canSeePlayer && !seenPlayerRecently && !heardPlayerRecently) {
			awareness = 0;
		}

		if (isStunned) {
			return;
		}

		utilityScript.Update();
	}

	private void FixedUpdate() {
		float awarenessPercentage = awareness / maximumAwareness;
		const float oneQuarter = 0.2f;
		float canHearPlayer = heardPlayerRecently ? oneQuarter : 0.0f;
		flashlight.ChangeColour(Mathf.Clamp(awarenessPercentage + canHearPlayer, 0.0f, 1.0f));
	}

	private void OnTriggerEnter(Collider other) {
		if (catchCollider.enabled && other.transform.root.gameObject.CompareTag("Player")) {
			catchColliderIsTouchingPlayer = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (catchCollider.enabled && other.transform.root.gameObject.CompareTag("Player")) {
			catchColliderIsTouchingPlayer = false;
		}
	}

	/// <summary>
	/// Finds the components that are used by this script, and attached to the 
	/// scripts game object, or children of.
	/// </summary>
	private void GetComponents() {
		navMeshAgent = GetComponent<NavMeshAgent>();
		visualSensor = GetComponentInChildren<VisualSensor>();
		audioSensor = GetComponentInChildren<AudioSensor>();
		flashlight = GetComponentInChildren<Flashlight>();
		animator = GetComponentInChildren<Animator>();
	}

	/// <summary>
	/// Searches other game-objects for necessary components.
	/// </summary>
	private void FindComponents() {
		playersParent = GameObject.FindGameObjectWithTag("Player").transform.root.gameObject;
		player = playersParent.GetComponentInChildren<PlayerController>();
		inputHandler = playersParent.GetComponentInChildren<InputHandler>();
		catchCollider = GameObject.FindGameObjectWithTag("Catch Collider").GetComponent<BoxCollider>();
		carryPosition = GameObject.FindGameObjectWithTag("Carry Position").transform;
		releasePosition = GameObject.FindGameObjectWithTag("Release Position").transform;
		pointsOfInterest = FindObjectsOfType<PointOfInterest>();
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
				new KeyValuePair<string, Action.Bool>("Can Carry Player", HasCaughtPlayer)
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
				new KeyValuePair<string, Action.Bool>("Seen Player Recently", ShouldSearchForPlayer)
			},
			new KeyValuePair<Motive, float>[] {
				new KeyValuePair<Motive, float>(containPlayerMotive, satisfactionAmount)
			},
			SearchForPlayer);
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

	#region Handling Player Sight & Sound
	private void HandlePlayerVisibility() {
		// Check if detection points are enabled to avoid potentially dividing
		// 0 by 0.
		percentageOfVisiblePlayerPoints = player.NumberOfDetectionPoints == 0 ?
			0.0f : (!canSeePlayer ?
			0.0f : (float)player.VisibleDetectionPoints / (float)player.NumberOfDetectionPoints);

		// Handles seeing the player.
		if (!canSeePlayer &&
			!player.IsHiding &&
			!inputHandler.isConcealed &&
			visualSensor.Contains(visualSensor.Data, playersParent)) {
			canSeePlayer = true;
			seenPlayerRecently = true;
		}

		if (canSeePlayer && awareness < maximumAwareness) {
			float movementBonus = player.GetComponent<Rigidbody>().velocity != Vector3.zero ? 5.0f : 0.8f;
			awareness += percentageOfVisiblePlayerPoints * playerDetectionRate * movementBonus * Time.deltaTime;
		}

		Debug.Log(awareness);

		if (canSeePlayer && awareness >= maximumAwareness && containPlayerInsitence == 0) {
			awareness = maximumAwareness;
			containPlayerInsitence = awareness;
			// Stops the current action so the AI can react to seeing the
			// player for the first time in a while.
			StopAction();
		}

		// Handles losing sight of the player.
		if (canSeePlayer && !visualSensor.Contains(visualSensor.Data, playersParent)) {
			canSeePlayer = false;
			timeToSpendSearchingForPlayer = maximumTimeToSpendSearchingForPlayer;
		}

		// Handles what happens whilst the player is visible to the farmer.
		if (canSeePlayer) {
			playersLastKnownPosition = player.transform.position;
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

	/// <summary>
	/// Acknowledges player sound cues detected by the farmer's audio sensor.
	/// </summary>
	private void HandlePlayerAudioCues() {
		if ((!heardPlayerRecently || !respondedToPlayerAudioCue) &&
			audioSensor.Contains(audioSensor.Data, playersParent)) {
			const float half = 0.5f;
			containPlayerInsitence = half * maximumContainChickenInsitence;
			heardPlayerRecently = true;
			respondedToPlayerAudioCue = true;
			playersLastKnownSoundCuePosition = player.transform.position;
			timeUntilPlayersLastSoundCudeIsForgotten = maximumTimeUntilPlayersLastSoundCudeIsForgotten;

			if (!canSeePlayer && !seenPlayerRecently) {
				// The farmer only needs to stop their current action and respond
				// to an audio cue if they haven't already found the player.
				StopAction();
			}
		}

		if (respondedToPlayerAudioCue && audioSensor.Contains(audioSensor.Data, playersParent)) {
			// Forget about the audio cue's source now it's been handled.
			audioSensor.ForgetObject(playersParent);
			respondedToPlayerAudioCue = false;
		}

		if (heardPlayerRecently) {
			timeUntilPlayersLastSoundCudeIsForgotten -= Time.deltaTime;

			if (timeUntilPlayersLastSoundCudeIsForgotten <= 0 &&
				// Allow the farmer to remember they've heard the player whilst
				// searching for them.
				timeToSpendSearchingForPlayer <= 0) {
				heardPlayerRecently = false;
			}
		}
	}
	#endregion

	#region Farmer's Actions
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

		if (!moveDestinationSet) {
			if (canVisitPointOfInterest) {
				GoToPointOfInterest();

				// Check if the farmer is walking to a point of interest.
				if (moveDestinationSet) {
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

			moveDestinationSet = navMeshAgent.SetDestination(wonderDestination);
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
			moveDestinationSet = navMeshAgent.SetDestination(wonderDestination);
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
	private bool SearchForPlayer() {
		if (canSeePlayer) {
			StopAction();
			animator.SetBool("Walking", false);
			return true;
		}

		// Check if the farmer has reached the player's last known position.
		if (HasArrivedAtDestination()) {
			Wonder();
		}

		if (moveDestinationSet) {
			return false;
		}

		if (seenPlayerRecently) {
			// Move to the player's last known position.
			moveDestinationSet = navMeshAgent.SetDestination(playersLastKnownPosition);
			animator.SetBool("Walking", true);
			Debug.DrawLine(playersLastKnownPosition, playersLastKnownPosition + Vector3.up, Color.blue, Mathf.Infinity);
		} else if (heardPlayerRecently) {
			moveDestinationSet = navMeshAgent.SetDestination(playersLastKnownSoundCuePosition);
			animator.SetBool("Walking", true);
			Debug.DrawLine(playersLastKnownPosition, playersLastKnownPosition + Vector3.up, Color.magenta, Mathf.Infinity);
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

		if (navMeshAgent.destination != playersLastKnownPosition) {
			moveDestinationChanged = true;
		}

		if (!moveDestinationSet || moveDestinationChanged) {
			Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
			// Must be lower than the catch range.
			const float spaceBetweenAgentAndPlayer = 1.25f;
			moveDestinationSet = navMeshAgent.SetDestination(playersLastKnownPosition - directionToPlayer * spaceBetweenAgentAndPlayer);
			animator.SetBool("Walking", true);
			navMeshAgent.autoBraking = false;
			moveDestinationChanged = false;
		}

		return false;
	}

	/// <summary>
	/// The farmer will try to catch the player if they're both close enough 
	/// to each other.
	/// </summary>
	/// <returns> True if the action has completed (successfully or unsuccessfully). </returns>
	private bool CatchPlayer() {
		if (!CanCatchPlayer()) {
			Debug.Log("Catch Action Stopped Early");
			StopLookAtCoroutine();
			StopCatchingPlayer();
			return true;
		}

		switch (catchAnimationState) {
			case AnimationStates.NotStarted: {
				Debug.Log("Catch Action Started");
				animator.SetTrigger("CatchingTrigger");
				animator.SetBool("Catching", true);
				catchAnimationState = AnimationStates.Started;
				break;
			}
			case AnimationStates.Started: {
				break;
			}
			case AnimationStates.Playing: {
				if (lookAtCoroutine == null) {
					originalRotation = transform.rotation;
					rotationAmount = 0;
					lookAtCoroutine = StartCoroutine(LookAtCoroutine(player.transform));
				}

				if (hasCaughtPlayer) {
					Debug.Log("Holding Onto Player");
					HoldOntoPlayer(true);
					player.transform.position = carryPosition.position;
				}

				break;
			}
			case AnimationStates.Ended: {
				Debug.Log("Catch Action Ended");
				StopLookAtCoroutine();
				catchAnimationState = AnimationStates.NotStarted;
				return true;
			}
			default: {
				Debug.Log("Catch Action Stopped by Default Case");
				StopLookAtCoroutine();
				StopCatchingPlayer();
				return true;
			}
		}

		return false;

		IEnumerator LookAtCoroutine(Transform transformToLookAt) {
			Vector3 targetDirection = transformToLookAt.position - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

			while (transform.rotation != targetRotation ||
				catchAnimationState != AnimationStates.NotStarted) {
				const float rotationSpeed = 1.5f;
				// Lerp from current rotation to a rotation that faces the player.
				transform.rotation = Quaternion.Slerp(originalRotation,
					targetRotation,
					rotationAmount = rotationAmount + Time.deltaTime * rotationSpeed);
				yield return null;
			}

			yield return null;
		}

		void StopLookAtCoroutine() {
			rotationAmount = 0;

			if (lookAtCoroutine != null) {
				StopCoroutine(lookAtCoroutine);
				lookAtCoroutine = null;
			}
		}
	}

	/// <summary>
	/// Carries the player back to the most recent checkpoint.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool CarryPlayer() {
		Debug.Log("Carrying Player");

		if (HasArrivedAtDestination()) {
			Debug.Log("Released Player");
			HoldOntoPlayer(false);
			CageController cageController = GameObject.FindGameObjectWithTag("ChickenCage").GetComponent<CageController>();
			cageController.LockPlayer(player.transform);
			hasCaughtPlayer = false;
			// Set to zero so the farmer doesn't instantly chase the chicken
			// after letting them go.
			containPlayerInsitence = 0.0f;
			ForgetAboutPlayer();
			BlindAndDeafenFarmer(maximumBlindAndDeafTime);
			animator.SetBool("Carrying", false);
			return true;
		}

		player.transform.position = carryPosition.position;

		if (!moveDestinationSet) {
			moveDestinationSet = navMeshAgent.SetDestination(releasePosition.position);
			animator.SetBool("Walking", true);
			animator.SetBool("Carrying", true);
		}

		return false;
	}
	#endregion

	private void BlindAndDeaf() {
		if (blindAndDeafTime > 0) {
			blindAndDeafTime -= Time.deltaTime;

			if (blindAndDeafTime <= 0) {
				visualSensor.gameObject.SetActive(true);
				audioSensor.gameObject.SetActive(true);
				isBlindAndDeaf = false;
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
				animator.ResetTrigger("StunnedTrigger");
			}
		}
	}

	/// <summary>
	/// Causes the farmer's sensors to forget they detected the player, and 
	/// makes the farmer completely forget about the player in every capacity.
	/// </summary>
	private void ForgetAboutPlayer() {
		visualSensor.ForgetObject(player.transform.root.gameObject);
		canSeePlayer = false;
		seenPlayerRecently = false;
		audioSensor.ForgetObject(player.transform.root.gameObject);
		heardPlayerRecently = false;
	}

	/// <summary>
	/// Checks if the agent has arrived at their destination.
	/// </summary>
	/// <returns> True if the agent is standing close enough to their destination. </returns>
	private bool HasArrivedAtDestination() {
		// Once a destination is reached, the path is automatically removed
		// from the nav mesh agent.
		if (moveDestinationSet && !navMeshAgent.hasPath) {
			moveDestinationSet = false;
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
