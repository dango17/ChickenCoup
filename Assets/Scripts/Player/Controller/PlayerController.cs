// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 27/01/2023

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

namespace DO
{
    //playerManager that handles movement types and the relevant logic to execute.
    public class PlayerController : MonoBehaviour, DetectionPoint.IDetectionPointData {
		/// <summary>
		/// True when the player is considered as hidden e.g. player is under 
        /// a table or in a vent.
		/// </summary>
		public bool IsHiding {
            get;
            private set;
        }
		public int VisibleDetectionPoints {
			get;
			set;
		}
        /// <summary>
        /// The amount of detection points placed around the character's model.
        /// </summary>
        public int NumberOfDetectionPoints {
            get { return detectionPoints.Length; }
            private set { }
        }
        public float MaximumMovementSpeed {
            // Add the player's move speed because it reflects the increase in
            // speed from eating foods.
            get { return maximumSprintSpeed + moveSpeed; }
            private set { }
        }

		[Header("Speeds")]
        [SerializeField] public float moveSpeed = 0.4f;
        [SerializeField] public float sprintSpeed = 0.8f;
        [SerializeField] public float rotateSpeed = 0.2f;
        [SerializeField] public float FPRotationSpeed = 0.2f; 
        [Header("Jumping")]
        [SerializeField] public float jumpHeight = 2f;
        [SerializeField] public float timeToJumpApex = 0.5f;
        [SerializeField] public float fallForce = 2f;
        [SerializeField] private float gravity;
        [SerializeField] public AnimationCurve jumpCurve;
        [SerializeField] public float fallMultiplier = 2.5f;
        [SerializeField] public float lowJumpMultiplier = 2f;
        [Header("Ground-Check")]
        [SerializeField] public LayerMask groundLayer;
        [SerializeField] public float raycastDistance = 0.2f; 
        [Header("Cover")]
        [SerializeField] public float wallSpeed = 0.2f;
        [SerializeField] public float wallCheckDistance = 0.2f;
        [SerializeField] public LayerMask coverLayer;
        [Header("Peeking")]
        [SerializeField] public float wallCamXPosition = 1;
        [SerializeField] public Transform wallCameraParent;
        [Header("EggLaying")]
        [SerializeField] public GameObject eggPrefab;
        [SerializeField] public Transform spawnPoint;
        [SerializeField] public float cooldownTime = 1f;
        [SerializeField] public float timeSinceLastSpawn = 3f; 
        [Header("Clucking")]
        [SerializeField] public AudioClip[] cluckSounds;
        [SerializeField] private AudioSource audioSource;
        [Header("Flags")]
        [SerializeField] public bool isOnCover;
        [SerializeField] public bool isGrounded;
        [SerializeField] public bool isInFreeLook;
        [SerializeField] public bool isFPMode;
        [SerializeField] public bool objectInHand;
        [SerializeField] public bool playerisArrested; 


        [HideInInspector] public Transform mTransform;
        [HideInInspector] public Animator animator;
        [HideInInspector] new Rigidbody rigidbody = null;
        [HideInInspector] public SkinnedMeshRenderer meshRenderer;
        [HideInInspector] public InputHandler inputHandler;

        /// <summary>
        /// Use the property for this variable when getting it's value,
        /// so the maximum speed factors in the speed boost from foods.
        /// </summary>
		private readonly float maximumSprintSpeed = 5.0f;
		private DetectionPoint[] detectionPoints = null;

		public void EnableFirstPerson(bool enableFirstPerson) {
			inputHandler.controller.isFPMode = enableFirstPerson;
            inputHandler.playerLeftEye.SetActive(false); 
            inputHandler.playerRightEye.SetActive(false); 
		}

        private void Start()
        {
            mTransform = this.transform;
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
            inputHandler = GetComponent<InputHandler>();
			detectionPoints = GetComponentsInChildren<DetectionPoint>();

            gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);

            foreach (DetectionPoint detectionPoint in detectionPoints) {
                detectionPoint.SetDataSource(this);
			}

            audioSource = GetComponentInChildren<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }


        private void Update()
        {
            //Increment time since last eggSpawn
            timeSinceLastSpawn += Time.deltaTime;

            //Ground Check the player
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false; 
            }

            if(isGrounded == true && isOnCover == false)
            {
                animator.Play("Locomotion");
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("CoverForHiding")) {
				IsHiding = true;
			}
        }

        /// <summary>
        /// Prevents the player from being revealed when entering cover
        /// that's adjacent to the one their currently in, because they'll 
        /// exit the old cover only after entering the new one, thus triggering
        /// isHidden to be set to false after entering the new cover.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerStay(Collider other) {
			if (other.CompareTag("CoverForHiding")) {
				IsHiding = true;
			}
		}

        private void OnTriggerExit(Collider other) {
			if (other.CompareTag("CoverForHiding")) {
				IsHiding = false;
			}
		}

        public void WallMovement(Vector3 moveDirection, Vector3 normal, float delta)
        {
            float dot = Vector3.Dot(moveDirection, Vector3.forward);
            Vector3 wallCameraTargetPosition = Vector3.zero; 
            if (dot < 0)
            {
                moveDirection.x *= -1; 
            }
            HandleRotation(normal, delta);
 
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normal);
            Debug.DrawRay(mTransform.position, projectedVelocity, Color.blue);

            Vector3 relativeDir = mTransform.InverseTransformDirection(projectedVelocity);

            Vector3 origin = mTransform.position;
            origin.y += 1;

            if (Mathf.Abs(relativeDir.x) > 0.01f)
            {
                if (relativeDir.x > 0)
                    origin += mTransform.right * wallCheckDistance;

                if (projectedVelocity.x < 0)
                    origin -= mTransform.right * wallCheckDistance; 

                Debug.DrawRay(origin, -normal, Color.red);
                if (Physics.Raycast(origin, -normal, out RaycastHit hit, 2f))
                {
                    //Do nothing
                }
                else
                {
                    projectedVelocity = Vector3.zero;
                    wallCameraTargetPosition.x = wallCamXPosition * ((relativeDir.x < 0) ? -1:1) ; 
                    relativeDir.x = 0; 
                }
            }
            else
            {
                projectedVelocity = Vector3.zero;
                relativeDir.x = 0; 
            }

            rigidbody.velocity = projectedVelocity * wallSpeed;

            float m = 0;
       
            //Looking inward therefore 
            m = relativeDir.x;

            if(m < 0.1f && m > -0.1f)
            {
                m = 0;
            }
            else
            {
                m = (m < 0) ? -1 : 1; 
            }

            animator.SetFloat("movement", m, 0.1f, delta);
            wallCameraParent.localPosition = Vector3.Lerp(wallCameraParent.localPosition, wallCameraTargetPosition, delta / 0.2f); 
        }

        public void Move(Vector3 moveDirection, float delta)
        { 
            rigidbody.velocity = moveDirection * moveSpeed;
            HandleRotation(moveDirection, delta);         
        }

        public void HandleRotation(Vector3 lookDir, float delta)
        {
            //Handle Rotation     
            if (lookDir == Vector3.zero)
                lookDir = mTransform.forward;

            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            mTransform.rotation = Quaternion.Slerp(mTransform.rotation, lookRotation, delta / rotateSpeed);
        }

        public void FPRotation(float horizontal, float delta)
        {
            Vector3 targetEuler = mTransform.eulerAngles;
            targetEuler.y += horizontal * delta / FPRotationSpeed; 

            mTransform.eulerAngles = targetEuler; 
        }

        public void HandleAnimatorStates()
        {
            animator.SetBool("isOnCover", isOnCover);
        }

        public void HandleMovementAnimations(float moveAmount, float delta)
        {
            //Set moveAmount Values 
            float m = moveAmount;
            if (m > 0.1f && m < 0.51f)
                m = 0.5f;

            if (m > 0.51f)
                m = 1;

            if (m < 0.1f)
                m = 0f;

            animator.SetFloat("movement", m, 0.1f, delta);
        }

        public void HandleEggSpawning()
        {
            inputHandler.isLayingEgg = true;
            timeSinceLastSpawn = 0f;

            Instantiate(eggPrefab, spawnPoint.position, Quaternion.identity);
        }

        public void HandleEggCoolDown()
        {
            inputHandler.isLayingEgg = false;
        }

        public void Jump()
        {
            StartCoroutine(HandleJump());
        }

        public IEnumerator HandleJump()
        {
            if(playerisArrested = true)
            { 
                 inputHandler.isJumping = false;

            float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
            float timeToReachApex = Mathf.Sqrt(-2 * jumpHeight / gravity);

            Vector3 jumpDirection = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);
            float timeElapsed = 0f;

               while (timeElapsed < timeToReachApex)
              {
                float t = timeElapsed / timeToReachApex;
                float curveValue = jumpCurve.Evaluate(t);

                Vector3 displacement = jumpDirection * curveValue * Time.deltaTime;
                transform.position += displacement;

                timeElapsed += Time.deltaTime;
                yield return null;
               } 
            }
        }

        public void handleFalling()
        {
            animator.Play("Float");

            rigidbody.AddForce(Vector3.down * 9.81f * rigidbody.mass);
            if (rigidbody.velocity.y < 0)
            {
                rigidbody.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * rigidbody.mass, ForceMode.Acceleration);
            }
            else if (rigidbody.velocity.y > 0 && !inputHandler.isJumping)
            {
                rigidbody.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * rigidbody.mass, ForceMode.Acceleration);
            }
        }

        public void HandleClucking()
        {
            int randomIndex = Random.Range(0, cluckSounds.Length);
            audioSource.clip = cluckSounds[randomIndex];
            audioSource.spatialBlend = 1;
            audioSource.Play();
            AudioSensor.NotifyNearbyAudioSensors(audioSource, transform.position);
        }
    }
}
