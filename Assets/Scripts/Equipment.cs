using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class for gear that has no real function, like quest items (i.e. can't equip)
public class Equipment : InventoryItem {
	public override InventoryCategory category
	{
		get { return InventoryCategory.Equipment; }
	}

	public override string type
	{
		get { return "Item"; }
	}

}
