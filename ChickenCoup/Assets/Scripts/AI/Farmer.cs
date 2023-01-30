// Written by Liam Bansal
// Date Created: 30/01/2023

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the navigation and actions of the farmer AI agent.
/// </summary>
public class Farmer : MonoBehaviour {
	private VisualSensor visualSensor = null;
	private AudioSensor audioSensor = null;

	private void Awake() {
		visualSensor = GetComponentInChildren<VisualSensor>();
		audioSensor = GetComponentInChildren<AudioSensor>();
	}
	
	private void Start() {

	}

	private void Update() {

	}
}
