// Author: Liam Bansal
// Collaborator: N/A
// Created On: 30/01/2023

using DO;
using System.Collections;
using System.Collections.Generic;
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

	public bool HasCaughtPlayer {
		get { return hasCaughtPlayer; }
		private set { hasCaughtPlayer = value; }
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
	private const float maximumTimeUntilPlayersLastSoundCudeIsForgotten = 5.0f;
	private Vector3 playersLastKnownSoundCuePosition = Vector3.zero;
	private AudioSensor audioSensor = null;

	/// <summary>
	/// True if the farmer has spotted an object that was potentially 
	/// created/caused by the player.
	/// </summary>
	private bool noticedPlayerClue = false;
	private Vector3 playersCluePosition = Vector3.zero;
	private bool noticedBrokenObject = false;
	private Vector3 brokenObjectPosition = Vector3.zero;

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
	/// 
	/// </summary>
	[SerializeField, Range(1, 100), Tooltip("The speed at which the farmer will become fully " +
		"aware of the player when they're visible.")]
	private float playerDetectionRate = 80.0f;
	#endregion

	#region Movement Variables
	[SerializeField, Tooltip("Controls how fast the farmer moves when walking.")]
	private float walkSpeed = 2.25f;
	/// <summary>
	/// A multiplier for the walk animation's playback speed.
	/// </summary>
	private float walkPlaybackSpeedMultiplier = 0.75f;
	[SerializeField, Tooltip("Controls how fast the farmer moves when chasing " +
		"the player.")]
	private float chaseSpeed = 4.25f;
	/// <summary>
	/// A multiplier for the chase animation's playback speed.
	/// </summary>
	private float chasePlaybackSpeedMultiplier = 1.4f;
	private bool moveDestinationSet = false;
	private bool moveDestinationChanged = false;
	/// <summary>
	/// True if the farmer can't move, but might be able to still see and/or hear.
	/// </summary>
	private bool isStunned = false;
	/// <summary>
	/// Is the farmer actively trying to stand up.
	/// </summary>
	private bool isGettingUp = false;
	/// <summary>
	/// How much time should pass by before the farmer can move again.
	/// </summary>
	private float stunTime = 0.0f;
	/// <summary>
	/// True if the farmer is able to wonder to a point of interest somewhere 
	/// around the level.
	/// </summary>
	private bool canVisitPointOfInterest = true;
	private bool isVisitingPointOfInterest = false;
	/// <summary>
	/// Index of the current point of interest that the farmer is wondering to.
	/// </summary>
	private int pointOfInterestIndex = 0;
	/// <summary>
	/// The length of time that will elapse before the farmer visits their 
	/// next point of interest.
	/// </summary>
	private float visitPointOfInterestTime = 0.0f;
	private const float maximumVisitPointOfInterestTime = 25.0f;
	private float inspectPointOfInterestTime = 0.0f;
	private const float maximumInspectPointOfInterestTime = 5.0f;
	private float timeToSpendSearchingForPlayer = 0.0f;
	[SerializeField, Tooltip("The length of time the farmer will spend " +
		"searching for the player. Measured in seconds.")]
	private const float maximumTimeToSpendSearchingForPlayer = 15.0f;
	private NavMeshAgent navMeshAgent = null;
	private PointOfInterest[] pointsOfInterest = null;
	private WonderPointManager wonderPointManager = null;
	#endregion

	#region Catch Variables
	private bool catchCollidersEnabled = false;
	/// <summary>
	/// True if at least one of the colliders is touching the player.
	/// </summary>
	private bool catchColliderIsTouchingPlayer = false;
	private bool hasCaughtPlayer = false;
	/// <summary>
	/// The maximum range allowed between the farmer and player where the 
	/// player can still be caught.
	/// </summary>
	private const float maximumRangeToCatchPlayer = 1.5f;
	private AnimationStates catchAnimationState = AnimationStates.NotStarted;
	/// <summary>
	/// The game-object the farmer is holding.
	/// </summary>
	private GameObject heldObject = null;
	[SerializeField, Tooltip("The trigger colliders used for detecting a " +
		"collision with the player when the farmer attempts to catch them.")]
	private BoxCollider[] catchColliders = null;
	private Transform carryPosition = null;
	/// <summary>
	/// The position where the farmer will attempt the place the player after 
	/// they've been caught.
	/// </summary>
	private Transform releasePosition = null;
	#endregion

	private float containPlayerInsitence = 0.0f;
	private const float maximumContainChickenInsitence = 100.0f;
	private UtilityScript utilityScript = null;
	
	private bool holdingKeycard = true;

	private Flashlight flashlight = null;
	private GameObject keycard = null;
	private Transform keycardHoldTransform = null;
	private Animator animator = null;
	[SerializeField, Tooltip("The avatar used for the farmer's animation " +
		"when they're stunned.")]
	private Avatar stunnedAvatar = null;

	private PlayerController player = null;
	private InputHandler inputHandler = null;
	private Rigidbody playerRigidbody = null;

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

	public bool IsHoldingPlayer() {
		return hasCaughtPlayer;
	}

	public bool ShouldSearchForPlayer() {
		return seenPlayerRecently || heardPlayerRecently || noticedPlayerClue || noticedBrokenObject ? true : false;
	}
	#endregion

	#region Public Action Triggers
	/// <summary>
	/// Stuns the farmer by freezing their position and disabling sensors for 
	/// a set time.
	/// </summary>
	/// <param name="stunLength"> Amount of time (in seconds) the farmer is immobile for. </param>
	public void StunFarmer(float stunLength) {
		StopAction(false);
		isStunned = true;
		DropKeycard();
		navMeshAgent.enabled = false;
		BlindAndDeafenFarmer(stunLength);
		stunTime = stunLength;
		animator.avatar = stunnedAvatar;
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

		if (keycard.transform.parent != null &&
			keycard.transform.parent.gameObject == gameObject) {
			keycard.transform.parent = null;
        }
		
		holdingKeycard = false;
	}

	/// <summary>
	/// Stops the current action from executing.
	/// Resets various variables used throughout the farmer's actions, to 
	/// prevent the data being misused.
	/// </summary>
	/// <param name="shouldIdle"> True if the farmer should enter their idle 
	/// animation after stopping their current action. </param>
	public void StopAction(bool shouldIdle) {
		ResetAnimatorParameters();

		if (shouldIdle) {
			animator.SetTrigger("Idling");
		}
		
		StopAllCoroutines();
		lookAtCoroutine = null;
		moveDestinationSet = false;
		moveDestinationChanged = false;
		isVisitingPointOfInterest = false;
		isStunned = false;
		stunTime = 0.0f;
		isGettingUp = false;
		navMeshAgent.enabled = true;
		navMeshAgent.autoBraking = true;
		navMeshAgent.ResetPath();
		utilityScript.Reset();
	}

	/// <summary>
	/// Used to completely stop/cancel the catch action, and resets the catch 
	/// related variables.
	/// </summary>
	public void StopCatchingPlayer() {
		if (hasCaughtPlayer) {
			HoldOntoPlayer(false);
		}

		hasCaughtPlayer = false;
		heldObject = null;
		catchColliderIsTouchingPlayer = false;
		ToggleCatchCollider(false);
		animator.SetBool("Catching", false);
		animator.ResetTrigger("CatchingTrigger");
		catchAnimationState = AnimationStates.NotStarted;
		StopLookAtCoroutine();
		StopAction(true);
	}
	#endregion

	#region Animation Events
	public void ToggleCatchCollider(bool enableCollider) {
		catchCollidersEnabled = enableCollider;
		catchColliders[0].enabled = enableCollider;
		catchColliders[1].enabled = enableCollider;
	}

	public void CatchAnimationStarted() {
		catchAnimationState = AnimationStates.Playing;
	}

	public void CatchAnimationEnded() {
		animator.ResetTrigger("CatchingTrigger");
		animator.SetBool("Catching", false);
		catchAnimationState = AnimationStates.Ended;
	}

	public void StunAnimationStarted() {
		animator.SetBool("Stunned", true);
	}

	public void GettingUpAnimationEnded() {
		animator.SetBool("GettingUp", false);
		isGettingUp = false;
		animator.avatar = null;
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
		SetVariables();
	}

	private void Update() {
		animator.ResetTrigger("Idling");
		UpdateTimers();

		if (!isBlindAndDeaf) {
			HandlePlayerVisibility();
			HandlePlayerAudioCues();
			HandleObjectVisibility();
		}

		if (awareness > 0 && !canSeePlayer && !seenPlayerRecently && !heardPlayerRecently) {
			awareness = 0;
		}

		if (holdingKeycard) {
			keycard.transform.position = keycardHoldTransform.position;
		}

		if (isStunned || isGettingUp) {
			return;
		}

		utilityScript.Update();
	}

	private void FixedUpdate() {
		UpdateFlashlightColour();
	}

	private void OnTriggerEnter(Collider other) {
		if (catchCollidersEnabled && other.gameObject.CompareTag("Player")) {
			catchColliderIsTouchingPlayer = true;
		}
	}

	private void OnTriggerStay(Collider other) {
		if (catchCollidersEnabled && other.gameObject.CompareTag("Player")) {
			catchColliderIsTouchingPlayer = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (catchCollidersEnabled && other.gameObject.CompareTag("Player")) {
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
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		inputHandler = player.GetComponent<InputHandler>();
		playerRigidbody = player.GetComponent<Rigidbody>();
		carryPosition = GameObject.FindGameObjectWithTag("Carry Position").transform;
		releasePosition = GameObject.FindGameObjectWithTag("Release Position").transform;
		pointsOfInterest = FindObjectsOfType<PointOfInterest>();
		keycard = GameObject.FindGameObjectWithTag("Keycard");
		keycardHoldTransform = GameObject.FindGameObjectWithTag("Keycard Hold Position").transform;
		wonderPointManager = GameObject.FindGameObjectWithTag("Wonder Point Manager").GetComponent<WonderPointManager>();
	}

	/// <summary>
	/// Sets any variables that need changing as soon as the game starts.
	/// </summary>
	private void SetVariables() {
		// If the nav mesh agent move speed is 0.75...
		const float speed = 0.75f;
		// ...0.25 would be a good animation playback speed multiplier.
		const float multiplier = 0.25f;
		// This equation mostly produces a believable playback speed for the
		// walk/chase animation.
		// Every other speed can use these base values to automatically
		// calculate any new multipliers, rather than manually doing it within
		// the inspector.
		walkPlaybackSpeedMultiplier = walkSpeed / speed * multiplier;
		chasePlaybackSpeedMultiplier = chaseSpeed / speed * multiplier;
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
				new KeyValuePair<string, Action.Bool>("Can Carry Player", IsHoldingPlayer)
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

	#region Handles Responding to Sight & Sound for Detected Objects
	private void HandlePlayerVisibility() {
		// Check if detection points are enabled to avoid potentially dividing
		// 0 by 0.
		percentageOfVisiblePlayerPoints = player.NumberOfDetectionPoints == 0 ?
			0.0f : (!canSeePlayer ?
			0.0f : (float)player.VisibleDetectionPoints / (float)player.NumberOfDetectionPoints);

		// Handles seeing the player.
		if (!canSeePlayer &&
			!player.IsHiding &&
			visualSensor.GetCollectedData(visualSensor.Data, player.gameObject).gameobject) {
			canSeePlayer = true;
			seenPlayerRecently = true;
		}

		// Handles losing sight of the player.
		if (canSeePlayer && !visualSensor.GetCollectedData(visualSensor.Data, player.gameObject).gameobject) {
			canSeePlayer = false;
			timeToSpendSearchingForPlayer = maximumTimeToSpendSearchingForPlayer;
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

		// Changes the farmers awareness based on how visible the player is.
		if (canSeePlayer && awareness < maximumAwareness) {
			float detectionBonusFromPlayerMovement = CalculateMovementBonus();
			float detectionBonusFromPlayerDistance = CalculateDistanceBonus();
			float detectionBonusFromPlayerPosition = CalculateFieldOfViewBonus();
			float detectionBonusFromPlayerConcealment = inputHandler.isConcealed ? 0.4f : 1.0f;
			float detectionBonuses = detectionBonusFromPlayerMovement *
				detectionBonusFromPlayerDistance *
				detectionBonusFromPlayerPosition *
				detectionBonusFromPlayerConcealment;
			awareness += percentageOfVisiblePlayerPoints *
				playerDetectionRate *
				detectionBonuses * 
				Time.deltaTime;
		}

		if (canSeePlayer && awareness >= maximumAwareness && containPlayerInsitence == 0) {
			awareness = maximumAwareness;
			containPlayerInsitence = awareness;
			// Stops the current action so the AI can react to seeing the
			// player for the first time in a while.
			StopAction(true);
		}

		// Handles what happens whilst the player is visible to the farmer.
		if (canSeePlayer) {
			playersLastKnownPosition = player.transform.position;
		}

		float CalculateMovementBonus() {
			float maximumVelocity = player.MaximumMovementSpeed;
			float playerVelocity = playerRigidbody.velocity.magnitude;
			// Scale the increased detection bonus from player movement based
			// on how fast the player is moving, up to a peak value.
			float increasedBonus = 1.0f + playerVelocity / maximumVelocity;
			const float decreasedBonus = 0.7f;
			return playerVelocity > 0 ? increasedBonus : decreasedBonus;
		}

		float CalculateDistanceBonus() {
			float maximumDistance = visualSensor.DetectionRange;
			float playerDistance = Vector3.Distance(player.transform.position,
				visualSensor.transform.position);
			// 0.5 = decreased detection rate, 1 = no effect, 1.5 = increased
			// detection rate.
			// Setting to 1.5 means the farmer is offered a greater bonus when
			// the player is within half their visual sensor's detection range;
			// no bonus when the player is at half their visual sensor's
			// detection range;
			// a decreased bonus when the player is beyond half their visual
			// sensor's detection range.
			const float defaultDistanceBonus = 1.5f;
			// Scale the increased detection bonus from player distance based
			// on how far they are from the farmer.
			// Subtract from the bonus because distances should provide a
			// greater bonus than longer distances.
			return defaultDistanceBonus - playerDistance / maximumDistance;
		}

		// The farmer's visual sensor field of view extents have been drawn in
		// the editor to help visualise what this method does.
		float CalculateFieldOfViewBonus() {
			Vector3 playerDirection = player.transform.position - visualSensor.transform.position;
			float angleBetweenFarmerAndPlayer = Vector3.Angle(visualSensor.transform.forward,
				playerDirection);
			const float fourtyPercent = 0.4f;
			const float half = 0.5f;

			// Check if the player is further than 40% of the way between the
			// farmer's visual sensor's forward direction and field of view extent.
			if (angleBetweenFarmerAndPlayer > fourtyPercent * (half * visualSensor.FieldOfView)) {
				float angleToFieldOfViewExtentLeft = Vector3.Angle(playerDirection,
					visualSensor.FieldOfViewExtentsDirection[0]);
				float angleToFieldOfViewExtentRight = Vector3.Angle(playerDirection,
					visualSensor.FieldOfViewExtentsDirection[1]);
				// Finds which field of view extent (left/right) the player is
				// the closest to.
				float smallestAngle = angleToFieldOfViewExtentLeft < angleToFieldOfViewExtentRight ?
					angleToFieldOfViewExtentLeft : angleToFieldOfViewExtentRight;
				float maximumFieldOfView = visualSensor.FieldOfView;
				// The bonus should be decreased based on the offset from the
				// direction to the player to the farmer's visual sensor's
				// forward direction.
				return smallestAngle / (half * maximumFieldOfView);
			} else {
				// The bonus should be substantial while the player is roughly
				// directly in front of the farmer.
				const float defaultBonus = 1.5f;
				return defaultBonus;
			}
		}
	}

	/// <summary>
	/// Acknowledges player sound cues detected by the farmer's audio sensor.
	/// </summary>
	private void HandlePlayerAudioCues() {
		if ((!heardPlayerRecently || !respondedToPlayerAudioCue) &&
			audioSensor.GetCollectedData(audioSensor.Data, player.gameObject).gameobject) {
			const float half = 0.5f;
			containPlayerInsitence = half * maximumContainChickenInsitence;
			heardPlayerRecently = true;
			respondedToPlayerAudioCue = true;
			playersLastKnownSoundCuePosition = player.transform.position;
			timeUntilPlayersLastSoundCudeIsForgotten = maximumTimeUntilPlayersLastSoundCudeIsForgotten;

			if (!canSeePlayer && !seenPlayerRecently) {
				// The farmer only needs to stop their current action and respond
				// to an audio cue if they haven't already found the player.
				StopAction(true);
			}
		}

		if (respondedToPlayerAudioCue && audioSensor.GetCollectedData(audioSensor.Data, player.gameObject).gameobject) {
			// Forget about the audio cue's source now it's been handled.
			audioSensor.ForgetObject(player.gameObject);
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

	private void HandleObjectVisibility() {
		if (canSeePlayer || heardPlayerRecently) {
			noticedPlayerClue = false;
			noticedBrokenObject = false;
			// Stops the processing of gathered data that isn't related to the
			// player.
			return;
		}

		if (visualSensor.Data.Count == 0) {
			return;
		}

		for (LinkedListNode<Sensor.CollectedData> iterator = visualSensor.Data.First;
			iterator != visualSensor.Data.Last;
			iterator = iterator.Next) {
			HandleInteractableObject(iterator.Value);
		}

		// Run one more time to evaluate the last element.
		HandleInteractableObject(visualSensor.Data.Last.Value);

		void HandleInteractableObject(Sensor.CollectedData sensorData) {
			int interactableLayerMask = LayerMask.GetMask("Interactable");
			int collectedDatasLayer = 1 << sensorData.gameobject.layer;

			if (collectedDatasLayer != interactableLayerMask) {
				return;
			}

			if (sensorData.gameobject && !sensorData.acknowledged) {
				// The script for breaking pots is two levels up from the
				// detected object.
				PotBreak potBreakScript = sensorData.gameobject.transform.parent.parent.GetComponent<PotBreak>();

				if (noticedBrokenObject) {
					// Return if a broken object has already been detected.
					return;
				}

				if (potBreakScript && potBreakScript.isBroken) {
					noticedBrokenObject = true;
					brokenObjectPosition = potBreakScript.gameObject.transform.position;
				}

				if (noticedBrokenObject) {
					sensorData.acknowledged = true;
					// Stop the sensor from forgetting about this broken object.
					// So it's no surprise when the farmer sees it again.
					sensorData.forgettable = false;
				}

				if (noticedBrokenObject) {
					noticedPlayerClue = true;
					playersCluePosition = sensorData.gameobject.transform.position;
				}

				if (noticedPlayerClue || noticedBrokenObject) {
					const float half = 0.5f;
					containPlayerInsitence = half * maximumContainChickenInsitence;
					StopAction(false);
				}
			}
		}
	}
	#endregion

	#region Farmer's Actions
	/// <summary>
	/// Makes the farmer wonder around the environment and occasionally visit 
	/// points of interest.
	/// </summary>
	/// <returns> True if the farmer has completed the action (successfully or unsuccessfully). </returns>
	private bool Wonder() {
		if (HasArrivedAtDestination()) {
			animator.SetBool("Walking", false);

			if (isVisitingPointOfInterest) {
				inspectPointOfInterestTime = maximumInspectPointOfInterestTime;
				animator.SetBool("Inspecting", true);
				return false;
			}

			return true;
		}

		// Check if the farmer isn't visiting a point of interest as well
		// because their destination shouldn't be changed when their moving to
		// it.
		if (!moveDestinationSet && !isVisitingPointOfInterest) {
			if (canVisitPointOfInterest) {
				GoToPointOfInterest();

				// Check if the farmer is walking to a point of interest.
				if (moveDestinationSet) {
					return false;
				}
			} else {
				SetRandomWonderPoint();
			}
		}

		return false;

		void SetRandomWonderPoint() {
			const int maxRotation = 40;
			int rotationAroundYAxis = Random.Range(-maxRotation, maxRotation);
			// Creates a rotation to offset the move direction by.
			// Helps add some unpredictability to the farmer's wonder action.
			Quaternion moveRotation = Quaternion.Euler(0, rotationAroundYAxis, 0);
			Vector3 newMoveDirection = Vector3.zero;
			// A direction towards the wonder point that most closely matches
			// this game-object's forward direction.
			Vector3 directionToAlignedPoint = wonderPointManager.GetDirectionToClosestAlignedPoint(transform.forward);

			if (directionToAlignedPoint != Vector3.zero) {
				newMoveDirection = moveRotation * directionToAlignedPoint;
			} else if (wonderPointManager.AverageDirectionToOpenSpace != Vector3.zero) {
				newMoveDirection = moveRotation * wonderPointManager.AverageDirectionToOpenSpace;
			} else if (wonderPointManager.DirectionToFurthestPointInOpenSpace != Vector3.zero) {
				newMoveDirection = moveRotation * wonderPointManager.DirectionToFurthestPointInOpenSpace;
			} else {
				newMoveDirection = transform.forward;
			}

			const int moveDistance = 3;
			// Get a position ahead of the agent.
			Vector3 wonderDestination = transform.position + newMoveDirection * moveDistance;
			NavMesh.SamplePosition(wonderDestination, out NavMeshHit navMeshHit, moveDistance, NavMesh.AllAreas);
			const float turnAroundThreshold = 2.8f;

			// Check if the farmer is at the edge of the nav mesh.
			if (navMeshHit.hit && Vector3.Distance(wonderDestination, navMeshHit.position) >= turnAroundThreshold) {
				Vector3 reverseMoveDirection = moveRotation * transform.position - wonderDestination;
				wonderDestination = transform.position + reverseMoveDirection * moveDistance;
			}

			moveDestinationSet = navMeshAgent.SetDestination(wonderDestination);
			animator.SetBool("Walking", true);
			navMeshAgent.speed = walkSpeed;
			animator.SetFloat("MoveMultiplier", walkPlaybackSpeedMultiplier);
		}

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
			navMeshAgent.speed = walkSpeed;
			animator.SetFloat("MoveMultiplier", walkPlaybackSpeedMultiplier);
			visitPointOfInterestTime = maximumVisitPointOfInterestTime;
			canVisitPointOfInterest = false;
			isVisitingPointOfInterest = true;
		}
	}

	/// <summary>
	/// Makes the farmer search for the player if their whereabouts are unknown,
	/// by moving to the players last known position and wondering around.
	/// </summary>
	/// <returns> True when the action has completed. </returns>
	private bool SearchForPlayer() {
		if (canSeePlayer) {
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

		Vector3 moveDestination = transform.position;

		if (seenPlayerRecently) {
			moveDestination = playersLastKnownPosition;
		} else if (heardPlayerRecently) {
			moveDestination = playersLastKnownSoundCuePosition;
		} else if (noticedPlayerClue) {
			moveDestination = playersCluePosition;
		} else if (noticedBrokenObject) {
			moveDestination = brokenObjectPosition;
		}
		
		// Move to the player's last known position.
		moveDestinationSet = navMeshAgent.SetDestination(moveDestination);
		animator.SetBool("Walking", true);
		navMeshAgent.speed = walkSpeed;
		animator.SetFloat("MoveMultiplier", walkPlaybackSpeedMultiplier);
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
			Vector3 chaseDestination = playersLastKnownPosition - directionToPlayer * spaceBetweenAgentAndPlayer;
			moveDestinationSet = navMeshAgent.SetDestination(chaseDestination);
			animator.SetBool("Walking", true);
			navMeshAgent.speed = chaseSpeed;
			animator.SetFloat("MoveMultiplier", chasePlaybackSpeedMultiplier);
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
		if (!CanCatchPlayer() && catchAnimationState == AnimationStates.NotStarted) {
			return true;
		}

		switch (catchAnimationState) {
			case AnimationStates.NotStarted: {
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

				if (catchCollidersEnabled && catchColliderIsTouchingPlayer && !heldObject) {
					// Concealment objects become children of the concealment
					// point, so check to see if one exists to verify the
					// player's actually concealed.
					if (inputHandler.isConcealed && player.ConcealmentPoint.transform.childCount > 0) {
						heldObject = player.ConcealmentPoint.transform.GetChild(0).gameObject;
						inputHandler.isGrabbing = false;
					} else {
						hasCaughtPlayer = true;
						heldObject = player.gameObject;
						HoldOntoPlayer(true);
					}
				}

				if (hasCaughtPlayer) {
					heldObject.transform.position = carryPosition.position;
				} else {
					HoldOntoPlayer(false);
				}

				break;
			}
			case AnimationStates.Ended: {
				StopLookAtCoroutine();
				catchAnimationState = AnimationStates.NotStarted;
				return true;
			}
			default: {
				StopCatchingPlayer();
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Makes the farmer look at a transform by rotating them over time until 
	/// they're looking the correct direction.
	/// </summary>
	/// <param name="transformToLookAt"> The transform to look at. </param>
	/// <returns> IEnumerator for the coroutine. </returns>
	private IEnumerator LookAtCoroutine(Transform transformToLookAt) {
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

	/// <summary>
	/// Stops the coroutine that makes the farmer look at a target transform.
	/// </summary>
	private void StopLookAtCoroutine() {
		rotationAmount = 0;

		if (lookAtCoroutine != null) {
			StopCoroutine(lookAtCoroutine);
			lookAtCoroutine = null;
		}
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
			hasCaughtPlayer = false;
			heldObject = null;
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
			navMeshAgent.speed = chaseSpeed;
			animator.SetFloat("MoveMultiplier", chasePlaybackSpeedMultiplier);
			animator.SetBool("Carrying", true);
		}

		return false;
	}
	#endregion

	#region Methods for Updating Timers
	private void UpdateTimers() {
		BlindAndDeaf();
		Stunned();

		if (visitPointOfInterestTime > 0) {
			visitPointOfInterestTime -= Time.deltaTime;

			if (visitPointOfInterestTime <= 0) {
				canVisitPointOfInterest = true;
			}
		}

		if (inspectPointOfInterestTime > 0) {
			inspectPointOfInterestTime -= Time.deltaTime;

			if (inspectPointOfInterestTime <= 0) {
				animator.SetBool("Inspecting", false);
				isVisitingPointOfInterest = false;
			}
		}
	}

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
				animator.SetBool("GettingUp", true);
				isGettingUp = true;
			}
		}
	}
	#endregion

	#region Miscellaneous Methods
	/// <summary>
	/// Updates the farmer's falshlight's colour based on the farmer's 
	/// awareness and current state.
	/// </summary>
	private void UpdateFlashlightColour() {
		// The intended colour intensity of the farmer's flashlight based on
		// the farmer's awareness level.
		float awarenessColourIntensity = awareness / maximumAwareness;
		const float half = 0.5f;
		float searchingColourIntensity = seenPlayerRecently ||
			heardPlayerRecently ||
			noticedPlayerClue ||
			noticedBrokenObject ? half : 0.0f;
		float colourIntensity = awarenessColourIntensity >= searchingColourIntensity ? awarenessColourIntensity : searchingColourIntensity;
		flashlight.ChangeColour(Mathf.Clamp(colourIntensity, 0.0f, 1.0f));
	}
	
	/// <summary>
	/// Causes the farmer's sensors to forget they detected the player, and 
	/// makes the farmer completely forget about the player in every capacity.
	/// </summary>
	private void ForgetAboutPlayer() {
		visualSensor.ForgetObject(player.gameObject);
		canSeePlayer = false;
		seenPlayerRecently = false;
		audioSensor.ForgetObject(player.gameObject);
		heardPlayerRecently = false;
		noticedPlayerClue = false;
		noticedBrokenObject = false;
	}

	/// <summary>
	/// Checks if the agent has arrived at their destination.
	/// </summary>
	/// <returns> True if the agent is standing close enough to their destination. </returns>
	private bool HasArrivedAtDestination() {
		// Once a destination is reached, the path is automatically removed
		// from the nav mesh agent, but a pending path means the agent hasn't
		// reached their destination yet.
		if (moveDestinationSet &&
			!navMeshAgent.hasPath &&
			!navMeshAgent.pathPending) {
			moveDestinationSet = false;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Disables the processing of player input and movement.
	/// </summary>
	/// <param name="holdOntoPlayer"> True if the farmer should hold the player 
	/// in place. </param>
	private void HoldOntoPlayer(bool holdOntoPlayer) {
		holdOntoPlayer = !holdOntoPlayer;
		player.enabled = holdOntoPlayer;
		player.gameObject.GetComponent<InputHandler>().enabled = holdOntoPlayer;
		player.gameObject.GetComponent<Rigidbody>().useGravity = holdOntoPlayer;
	}

	/// <summary>
	/// Resets most of the attached animator's parameters to their default 
	/// values.
	/// </summary>
	private void ResetAnimatorParameters() {
		animator.ResetTrigger("Idling");
		animator.SetBool("Walking", false);
		animator.SetBool("Carrying", false);
		animator.SetBool("Catching", false);
		animator.ResetTrigger("CatchingTrigger");
		animator.SetBool("Stunned", false);
		animator.ResetTrigger("StunnedTrigger");
		animator.SetBool("Inspecting", false);
		animator.SetBool("GettingUp", false);
		animator.avatar = null;
	}
	#endregion
}
