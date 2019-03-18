using System;
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

	[SerializeField] Image upperLactateImage = null;
	[SerializeField] Image bloodLactateImage = null;
	[SerializeField] Image lowerLactateImage = null;
	[SerializeField] Image hydrationImage = null;
	[SerializeField] Image hungerImage = null;

	[SerializeField] Color optimalColor = Color.green;
	[SerializeField] Color intermediateColor = Color.yellow;
	[SerializeField] Color criticalColor = Color.red;

	[SerializeField] GameObject activityOverrideLabel = null;
	[SerializeField] Text activityOverrideText = null;
	[SerializeField] Text activityOverrideTime = null;

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
				input.controller.physique.onPhysiqueUpdated += PhysiqueUpdated;
			}
		}
		else
		{
			// update HUD elements every updateInterval seconds
			if (intervalTimer <= 0f)
			{
				intervalTimer = updateInterval;

				UpdateTarget();
				UpdateActivator();
				UpdatePose();
				UpdateSpeed();
			}
			else
			{
				intervalTimer -= Time.deltaTime;
			}
		}
	}

	private void OnDestroy()
	{
		input.controller.physique.onPhysiqueUpdated -= PhysiqueUpdated;
	}

	private void PhysiqueUpdated(Physique physique)
	{
		UpdateStamina(physique);
		UpdateHTS(physique);
		UpdateActivityOverride(physique);
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
	void UpdateStamina(Physique physique)
	{
		calorieText.text = (input.controller.physique.GetCalorieRate() * 3600f * 24f).ToString("N0");

		upperLactate.barValue = input.controller.physique.GetUpperLactateFraction();
		bloodLactate.barValue = input.controller.physique.GetBloodLactateFraction();
		lowerLactate.barValue = input.controller.physique.GetLowerLactateFraction();

		SetImageColor(upperLactateImage, input.controller.physique.GetUpperLactateFraction());
		SetImageColor(bloodLactateImage, input.controller.physique.GetBloodLactateFraction());
		SetImageColor(lowerLactateImage, input.controller.physique.GetLowerLactateFraction());
	}

	void SetImageColor(Image image, float fraction)
	{
		if (fraction < 0.5f)
		{
			image.color = Color.Lerp(optimalColor, intermediateColor, fraction * 2f);
		}
		else
		{
			image.color = Color.Lerp(intermediateColor, criticalColor, (fraction - 0.5f) * 2f);
		}
	}

	void SetImageOpacity(Image image, float fraction, bool fullAlphaAtZero)
	{
		Color newColor = image.color;
		if (fullAlphaAtZero)
		{
			newColor.a = 1f - fraction;
		}
		else
		{
			newColor.a = fraction;
		}
		image.color = newColor;
	}

	// updates the hunger/thirst/sleep information
	void UpdateHTS(Physique physique)
	{
		upperGlycogen.barValue = physique.GetUpperGlycogenFraction();
		liverGlycogen.barValue = physique.GetLiverGlycogenFraction();
		lowerGlycogen.barValue = physique.GetLowerGlycogenFraction();
		protein.barValue = physique.GetProteinFraction();
		hydration.barValue = physique.GetHydrationFraction();

		SetImageOpacity(hydrationImage, physique.GetHydrationFraction(), true);
		float mostHungry = Mathf.Min(new float[]{ physique.GetUpperGlycogenFraction(),
													physique.GetLiverGlycogenFraction(),
													physique.GetLowerGlycogenFraction(),
													physique.GetProteinFraction()});
		SetImageOpacity(hungerImage, mostHungry, true);
	}

	void UpdateActivityOverride(Physique physique)
	{
		ActivityOverride ao = physique.GetActivityOverride();
		if (ao == null)
		{
			activityOverrideLabel.SetActive(false);
		}
		else
		{
			activityOverrideLabel.SetActive(true);
			activityOverrideText.text = ao.GetActionData().name;
			activityOverrideTime.text = (physique.GetActivityOverrideTimer() / 60f).ToString("N0") + " min";
		}
	}
}
