// Author: Liam Bansal
// Collaborator: N/A
// Created On: 9/5/2023 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a trigger collider used for detecting geometry when the farmer AI 
/// is executing their wondering action.
/// </summary>
public class WonderPoint : MonoBehaviour {
    public bool IsTouching {
        get { return isTouching; }
        private set { isTouching = value; }
    }

    private bool isTouching = false;
    [SerializeField, Tooltip("Mark any layers that should trigger collisions " +
        "with this game-object.")]
    private LayerMask collisionLayers = default;  

    private void OnTriggerEnter(Collider other) {
        TriggeredCollision(other, true);
    }

    private void OnTriggerStay(Collider other) {
        TriggeredCollision(other, true);
    }

    private void OnTriggerExit(Collider other) {
        TriggeredCollision(other, false);
    }

    private void TriggeredCollision(Collider collision, bool enteringCollision) {
        int bitshiftedLayerValue = 1 << collision.gameObject.layer;

        if ((bitshiftedLayerValue & collisionLayers) == 0) {
            return;
        }

        if (enteringCollision) {
            isTouching = true;
        } else {
            isTouching = false;
        }
    }
}
