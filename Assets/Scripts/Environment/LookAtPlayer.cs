using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class LookAtPlayer : MonoBehaviour
    {
        [SerializeField] private Transform target;

        void Update()
        {
            transform.LookAt(target);
        }
    }
}