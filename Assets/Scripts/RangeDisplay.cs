using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// in-game, interactable menu that lets you adjust the range to a target
// communicates with a DummyInput InputSource, reacting to OnDeath events
// TODO: fix falling-through-floor bug when target is healed from compromised mobility crawl
public class RangeDisplay : Display {
	public DummyInput dummyInput;
	public GameObject closeRange;
	public GameObject farRange;
	public GameObject origin;

	public Slider rangeSlider;
	public Text rangeText;
	public float adjustmentVelocity;

	AudioSource deathSound;

	float yaw;
	float distance;
	float minDistance;
	float rangeFractionTarget;
	float rangeFraction;
	float velocityFraction;

	// Use this for initialization
	void Start () {
		yaw = origin.transform.rotation.eulerAngles.y;

		//Adds a listener to the main slider and invokes a method when the value changes.
		rangeSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

		minDistance = (closeRange.transform.position - origin.transform.position).magnitude;
		distance = (farRange.transform.position - closeRange.transform.position).magnitude;
		velocityFraction = adjustmentVelocity / distance;

		rangeFractionTarget = rangeSlider.value;
		SetRangeText();

		StopDisplay();

		deathSound = GetComponent<AudioSource>();
		dummyInput.controller.armor.onOrganChange += OnOrganChange;
	}

	// when an organ is damaged, play the death sound and heal
	public void OnOrganChange(bool isDead)
	{
		if (isDead)
		{
			deathSound.Play();
			dummyInput.controller.armor.Heal();
			dummyInput.controller.ambulation = Ambulation.Stand;
		}
	}

	// Update the target's position until the target range has been reached
	void Update() {
		if (rangeFraction < rangeFractionTarget)
		{
			rangeFraction = Mathf.Min(rangeFraction + velocityFraction * Time.deltaTime, rangeFractionTarget);
			Vector3 position = Vector3.Lerp(closeRange.transform.position, farRange.transform.position, rangeFraction);
			dummyInput.controller.SetLocation(position, yaw);
		}
		else if (rangeFraction > rangeFractionTarget)
		{
			rangeFraction = Mathf.Max(rangeFraction - velocityFraction * Time.deltaTime, rangeFractionTarget);
			Vector3 position = Vector3.Lerp(closeRange.transform.position, farRange.transform.position, rangeFraction);
			dummyInput.controller.SetLocation(position, yaw);
		}
	}

	// Invoked when the value of the slider changes.
	public void ValueChangeCheck()
	{
		rangeFractionTarget = rangeSlider.value;
		SetRangeText();
	}

	void SetRangeText()
	{
		float range = rangeFractionTarget * distance + minDistance;
		rangeText.text = range.ToString("N1") + " m";
	}

	// implements the Display base class by setting the menu's interactability
	public override void StartDisplay()
	{
		rangeSlider.interactable = true;
	}

	public override void StopDisplay()
	{
		rangeSlider.interactable = false;
	}
}
