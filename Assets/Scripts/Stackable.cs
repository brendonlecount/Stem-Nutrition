using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for InventoryItems that are stacked in the gameworld, like armor nanites and ammunition
// TODO: make it an interface?
public abstract class Stackable : InventoryItem {
	public int stackCount;

	public override void Activate(InputSource user)
	{
		user.controller.inventory.AddItem(this.inventoryID, stackCount);
		GameObject.Destroy(this.gameObject);
	}
}
