using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// menu used for displaying physique-related stats, and eating foods
public class PhysiqueMenu : MonoBehaviour {
	public float updateInterval = 0.2f;

	public Bar upperLacticAcidBar;
	public Bar bloodLacticAcidBar;
	public Bar lowerLacticAcidBar;
	public Bar upperGlycogenBar;
	public Bar liverGlycogenBar;
	public Bar lowerGlycogenBar;
	public Bar hydrationBar;
	public Bar proteinBar;
	public Bar satietyBar;

	public Bar upperFTBar;
	public Bar upperSTBar;
	public Bar lowerFTBar;
	public Bar lowerSTBar;
	public Bar bodyFatBar;
	public Bar totalMassBar;

	public Bar caloriesNIBar;
	public Bar satietyNIBar;
	public Bar fatNIBar;
	public Bar carbsNIBar;
	public Bar proteinNIBar;
	public Bar waterNIBar;
	public Bar ingestibleQuantity;
	[Tooltip("Scrollview content gameobject.")]
	public GameObject ingestiblesList;
	[Tooltip("Prefab for ingestiblesList buttons.")]
	public GameObject ingestibleButton;

	public Button consumeButton;

	PlayerInput player;
	Physique physique;
	CharacterInventory inventory;

	float intervalTimer;
	int currentIngestibleID = -1;
	IngestibleButton currentIngestibleButton;

	private void Awake()
	{
		GameObject go = GameObject.FindGameObjectWithTag("PlayerController");
		if (go != null)
		{
			player = go.GetComponent<PlayerInput>();
			inventory = player.controller.inventory;
			physique = player.controller.physique;
		}
	}

	// update list of ingestibles on enable
	private void OnEnable()
	{
		RefreshIngestibles();
	}

	// updates the list of ingestibles
	void RefreshIngestibles()
	{
		Button[] oldButtons = ingestiblesList.GetComponentsInChildren<Button>();
		foreach (Button button in oldButtons)
		{
			GameObject.Destroy(button.gameObject);
		}

		List<InventoryEntry> ingestibles = inventory.GetCategory(InventoryCategory.Ingestible);
		foreach (InventoryEntry entry in ingestibles)
		{
			GameObject nextObject = GameObject.Instantiate(ingestibleButton, ingestiblesList.transform);
			IngestibleButton nextButton = nextObject.GetComponent<IngestibleButton>();
			nextButton.InitializeButton(this, entry);
			if (currentIngestibleID == -1)
			{
				currentIngestibleID = entry.inventoryID;
			}
			if (entry.inventoryID == currentIngestibleID)
			{
				currentIngestibleButton = nextButton;
				currentIngestibleButton.SetInteractable(false);
			}
		}
		SetIngestibleInfo();
	}

	// update the bar values ever updateInterval seconds
	void Update () {
		if (intervalTimer <= 0f)
		{
			intervalTimer = updateInterval;

			upperLacticAcidBar.text = (1000f * physique.upperLactate).ToString("N1") + " g";
			upperLacticAcidBar.barValue = physique.GetUpperLactateFraction();

			bloodLacticAcidBar.text = (1000f * physique.bloodLactate).ToString("N1") + " g";
			bloodLacticAcidBar.barValue = physique.GetBloodLactateFraction();

			lowerLacticAcidBar.text = (1000f * physique.lowerLactate).ToString("N1") + " g";
			lowerLacticAcidBar.barValue = physique.GetLowerLactateFraction();

			upperGlycogenBar.text = (1000f * physique.glycogenUpper).ToString("N1") + " g";
			upperGlycogenBar.barValue = physique.GetUpperGlycogenFraction();

			liverGlycogenBar.text = (1000f * physique.glycogenLiver).ToString("N1") + " g";
			liverGlycogenBar.barValue = physique.GetLiverGlycogenFraction();

			lowerGlycogenBar.text = (1000f * physique.glycogenLower).ToString("N1") + " g";
			lowerGlycogenBar.barValue = physique.GetLowerGlycogenFraction();

			proteinBar.text = (1000f * physique.proteinDigesting).ToString("N1") + " g";
			proteinBar.barValue = physique.GetProteinFraction();

			hydrationBar.text = physique.hydration.ToString("N1") + " kg";
			hydrationBar.barValue = physique.GetHydrationFraction();

			satietyBar.text = (physique.satiety * 100f).ToString("N0") + "%";
			satietyBar.barValue = physique.satiety;

			upperFTBar.text = physique.fastTwitchUpper.ToString("N1") + " kg";
			upperFTBar.text2 = physique.upperAnaerobicThreshold.ToString("N0") + " W";

			upperSTBar.text = physique.slowTwitchUpper.ToString("N1") + " kg";
			upperSTBar.text2 = physique.upperAerobicThreshold.ToString("N0") + " W";

			lowerFTBar.text = physique.fastTwitchLower.ToString("N1") + " kg";
			lowerFTBar.text2 = physique.lowerAnaerobicThreshold.ToString("N0") + " W";

			lowerSTBar.text = physique.slowTwitchLower.ToString("N1") + " kg";
			lowerSTBar.text2 = physique.lowerAerobicThreshold.ToString("N0") + " W";

			bodyFatBar.text = physique.massFat.ToString("N1") + " kg";
			bodyFatBar.text2 = ((physique.massFat / physique.massTotal) * 100f).ToString("N1") + "%";

			totalMassBar.text = physique.massTotal.ToString("N1") + " kg";
		}
		else
		{
			intervalTimer -= Time.deltaTime;
		}
	}

	// updates displayed ingestible nutrition information
	void SetIngestibleInfo()
	{
		if (currentIngestibleButton == null)
		{
			consumeButton.interactable = false;
			currentIngestibleID = -1;
			caloriesNIBar.text = "--";
			satietyNIBar.text = "--";
			fatNIBar.text = "--";
			carbsNIBar.text = "--";
			proteinNIBar.text = "--";
			waterNIBar.text = "--";
			ingestibleQuantity.text = "--";
		}
		else
		{
			consumeButton.interactable = true;
			Ingestible ingestible = currentIngestibleButton.GetIngestible();
			currentIngestibleID = currentIngestibleButton.GetInventoryID();
			caloriesNIBar.text = ingestible.calories.ToString("N0");
			satietyNIBar.text = (physique.AdjustSatiety(ingestible.satiety) * 100f).ToString("N0") + "%";
			fatNIBar.text = ingestible.fat.ToString("N1") + " g";
			carbsNIBar.text = ingestible.carbs.ToString("N1") + " g";
			proteinNIBar.text = ingestible.protein.ToString("N1") + " g";
			waterNIBar.text = ingestible.water.ToString("N1") + " g";
			ingestibleQuantity.text = currentIngestibleButton.GetCount().ToString("N0");
		}
	}

	// selects a new ingestible
	public void IngestiblePressed(IngestibleButton button)
	{
		if (currentIngestibleButton != null)
		{
			currentIngestibleButton.SetInteractable(true);
		}
		currentIngestibleButton = button;
		SetIngestibleInfo();
	}

	// attempts to ingest the currently selected food (fails if too full to eat)
	public void ConsumePressed()
	{
		if (inventory.EquipIfAble(currentIngestibleID))
		{
			if (currentIngestibleButton.GetCount() == 1)
			{
				currentIngestibleID = -1;
			}
			RefreshIngestibles();
		}
	}
}
