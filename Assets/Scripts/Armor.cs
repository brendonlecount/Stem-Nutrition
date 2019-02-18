using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Armor that can be equipped. Stacks in inventory and when dropped.
public class Armor : Stackable {
	[Tooltip("Armor rating in joules per cubic centimeter.")]
	public float rating;
	public float regen;		// regen rate of armor (not used)
	public float density;	// used to calculate armor weight
	public bool dynamic;	// does the armor fill in holes as they are created?

	public const float UNIT_VOLUME = 0.000001f;		// 1cc

	// converts armor rating from per cc to per cubic meter
	public float ratingM3
	{
		get { return rating * 1000000f; }
	}

	public override InventoryCategory category
	{
		get { return InventoryCategory.Armor; }
	}

	public override string type
	{
		get { return "Armor"; }
	}
}
