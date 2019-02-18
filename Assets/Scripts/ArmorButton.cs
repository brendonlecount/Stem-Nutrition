using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Button that lists an armor in the health menu
// Contains information pertaining to that armor for use by HealthMenu.cs
public class ArmorButton : MonoBehaviour {
	public Button button;
	public Text text;

	Armor armor;
	int inventoryID;
	HealthMenu healthMenu;
	int count;
	bool isEquipped;

	public void InitializeButton(HealthMenu healthMenu, InventoryEntry entry, bool isEquipped)
	{
		this.healthMenu = healthMenu;
		text.text = entry.activatorName;
		this.inventoryID = entry.inventoryID;
		if (entry.item != null)
		{
			this.armor = entry.item.GetComponent<Armor>();
		}
		this.count = entry.count;
		this.isEquipped = isEquipped;
	}

	public void OnClick()
	{
		healthMenu.ArmorPressed(this);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	public Armor GetArmor()
	{
		return armor;
	}

	public int GetInventoryID()
	{
		return inventoryID;
	}

	public int GetCount()
	{
		return count;
	}

	public bool GetIsEquipped()
	{
		return isEquipped;
	}
}
