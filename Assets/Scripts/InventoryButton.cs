using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// button used by the inventory menu to keep track of inventory items
public class InventoryButton : MonoBehaviour {
	public Button button;
	public Text text;

	InventoryItem item;
	int inventoryID;
	InventoryMenu inventoryMenu;
	int count;
	bool isEquipped;

	public void InitializeButton(InventoryMenu inventoryMenu, InventoryEntry entry, bool isEquipped)
	{
		this.inventoryMenu = inventoryMenu;
		text.text = entry.activatorName;
		this.inventoryID = entry.inventoryID;
		this.item = entry.item.GetComponent<InventoryItem>();
		this.count = entry.count;
		this.isEquipped = isEquipped;
	}

	public void OnClick()
	{
		inventoryMenu.InventoryPressed(this);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	public InventoryItem GetInventoryItem()
	{
		return item;
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
