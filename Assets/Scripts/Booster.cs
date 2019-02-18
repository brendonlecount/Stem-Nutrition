using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for category of InventoryItem that includes things like health kits, power ups, etc.
public abstract class Booster : InventoryItem
{
	public override InventoryCategory category
	{
		get { return InventoryCategory.Booster; }
	}

	public override string type
	{
		get { return "Booster"; }
	}

	// what happens when the booster is applied
	public abstract void ApplyBooster(MovementController controller);
}
