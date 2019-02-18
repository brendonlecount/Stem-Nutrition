using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// updates the HUD
public class HUDController : MonoBehaviour {
	public float updateInterval;
	public Text crouchingText;
	public Text standingText;
	public Text proneText;
	public Text targetText;
	public Text targetCellText;
	public Text activationText;
	public Text speedText;

	public GameObject targetPanel;
	public GameObject activatorPanel;

	public Text calorieText;
	public Bar upperGlycogen;
	public Bar liverGlycogen;
	public Bar lowerGlycogen;
	public Bar protein;
	public Bar hydration;

	public Bar upperLactate;
	public Bar bloodLactate;
	public Bar lowerLactate;

	PlayerInput input;
	Crosshair crosshair;

	float intervalTimer;

	// Update teh menu every updateInterval seconds.
	// also poll for PlayerController until it is found (can't go in Start or Awake block because
	// PlayerController is instantiated after the scene is loaded.)
	// TODO: make PlayerController a singleton? won't fix problem since singleton will return null
	// until PlayerController is instantiated, but will look cleaner.
	void Update () {
		if (input == null)
		{
			// poll for PlayerController
			GameObject go = GameObject.FindGameObjectWithTag("PlayerController");
			if (go != null)
			{
				input = go.GetComponent<PlayerInput>();
				crosshair = go.GetComponent<Crosshair>();
			}
		}
		else
		{
			// update HUD elements every updateInterval seconds
			if (intervalTimer <= 0f)
			{
				intervalTimer = updateInterval;

				UpdatePose();
				UpdateTarget();
				UpdateActivator();
				UpdateSpeed();
				UpdateStamina();
				UpdateHTS();
			}
			else
			{
				intervalTimer -= Time.deltaTime;
			}
		}
	}

	// TODO: have one text object and set text from poseName obtained from controller.
	void UpdatePose()
	{
		switch (input.controller.pose)
		{
			case Pose.Crouching:
				crouchingText.enabled = true;
				standingText.enabled = false;
				proneText.enabled = false;
				break;
			case Pose.Standing:
				crouchingText.enabled = false;
				standingText.enabled = true;
				proneText.enabled = false;
				break;
			case Pose.Prone:
				crouchingText.enabled = false;
				standingText.enabled = false;
				proneText.enabled = true;
				break;
		}
	}

	// Updates information related to the target currently under the crosshair, if any
	void UpdateTarget()
	{
		ConditionComponent target = crosshair.GetTargetConditionComponent();
		int targetCell = crosshair.GetTargetArmorCell();
		if (target != null)
		{
			targetText.text = target.componentName + " " + (100f * target.GetConditionFraction()).ToString("N0") + "%";
			if (targetCell != -1)
			{
				targetCellText.text = targetCell.ToString("N0") + " " + (100f * target.GetCellConditionFraction(targetCell)).ToString("N0") + "%";
			}
			else
			{
				targetCellText.text = "";
			}
			targetPanel.SetActive(true);
		}
		else
		{
			targetPanel.SetActive(false);
		}
	}

	// Updates information related to the currently targeted activator, if any
	void UpdateActivator()
	{
		Stem.Activator targetActivator = crosshair.GetTargetActivator();
		if (targetActivator != null)
		{
			if (MenuManager.menuMode == MenuModes.none)
			{
				activationText.text = targetActivator.activatorName;
				activatorPanel.SetActive(true);
			}
			else
			{
				activatorPanel.SetActive(false);
			}
		}
		else
		{
			activatorPanel.SetActive(false);
		}
	}

	// updates the speedometer
	void UpdateSpeed()
	{
		speedText.text = (input.controller.GetSpeed() * 2.237f).ToString("N1") + " mph";
	}

	// updates the stamina (lactic acid) bars
	void UpdateStamina()
	{
		calorieText.text = (input.controller.physique.GetCalorieRate() * 3600f * 24f).ToString("N0");

		upperLactate.barValue = input.controller.physique.GetUpperLactateFraction();
		bloodLactate.barValue = input.controller.physique.GetBloodLactateFraction();
		lowerLactate.barValue = input.controller.physique.GetLowerLactateFraction();
	}

	// updates the hunger/thirst/sleep information
	void UpdateHTS()
	{
		upperGlycogen.barValue = input.controller.physique.GetUpperGlycogenFraction();
		liverGlycogen.barValue = input.controller.physique.GetLiverGlycogenFraction();
		lowerGlycogen.barValue = input.controller.physique.GetLowerGlycogenFraction();
		protein.barValue = input.controller.physique.GetProteinFraction();
		hydration.barValue = input.controller.physique.GetHydrationFraction();
	}
}
