// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 30/03/2023

using UnityEngine;
using UnityEngine.AI; 

namespace DO
{
    public class ChickWander : MonoBehaviour
    {
        public float moveSpeed = 1.0f;
        public float turnSpeed = 120.0f;
        public float idleTime = 3.0f;
        public AudioClip[] cluckingSounds;

        private Animator anim;
        private AudioSource audioSource;
        private NavMeshAgent navAgent;
        private Vector3 targetPosition;
        private float idleTimer;

        void Start()
        {
            anim = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            navAgent = GetComponent<NavMeshAgent>();
            targetPosition = transform.position;
            idleTimer = Random.Range(0.0f, idleTime);
        }

        void Update()
        {

            if (idleTimer > 0.0f)
            {
                idleTimer -= Time.deltaTime;
                anim.SetBool("isWalking", false);
                return;
            }

            //If the chicken has reached its target position, choose a new target position
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                targetPosition = GetRandomTargetPosition();
                idleTimer = Random.Range(0.0f, idleTime);
            }

            //Rotate towards target position
            Vector3 direction = (targetPosition - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);

            navAgent.SetDestination(targetPosition);

            //scale the aniamtion speed in accordance to movement speed
            anim.SetFloat("Speed", navAgent.velocity.magnitude / moveSpeed);

            anim.SetBool("isWalking", true);
        }

        //Get random target position within a certain range
        Vector3 GetRandomTargetPosition()
        {
            float range = 3.0f;
            Vector3 randomDirection = Random.insideUnitSphere * range;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, range, 1);
            return hit.position;
        }

        //Play random cluck sound
        public void Cluck()
        {
            if (cluckingSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, cluckingSounds.Length);
                audioSource.clip = cluckingSounds[randomIndex];
                audioSource.Play();
            }
        }
    }
}