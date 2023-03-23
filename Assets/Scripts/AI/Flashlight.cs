// Author: Liam Bansal
// Collaborator: N/A
// Created On: 8/3/2023

using UnityEngine;

/// <summary>
/// Controls the field of view and colour of the farmer's flashlight.
/// </summary>
public class Flashlight : MonoBehaviour {
	private Light spotlight = null;
	[SerializeField, Tooltip("Colour of the flashlight when the player has not " +
		"been spotted by the farmer.")]
	private Color unspottedLightColour = Color.white;
	[SerializeField, Tooltip("Colour of the flashlight once the player has " +
		"been spotted by the farmer.")]
	private Color spottedLightColour = Color.red;

	/// <summary>
	/// Changes the colour of the flashlight based whether or not the farmer 
	/// can see the player.
	/// </summary>
	/// <param name="colourChange"> Percentage to lerp between the unspotted and spotted colours. </param>
	public void ChangeColour(float colourChange) {
		Color lerpedColour = new Color(Mathf.Lerp(unspottedLightColour.r, spottedLightColour.r, colourChange),
			Mathf.Lerp(unspottedLightColour.g, spottedLightColour.g, colourChange),
			Mathf.Lerp(unspottedLightColour.b, spottedLightColour.b, colourChange));
		spotlight.color = lerpedColour;
	}

	private void Awake() {
		spotlight = GetComponent<Light>();
	}

	private void Start() {
		VisualSensor farmerVisualSensor = GameObject.FindGameObjectWithTag("Farmer").GetComponentInChildren<VisualSensor>();

		if (!farmerVisualSensor) {
			return;
		}

		spotlight.spotAngle = farmerVisualSensor.FieldOfView;
		spotlight.color = unspottedLightColour;
	}
}
