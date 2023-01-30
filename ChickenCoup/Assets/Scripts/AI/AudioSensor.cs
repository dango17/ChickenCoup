// Written by Liam Bansal
// Date Created: 30/01/2023

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class AudioSensor : Sensor {
	protected override bool VerifyDetection(GameObject gameobject) {
		return false;
	}
}
