using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// menu for equipping armor and displaying health (armor, skeleton, and organ)
// TODO: switch from bars to a cool graphic showing the player's body, a layer for each category
public class HealthMenu : MonoBehaviour {
	// common components
	public Button armorPaneButton;
	public Button skeletonPaneButton;
	public Button vitalsPaneButton;

	public GameObject componentList;

	public GameObject componentBarPrefab;

	// armor pane components
	public GameObject armorPane;

	public GameObject armorList;
	public Button equipArmorButton;
	public Button redistributeArmorButton;
	public ArmorButton equippedArmorButton;

	public Text armorDescription;
	public Bar armorQuantityBar;
	public Bar armorMassBar;
	public Bar armorRatingBar;
	public Bar armorHPBar;

	public GameObject armorButtonPrefab;

	// skeleton pane components
	// TODO: implement skeleton pane, with healing options and skeleton type
	public GameObject skeletonPane;

	// vitals pane components
	// TODO: implement vitals pane, with healing options and organ information
	public GameObject vitalsPane;



	PlayerInput player;
	CharacterInventory inventory;
	ArmorManager conditionManager;

	int currentArmorInventoryID = -1;
	ArmorButton currentArmorButton;

	// note: this doesn't get called until the menu is enabled, by which point the
	// PlayerController has been instantiated. not so for the HUDController.
	// TODO: consider making the player controller a singleton?
	private void Awake()
	{
		GameObject go = GameObject.FindGameObjectWithTag("PlayerController");
		if (go != null)
		{
			player = go.GetComponent<PlayerInput>();
			inventory = player.controller.inventory;
			conditionManager = player.controller.armor;
		}
	}

	// always start out on the armor pane
	private void OnEnable()
	{
		OnClickArmorButton();
	}

	// switch to teh armor pane
	public void OnClickArmorButton()
	{
		armorPaneButton.interactable = false;
		skeletonPaneButton.interactable = true;
		vitalsPaneButton.interactable = true;

		armorPane.SetActive(true);
		skeletonPane.SetActive(false);
		vitalsPane.SetActive(false);

		RefreshArmor();
	}

	// swithc to the skeleton pane
	public void OnClickSkeletonButton()
	{
		armorPaneButton.interactable = true;
		skeletonPaneButton.interactable = false;
		vitalsPaneButton.interactable = true;

		armorPane.SetActive(false);
		skeletonPane.SetActive(true);
		vitalsPane.SetActive(false);

		RefreshSkeleton();
	}

	// switch to the organs pane
	public void OnClickVitalsButton()
	{
		armorPaneButton.interactable = true;
		skeletonPaneButton.interactable = true;
		vitalsPaneButton.interactable = false;

		armorPane.SetActive(false);
		skeletonPane.SetActive(false);
		vitalsPane.SetActive(true);

		RefreshVitals();
	}

	// populate the armor pane
	void RefreshArmor()
	{
		// create and display health bars for all of the armor components
		PopulateComponentsList(conditionManager.GetComponentCategory(ComponentCategory.Armor));

		// display the currently equipped armor
		equippedArmorButton.InitializeButton(this, conditionManager.GetEquippedArmor(), true);
		if (equippedArmorButton.GetInventoryID() == -1)
		{
			equippedArmorButton.gameObject.SetActive(false);
			currentArmorInventoryID = -1;
			currentArmorButton = null;
		}
		else
		{
			equippedArmorButton.gameObject.SetActive(true);
			currentArmorInventoryID = equippedArmorButton.GetInventoryID();
			currentArmorButton = equippedArmorButton;
		}

		// refresh armor list
		Button[] oldButtons = armorList.GetComponentsInChildren<Button>();
		foreach (Button button in oldButtons)
		{
			GameObject.Destroy(button.gameObject);
		}

		List<InventoryEntry> armorEntries = inventory.GetCategory(InventoryCategory.Armor);
		foreach (InventoryEntry entry in armorEntries)
		{
			GameObject nextObject = GameObject.Instantiate(armorButtonPrefab, armorList.transform);
			ArmorButton nextButton = nextObject.GetComponent<ArmorButton>();
			nextButton.InitializeButton(this, entry, false);
			if (currentArmorInventoryID == -1)
			{
				currentArmorInventoryID = entry.inventoryID;
			}
			if (entry.inventoryID == currentArmorInventoryID)
			{
				currentArmorButton = nextButton;
				currentArmorButton.SetInteractable(false);
			}
		}

		// refresh currently displayed armor information
		RefreshArmorInformation();
	}

	// updates display information for currently selected armor, if any
	void RefreshArmorInformation()
	{
		if (currentArmorInventoryID == -1)
		{
			armorDescription.text = "";
			armorQuantityBar.text = "--";
			armorMassBar.text = "--";
			armorRatingBar.text = "--";
			armorHPBar.text = "--";

			equipArmorButton.gameObject.SetActive(false);
			redistributeArmorButton.gameObject.SetActive(false);
		}
		else
		{
			Armor armor = currentArmorButton.GetArmor();
			float ccs = Mathf.Min(conditionManager.armorMaxVolume / Armor.UNIT_VOLUME, currentArmorButton.GetCount());
			armorDescription.text = armor.description;
			armorQuantityBar.text = ccs.ToString("N0") + " cc";
			armorMassBar.text = (armor.mass * ccs).ToString("N0") + " kg";
			armorRatingBar.text = currentArmorButton.GetArmor().rating.ToString("N0") + "J/cc";
			armorHPBar.text = (ccs * armor.rating).ToString("N0") + " J";

			if (currentArmorButton.GetIsEquipped())
			{
				equipArmorButton.gameObject.SetActive(false);
				redistributeArmorButton.gameObject.SetActive(true);
			}
			else
			{
				equipArmorButton.gameObject.SetActive(true);
				redistributeArmorButton.gameObject.SetActive(false);
			}
		}
	}

	void RefreshSkeleton()
	{
		// populate list of skeleton healths
		PopulateComponentsList(conditionManager.GetComponentCategory(ComponentCategory.Bone));
	}

	void RefreshVitals()
	{
		// populate list of organ healths
		PopulateComponentsList(conditionManager.GetComponentCategory(ComponentCategory.Organ));
	}

	// populates a list of health bars from a list of ConditionComponentName structs
	void PopulateComponentsList(List<ConditionComponentName> components)
	{
		// clear the old list
		Bar[] oldBars = componentList.GetComponentsInChildren<Bar>();
		foreach (Bar bar in oldBars)
		{
			GameObject.Destroy(bar.gameObject);
		}

		// populate from the current list
		foreach(ConditionComponentName component in components)
		{
			GameObject nextObject = GameObject.Instantiate(componentBarPrefab, componentList.transform);
			Bar nextBar = nextObject.GetComponent<Bar>();
			nextBar.InitializeBar();
			nextBar.label = component.componentName;
			nextBar.barValue = component.conditionFraction;
		}
	}

	// TODO: hmmm. needs implementing?
	public void ArmorPressed(ArmorButton button)
	{

	}

	// equips the currently selected armor
	public void EquipPressed()
	{
		if (inventory.EquipIfAble(currentArmorButton.GetInventoryID()))
		{
			RefreshArmor();
		}
	}

	// TODO: implement armor redistribution and unequipping
	public void RedistributePressed()
	{

	}
}
