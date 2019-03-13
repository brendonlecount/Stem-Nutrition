using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FridgeMenu : MonoBehaviour
{
	[SerializeField] GameObject menuPanel = null;
	[SerializeField] Text foodNameText = null;
	[SerializeField] Text caloriesText = null;
	[SerializeField] Text satietyText = null;
	[SerializeField] Text fatText = null;
	[SerializeField] Text carbsText = null;
	[SerializeField] Text proteinText = null;
	[SerializeField] Text waterText = null;

	[SerializeField] MultiBar satietyBar = null;
	[SerializeField] MultiBar carbsBar = null;
	[SerializeField] MultiBar proteinBar = null;
	[SerializeField] MultiBar hydrationBar = null;

	private static FridgeMenu instance = null;
	Physique physique = null;

	FoodNI currentFood = null;
	float currentSatietyFraction = 0f;
	float currentCarbFraction = 0f;
	float currentProteinFraction = 0f;
	float currentHydrationFraction = 0f;

	private void Awake()
	{
		instance = this;
	}

	public static FridgeMenu GetInstance() { return instance; }

	public void ShowMenu()
	{
		GameObject go = GameObject.FindGameObjectWithTag("PlayerController");
		if (go != null)
		{
			PlayerInput player = go.GetComponent<PlayerInput>();
			physique = player.controller.physique;
		}

		foodNameText.text = "--";
		caloriesText.text = "--";
		satietyText.text = "--";
		fatText.text = "--";
		carbsText.text = "--";
		proteinText.text = "--";
		waterText.text = "--";
		menuPanel.SetActive(true);
		MenuManager.menuMode = MenuModes.display;

		physique.onPhysiqueUpdated += PhysiqueUpdated;
	}

	public void FoodClicked(FoodButton foodButton)
	{
		currentFood = foodButton.GetFoodNI();
		foodNameText.text = currentFood.foodName;
		caloriesText.text = currentFood.calories.ToString("N0");
		currentSatietyFraction = physique.AdjustSatiety(currentFood.satiety);
		satietyText.text = (100f * currentSatietyFraction).ToString("N0") + "%";
		fatText.text = currentFood.fat.ToString("N1") + " g";
		currentCarbFraction = physique.GetGlycogenFraction(currentFood.carbs / 1000f);
		carbsText.text = currentFood.carbs.ToString("N1") + " g";
		currentProteinFraction = physique.GetProteinFraction(currentFood.protein / 1000f);
		proteinText.text = currentFood.protein.ToString("N1") + " g";
		currentHydrationFraction = physique.GetHydrationFraction(currentFood.water / 1000f);
		waterText.text = currentFood.water.ToString("N1") + " g";
	}

	public void ConsumeClicked()
	{
		if (currentFood != null)
		{
			physique.Consume(currentFood);
		}
	}

	public void DoneClicked()
	{
		MenuManager.menuMode = MenuModes.none;
		menuPanel.SetActive(false);
	}

	public void PhysiqueUpdated(Physique physique)
	{
		satietyBar.SetBarValue(0, currentSatietyFraction);
		satietyBar.SetBarValue(1, physique.satiety);
		satietyBar.UpdateBarFills();

		carbsBar.SetBarValue(0, currentCarbFraction);
		carbsBar.SetBarValue(1, physique.GetGlycogenFraction(physique.carbDigesting));
		carbsBar.SetBarValue(2, physique.GetGlycogenFraction(physique.glycogenLiver + physique.glycogenUpper + physique.glycogenUpper));
		carbsBar.UpdateBarFills();

		proteinBar.SetBarValue(0, currentProteinFraction);
		proteinBar.SetBarValue(1, physique.GetProteinFraction());
		proteinBar.UpdateBarFills();

		hydrationBar.SetBarValue(0, currentHydrationFraction);
		hydrationBar.SetBarValue(1, physique.GetHydrationFraction(physique.waterDigesting));
		hydrationBar.SetBarValue(2, physique.GetHydrationFraction());
	}
}
