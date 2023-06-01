// Author: Liam Bansal
// Collaborator: N/A
// Created On: 3/5/2023

using UnityEngine;

/// <summary>
/// Designed to act as an intermediate script between the farmer's animator 
/// component and farmer script.
/// It informs the farmer script of animation events.
/// </summary>
public class FarmerAnimator : MonoBehaviour {
	private Farmer farmerScript = null;

	public void EnableCatchCollider() {
		farmerScript.ToggleCatchCollider(true);
	}

	public void DisableCatchCollider() {
		farmerScript.ToggleCatchCollider(false);
	}

	public void CatchAnimationStarted() {
		farmerScript.CatchAnimationStarted();
	}

	public void CatchAnimationEnded() {
		farmerScript.CatchAnimationEnded();
	}

	public void StunAnimationStarted() {
		farmerScript.StunAnimationStarted();
	}

	public void GettingUpAnimationEnded() {
		farmerScript.GettingUpAnimationEnded();
	}

	private void Start() {
		farmerScript = GameObject.FindGameObjectWithTag("Farmer").GetComponent<Farmer>();
	}
}
