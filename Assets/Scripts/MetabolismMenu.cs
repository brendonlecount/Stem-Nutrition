using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MetabolismMenu : MonoBehaviour
{
	enum GroupVisibility { Stamina, Protein, Hydration, Mass }

	[SerializeField] float updateInterval = 0.1f;

	[Header("Stamina Group")]
	[SerializeField] Button staminaButton = null;
	[SerializeField] GameObject staminaGroup = null;
	[SerializeField] Bar bloodLacticAcid = null;
	[SerializeField] Bar upperLacticAcid = null;
	[SerializeField] Bar lowerLacticAcid = null;
	[SerializeField] Bar liverGlycogen = null;
	[SerializeField] Bar upperGlycogen = null;
	[SerializeField] Bar lowerGlycogen = null;

	[Header("Protein Group")]
	[SerializeField] Button proteinButton = null;
	[SerializeField] GameObject proteinGroup = null;
	[SerializeField] Bar protein = null;

	[Header("Hydration Group")]
	[SerializeField] Button hydrationButton = null;
	[SerializeField] GameObject hydrationGroup = null;
	[SerializeField] Bar hydration = null;

	[Header("Mass Group")]
	[SerializeField] Button massButton = null;
	[SerializeField] GameObject massGroup = null;
	[SerializeField] Bar bodyWeight = null;
	[SerializeField] Bar bodyFat = null;
	[SerializeField] Bar upperFastTwitch = null;
	[SerializeField] Bar lowerFastTwitch = null;
	[SerializeField] Bar upperSlowTwitch = null;
	[SerializeField] Bar lowerSlowTwitch = null;

	GroupVisibility groupVisibility = GroupVisibility.Stamina;
	PlayerInput player;
    
	private void Awake()
	{
		GameObject go = GameObject.FindGameObjectWithTag("PlayerController");
		if (go != null)
		{
			player = go.GetComponent<PlayerInput>();
		}
	}

	private void OnEnable()
	{
		SetGroupVisibility(GroupVisibility.Stamina);
		player.controller.physique.onPhysiqueUpdated += OnPhysiqueUpdated;
	}

	private void OnDisable()
	{
		player.controller.physique.onPhysiqueUpdated -= OnPhysiqueUpdated;
	}

	public void StaminaGroupClicked() { SetGroupVisibility(GroupVisibility.Stamina); }
	public void ProteinGroupClicked() { SetGroupVisibility(GroupVisibility.Protein); }
	public void HydrationGroupClicked() { SetGroupVisibility(GroupVisibility.Hydration); }
	public void MassGroupClicked() { SetGroupVisibility(GroupVisibility.Mass); }

	void SetGroupVisibility(GroupVisibility groupVisibility)
	{
		this.groupVisibility = groupVisibility;

		staminaGroup.SetActive(groupVisibility == GroupVisibility.Stamina);
		proteinGroup.SetActive(groupVisibility == GroupVisibility.Protein);
		hydrationGroup.SetActive(groupVisibility == GroupVisibility.Hydration);
		massGroup.SetActive(groupVisibility == GroupVisibility.Mass);

		staminaButton.interactable = groupVisibility != GroupVisibility.Stamina;
		proteinButton.interactable = groupVisibility != GroupVisibility.Protein;
		hydrationButton.interactable = groupVisibility != GroupVisibility.Hydration;
		massButton.interactable = groupVisibility != GroupVisibility.Mass;
	}

	public void OnPhysiqueUpdated(Physique physique)
	{
		switch (groupVisibility)
		{
			case GroupVisibility.Stamina:
				UpdateStaminaGroup(physique);
				break;
			case GroupVisibility.Protein:
				UpdateProteinGroup(physique);
				break;
			case GroupVisibility.Hydration:
				UpdateHydrationGroup(physique);
				break;
			case GroupVisibility.Mass:
				UpdateMassGroup(physique);
				break;
		}
	}

	private void UpdateStaminaGroup(Physique physique)
	{
		upperLacticAcid.text = (1000f * physique.upperLactate).ToString("N1") + " g";
		upperLacticAcid.barValue = physique.GetUpperLactateFraction();

		bloodLacticAcid.text = (1000f * physique.bloodLactate).ToString("N1") + " g";
		bloodLacticAcid.barValue = physique.GetBloodLactateFraction();

		lowerLacticAcid.text = (1000f * physique.lowerLactate).ToString("N1") + " g";
		lowerLacticAcid.barValue = physique.GetLowerLactateFraction();

		upperGlycogen.text = (1000f * physique.glycogenUpper).ToString("N1") + " g";
		upperGlycogen.barValue = physique.GetUpperGlycogenFraction();

		liverGlycogen.text = (1000f * physique.glycogenLiver).ToString("N1") + " g";
		liverGlycogen.barValue = physique.GetLiverGlycogenFraction();

		lowerGlycogen.text = (1000f * physique.glycogenLower).ToString("N1") + " g";
		lowerGlycogen.barValue = physique.GetLowerGlycogenFraction();
	}

	private void UpdateProteinGroup(Physique physique)
	{
		protein.text = (1000f * physique.proteinDigesting).ToString("N1") + " g";
		protein.barValue = physique.GetProteinFraction();
	}

	private void UpdateHydrationGroup(Physique physique)
	{
		hydration.text = physique.hydration.ToString("N1") + " kg";
		hydration.barValue = physique.GetHydrationFraction();
	}

	private void UpdateMassGroup(Physique physique)
	{
		upperFastTwitch.text = physique.fastTwitchUpper.ToString("N1") + " kg";
		upperFastTwitch.text2 = physique.upperAnaerobicThreshold.ToString("N0") + " W";

		upperSlowTwitch.text = physique.slowTwitchUpper.ToString("N1") + " kg";
		upperSlowTwitch.text2 = physique.upperAerobicThreshold.ToString("N0") + " W";

		lowerFastTwitch.text = physique.fastTwitchLower.ToString("N1") + " kg";
		lowerFastTwitch.text2 = physique.lowerAnaerobicThreshold.ToString("N0") + " W";

		lowerSlowTwitch.text = physique.slowTwitchLower.ToString("N1") + " kg";
		lowerSlowTwitch.text2 = physique.lowerAerobicThreshold.ToString("N0") + " W";

		bodyFat.text = physique.massFat.ToString("N1") + " kg";
		bodyFat.text2 = ((physique.massFat / physique.massTotal) * 100f).ToString("N1") + "%";
		bodyFat.barValue = physique.massFat / physique.massTotal;

		bodyWeight.text = physique.massTotal.ToString("N1") + " kg";
	}
}
