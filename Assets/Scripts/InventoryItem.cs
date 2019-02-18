using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for items that can be placed in inventory. extends Stem.Activator by
// causing the item to be moved to inventory when activated. Used for any item
// that can be picked up and placed in inventory.
// TODO: consider making it an interface?
// note: a unique object ID is calculated by an editor script, so they don't need to be specified by hand
// note: prefabs that use classes derived from InventoryItem need to be placed in the appropriate Resources folder,
// so that an objectID can be calculated, and a dictionary of objectIDs and prefabs can be maintained by InventoryLookup
public abstract class InventoryItem : Stem.Activator {
	public virtual InventoryCategory category
	{
		get { return InventoryCategory.Equipment; }
	}
	public int objectID;		// unique ID for this object, used by InventoryLookup to look up the associated prefab
	public float mass;
	public float value;
	[TextArea(3, 10)]
	public string description;

	public int inventoryID
	{
		get
		{
			return objectID + InventoryLookup.GetCategoryPrefix(category) * InventoryLookup.GetCategoryPrefixMult();
		}
	}

	public virtual string type
	{
		get { return "Item"; }
	}

	// causes the InventoryItem to be placed in the user's inventory when activated
	public override void Activate(InputSource user)
	{
		user.controller.inventory.AddItem(this.inventoryID, 1);
		GameObject.Destroy(this.gameObject);
	}

	public override void StopActivate()
	{
	}

	// what happens when the user equips the item? (not used, see CharacterInventory)
	public virtual void Equip(PlayerInput input)
	{
	}
}
