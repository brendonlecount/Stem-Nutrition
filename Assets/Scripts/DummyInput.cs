using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple AI for targeting dummy. Just spawns the controller and equips the specified armor.
public class DummyInput : InputSource {
	[SerializeField] InventoryCount startingArmor = null;

	void Awake () {
		// set up controller
		base.StartUp();
	}

	private void Start()
	{
		// add and equip starting armor
		InventoryItem inventoryItem = startingArmor.item.GetComponent<InventoryItem>();
		if (inventoryItem != null)
		{
			controller.inventory.AddItem(inventoryItem.inventoryID, startingArmor.count);
			controller.inventory.EquipIfAble(inventoryItem.inventoryID);
		}
	}

	private void LateUpdate()
	{
		controller.PositionTargets();
	}
}
