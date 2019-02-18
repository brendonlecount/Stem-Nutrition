using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// button used to keep track of gear (weapons)
public class GearButton : MonoBehaviour {
	public Button button;
	public Text text;

	Weapon weapon;
	int inventoryID;
	GearMenu gearMenu;
	int count;
	bool isEquipped;

	public void InitializeButton(GearMenu gearMenu, InventoryEntry entry, bool isEquipped)
	{
		this.gearMenu = gearMenu;
		text.text = entry.activatorName;
		this.inventoryID = entry.inventoryID;
		this.weapon = entry.item.GetComponent<Weapon>();
		this.count = entry.count;
		this.isEquipped = isEquipped;
	}

	public void OnClick()
	{
		gearMenu.WeaponPressed(this);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	public Weapon GetWeapon()
	{
		return weapon;
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
