using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// menu that displays the current inventory
public class InventoryMenu : MonoBehaviour {
	public GameObject inventoryList;
	public GameObject equippedList;
	public GameObject inventoryButton;

	public Bar inventoryMass;
	public Bar equippedMass;
	public Bar carriedMass;

	public Text itemDescription;

	public Bar itemQuantity;
	public Bar itemType;
	public Bar itemMass;
	public Bar itemValue;

	PlayerInput player;
	CharacterInventory inventory;

	int currentInventoryID = -1;
	InventoryButton currentInventoryButton;

	public Button equipButton;
	public Button dropButton;
	public Text equipButtonText;

	private void Awake()
	{
		GameObject go = GameObject.FindGameObjectWithTag("PlayerController");
		if (go != null)
		{
			player = go.GetComponent<PlayerInput>();
			inventory = player.controller.inventory;
		}
	}

	private void OnEnable()
	{
		RefreshInventoryLists();
	}

	// creates lists of buttons representing stored and equipped InventoryItems
	void RefreshInventoryLists()
	{
		// refresh list of stored inventory items
		foreach (Transform child in inventoryList.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		inventoryList.transform.DetachChildren();

		bool currentInventoryButtonSet = false;

		List<InventoryEntry> inventoryEntries = inventory.GetInventory();
		foreach (InventoryEntry entry in inventoryEntries)
		{
			GameObject nextObject = GameObject.Instantiate(inventoryButton, inventoryList.transform);
			InventoryButton nextButton = nextObject.GetComponent<InventoryButton>();
			nextButton.InitializeButton(this, entry, false);
			if (currentInventoryID == -1)
			{
				currentInventoryID = entry.inventoryID;
			}
			if (entry.inventoryID == currentInventoryID)
			{
				currentInventoryButtonSet = true;
				currentInventoryButton = nextButton;
				currentInventoryButton.SetInteractable(false);
			}
		}

		if (!currentInventoryButtonSet)
		{
			currentInventoryButton = null;
		}

		// refresh list of equipped inventory items
		foreach (Transform child in equippedList.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
		equippedList.transform.DetachChildren();

		inventoryEntries = inventory.GetEquippedInventory();
		foreach (InventoryEntry entry in inventoryEntries)
		{
			GameObject nextObject = GameObject.Instantiate(inventoryButton, equippedList.transform);
			InventoryButton nextButton = nextObject.GetComponent<InventoryButton>();
			nextButton.InitializeButton(this, entry, true);
			if (currentInventoryID == -1)
			{
				currentInventoryID = entry.inventoryID;
			}
			if (entry.inventoryID == currentInventoryID && !currentInventoryButtonSet)
			{
				currentInventoryButton = nextButton;
				currentInventoryButton.SetInteractable(false);
			}
		}

		// display stored, equipped, and total inventory mass
		inventoryMass.text = inventory.inventoryMass.ToString("N1") + " kg";
		equippedMass.text = inventory.equippedMass.ToString("N1") + " kg";
		carriedMass.text = (inventory.inventoryMass + inventory.equippedMass).ToString("N1") + " kg";

		// update currently selected inventory item display information
		SetInventoryInfo();
	}

	// called by InventoryButtons when pressed (causes their associated item information to be displayed)
	public void InventoryPressed(InventoryButton button)
	{
		if (currentInventoryButton != null)
		{
			currentInventoryButton.SetInteractable(true);
		}
		currentInventoryButton = button;
		currentInventoryButton.SetInteractable(false);
		SetInventoryInfo();
	}

	// updates displayed inventory item information to reflect the currently selected InventoryButton
	void SetInventoryInfo()
	{
		if (currentInventoryButton == null)
		{
			currentInventoryID = -1;

			equipButtonText.text = "Equip";
			equipButton.interactable = false;
			dropButton.interactable = false;

			itemDescription.text = "";

			itemQuantity.text = "--";
			itemType.text = "--";
			itemMass.text = "--";
			itemValue.text = "--";
		}
		else
		{
			InventoryItem inventoryItem = currentInventoryButton.GetInventoryItem();

			currentInventoryID = currentInventoryButton.GetInventoryID();

			if (currentInventoryButton.GetIsEquipped())
			{
				equipButtonText.text = "Unequip";
			}
			else
			{
				equipButtonText.text = "Equip";
			}
			equipButton.interactable = true;
			dropButton.interactable = true;

			itemDescription.text = inventoryItem.description;

			itemQuantity.text = currentInventoryButton.GetCount().ToString("N0");
			itemType.text = inventoryItem.type;
			itemMass.text = (inventoryItem.mass * currentInventoryButton.GetCount()).ToString("N1") + " kg";
			itemValue.text = inventoryItem.value.ToString("N0");
		}
	}

	// drops the selected item from inventory
	public void DropPressed()
	{
		inventory.DropItem(currentInventoryID, 1);
		if (currentInventoryButton.GetCount() == 1)
		{
			currentInventoryID = -1;
		}
		RefreshInventoryLists();
	}

	// equips/unequips the currently selected inventory item
	// TODO: create separate buttons and functions for equip/unequip and enable/disable
	public void EquipPressed()
	{
		if (currentInventoryButton.GetIsEquipped())
		{
			inventory.UnequipIfAble(currentInventoryID);
		}
		else
		{
			inventory.EquipIfAble(currentInventoryID);
		}
		RefreshInventoryLists();
	}
}
