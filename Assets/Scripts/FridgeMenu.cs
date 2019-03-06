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

	private static FridgeMenu instance = null;
	FoodNI currentFood = null;
	Physique physique = null;

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
	}

	public void FoodClicked(FoodButton foodButton)
	{
		currentFood = foodButton.GetFoodNI();
		foodNameText.text = currentFood.foodName;
		caloriesText.text = currentFood.calories.ToString("N0");
		satietyText.text = (100f * physique.AdjustSatiety(currentFood.satiety)).ToString("N0") + "%";
		fatText.text = currentFood.fat.ToString("N1") + " g";
		carbsText.text = currentFood.carbs.ToString("N1") + " g";
		proteinText.text = currentFood.protein.ToString("N1") + " g";
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
}
