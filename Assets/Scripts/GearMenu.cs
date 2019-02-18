using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// menu for displaying gear (weapons)
public class GearMenu : MonoBehaviour {
	// buttons for selecting holsters
	public Button HolsterButtonHead;
	public Button HolsterButtonLeftShoulder;
	public Button HolsterButtonRightShoulder;
	public Button HolsterButtonChest;
	public Button HolsterButtonLeftBack;
	public Button HolsterButtonRightBack;
	public Button HolsterButtonLeftThigh;
	public Button HolsterButtonRightThigh;
	public Button HolsterButtonLeftGauntlet;
	public Button HolsterButtonRightGauntlet;

	// UI elements for reporting specified weapon
	public GearButton equippedGearButton;
	public GameObject gearList;
	public Text gearDescription;
	public Bar gearQuantity;
	public Bar gearMass;
	public Bar gearAmmo;
	public Bar gearDamage;
	public Bar gearDPS;

	public Button equipButton;
	public Button unequipButton;

	// button to use for weapons
	public GameObject gearButtonPrefab;

	PlayerInput player;
	CharacterInventory inventory;

	HolsterName currentHolster;

	int currentInventoryID = -1;
	GearButton currentGearButton;

	private void Awake()
	{
		GameObject go = GameObject.FindGameObjectWithTag("PlayerController");
		if (go != null)
		{
			player = go.GetComponent<PlayerInput>();
			inventory = player.controller.inventory;
		}
	}

	void OnEnable () {
		HolsterPressed(currentHolster);
	}

	// used for hoster button presses
	public void HolsterPressedHead() { HolsterPressed(HolsterName.Head); }
	public void HolsterPressedLeftShoulder() { HolsterPressed(HolsterName.ShoulderLeft); }
	public void HolsterPressedRightShoulder() { HolsterPressed(HolsterName.ShoulderRight); }
	public void HolsterPressedChest() { HolsterPressed(HolsterName.Chest); }
	public void HolsterPressedLeftBack() { HolsterPressed(HolsterName.BackLeft); }
	public void HolsterPressedRightBack() { HolsterPressed(HolsterName.BackRight); }
	public void HolsterPressedLeftThigh() { HolsterPressed(HolsterName.ThighLeft); }
	public void HolsterPressedRightThigh() { HolsterPressed(HolsterName.ThighRight); }
	public void HolsterPressedLeftGauntlet() { HolsterPressed(HolsterName.UnarmedLeft); }
	public void HolsterPressedRightGauntlet() { HolsterPressed(HolsterName.UnarmedRight); }

	// sets the current weapon and display information
	public void WeaponPressed(GearButton button)
	{
		if (currentGearButton != null)
		{
			currentGearButton.SetInteractable(true);
		}

		currentGearButton = button;
		currentInventoryID = currentGearButton.GetInventoryID();
		currentGearButton.SetInteractable(false);

		SetGearInfo();
	}

	// equips the currently selected weapon
	public void EquipPressed()
	{
		inventory.ClearHolster(currentHolster);
		inventory.EquipToHolsterIfAble(currentGearButton.GetInventoryID(), currentHolster);
		PopulateHolsterPane();
	}

	// unequips the currently selected weapon
	public void UnequipPressed()
	{
		inventory.ClearHolster(currentHolster);
		PopulateHolsterPane();
	}

	// sets up the specified holster for display
	void HolsterPressed(HolsterName holsterName)
	{
		currentHolster = holsterName;
		currentInventoryID = -1;

		HolsterButtonHead.interactable = holsterName != HolsterName.Head;
		HolsterButtonHead.gameObject.SetActive(inventory.HasHolster(HolsterName.Head));

		HolsterButtonLeftShoulder.interactable = holsterName != HolsterName.ShoulderLeft;
		HolsterButtonLeftShoulder.gameObject.SetActive(inventory.HasHolster(HolsterName.ShoulderLeft));

		HolsterButtonRightShoulder.interactable = holsterName != HolsterName.ShoulderRight;
		HolsterButtonRightShoulder.gameObject.SetActive(inventory.HasHolster(HolsterName.ShoulderRight));

		HolsterButtonChest.interactable = holsterName != HolsterName.Chest;
		HolsterButtonChest.gameObject.SetActive(inventory.HasHolster(HolsterName.Chest));

		HolsterButtonLeftBack.interactable = holsterName != HolsterName.BackLeft;
		HolsterButtonLeftBack.gameObject.SetActive(inventory.HasHolster(HolsterName.BackLeft));

		HolsterButtonRightBack.interactable = holsterName != HolsterName.BackRight;
		HolsterButtonRightBack.gameObject.SetActive(inventory.HasHolster(HolsterName.BackRight));

		HolsterButtonLeftThigh.interactable = holsterName != HolsterName.ThighLeft;
		HolsterButtonLeftThigh.gameObject.SetActive(inventory.HasHolster(HolsterName.ThighLeft));

		HolsterButtonRightThigh.interactable = holsterName != HolsterName.ThighRight;
		HolsterButtonRightThigh.gameObject.SetActive(inventory.HasHolster(HolsterName.ThighRight));

		HolsterButtonLeftGauntlet.interactable = holsterName != HolsterName.UnarmedLeft;
		HolsterButtonLeftGauntlet.gameObject.SetActive(inventory.HasHolster(HolsterName.UnarmedLeft));

		HolsterButtonRightGauntlet.interactable = holsterName != HolsterName.UnarmedRight;
		HolsterButtonRightGauntlet.gameObject.SetActive(inventory.HasHolster(HolsterName.UnarmedRight));

		PopulateHolsterPane();
	}

	// sets up the holster pane to display the specified holster
	void PopulateHolsterPane()
	{
		// set up the equipped gear button
		InventoryEntry equippedGear = inventory.GetHolsterContents(currentHolster);
		if (equippedGear.inventoryID == -1)
		{
			equippedGearButton.gameObject.SetActive(false);
		}
		else
		{
			equippedGearButton.gameObject.SetActive(true);
			equippedGearButton.InitializeButton(this, equippedGear, true);
			if (currentInventoryID == equippedGearButton.GetInventoryID() || currentInventoryID == -1)
			{
				currentGearButton = equippedGearButton;
				currentGearButton.SetInteractable(false);
			}
			else
			{
				equippedGearButton.SetInteractable(true);
			}
		}

		// clear the old list of equippable weapons
		foreach (Transform child in gearList.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		gearList.transform.DetachChildren();

		// populate the current list of equippable weapons
		List<InventoryEntry> compatibleWeapons = inventory.GetHolsterCompatible(currentHolster);
		foreach (InventoryEntry entry in compatibleWeapons)
		{
			GameObject nextObject = GameObject.Instantiate(gearButtonPrefab, gearList.transform);
			GearButton nextButton = nextObject.GetComponent<GearButton>();
			nextButton.InitializeButton(this, entry, false);
			if (currentInventoryID == -1)
			{
				currentInventoryID = entry.inventoryID;
			}
			if (entry.inventoryID == currentInventoryID)
			{
				currentGearButton = nextButton;
				currentGearButton.SetInteractable(false);
			}
		}

		// update the gear info for the currently selected weapon
		SetGearInfo();
	}

	// updates the displayed weapon information, if any
	void SetGearInfo()
	{
		if (currentGearButton == null)
		{
			currentInventoryID = -1;

			equipButton.gameObject.SetActive(false);
			unequipButton.gameObject.SetActive(false);

			gearDescription.text = "";
			gearQuantity.text = "--";
			gearMass.text = "--";
			gearAmmo.text = "--";
			gearDamage.text = "--";
			gearDPS.text = "--";
		}
		else
		{
			equipButton.gameObject.SetActive(!currentGearButton.GetIsEquipped());
			unequipButton.gameObject.SetActive(currentGearButton.GetIsEquipped());

			gearDescription.text = currentGearButton.GetWeapon().description;
			gearQuantity.text = currentGearButton.GetCount().ToString("N0");
			gearMass.text = currentGearButton.GetWeapon().mass.ToString("N1") + " kg";
			gearAmmo.text = currentGearButton.GetWeapon().GetAmmunitionName();
			gearDamage.text = currentGearButton.GetWeapon().GetDamagePerShot().ToString("N0") + " J";
			gearDPS.text = currentGearButton.GetWeapon().GetDamagePerSecond().ToString("N0") + " J";
		}
	}
}
