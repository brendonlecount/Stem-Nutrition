using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// keeps track of character inventory. Extends Inventory by allowing items to be equipped.
[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(ArmorManager))]
[RequireComponent(typeof(Physique))]
public class CharacterInventory : Inventory, IMovementControllerReliant {

	MovementController controller;
	WeaponManager weapons;
	ArmorManager armor;
	Physique physique;

	public float equippedMass
	{
		get { return weapons.equippedMass + armor.equippedMass; }
	}

	private void Awake()
	{
		base.RunStartup();

		weapons = GetComponent<WeaponManager>();
		armor = GetComponent<ArmorManager>();
		physique = GetComponent<Physique>();

		//TODO equip from inventory
	}

	void IMovementControllerReliant.SetMovementController(MovementController movementController)
	{
		controller = movementController;
	}

	// returns a list of equipped items for use by the UI
	public List<InventoryEntry> GetEquippedInventory()
	{
		List<InventoryEntry> equipped = new List<InventoryEntry>();
		SortedList<int, int> weaponIDs = weapons.GetHolsteredWeaponIDs();
		foreach (KeyValuePair<int, int> weaponCount in weaponIDs)
		{
			GameObject go = InventoryLookup.GetInventoryObject(weaponCount.Key);
			if (go != null)
			{
				InventoryItem item = go.GetComponent<InventoryItem>();
				if (item != null)
				{
					Inventory.AddSorted(equipped, new InventoryEntry(item.activatorName, item.inventoryID, go, weaponCount.Value));
				}
			}
		}
		InventoryEntry armorEntry = armor.GetEquippedArmor();
		if (armorEntry.inventoryID != -1)
		{
			Inventory.AddSorted(equipped, armor.GetEquippedArmor());
		}
		return equipped;
	}

	// shows the contents of the specified holster, for use by the UI
	public InventoryEntry GetHolsterContents(HolsterName holster)
	{
		int inventoryID = weapons.GetHolsterContents(holster);
		if (inventoryID != -1)
		{
			GameObject go = InventoryLookup.GetInventoryObject(inventoryID);
			if (go != null)
			{
				InventoryItem ii = go.GetComponent<InventoryItem>();
				if (ii != null)
				{
					return new InventoryEntry(ii.activatorName, inventoryID, go, 1);
				}
			}
		}
		return new InventoryEntry("", -1, null, 0);
	}

	// Generates a list of weapons compatible with the specified holster, for use by the UI
	public List<InventoryEntry> GetHolsterCompatible(HolsterName holsterName)
	{
		List<InventoryEntry> items = new List<InventoryEntry>();
		WeaponSlot[] weaponSlots = weapons.GetHolsterSlots(holsterName);
		if (weaponSlots != null)
		{
			foreach (KeyValuePair<int, InventoryCount> next in inventory)
			{
				if (InventoryLookup.GetInventoryCategory(next.Key) == InventoryCategory.Weapon)
				{
					WeaponSlot slot = next.Value.item.GetComponent<Weapon>().GetSlot();
					bool matchesSlot = false;
					foreach (WeaponSlot nextSlot in weaponSlots)
					{
						if (slot == nextSlot)
						{
							matchesSlot = true;
							break;
						}
					}
					if (matchesSlot)
					{
						AddSorted(items, new InventoryEntry(next.Value));
					}
				}
			}
		}
		return items;
	}

	// does this character have the specified holster?
	public bool HasHolster(HolsterName holsterName)
	{
		return weapons.HasHolster(holsterName);
	}

	// equips an item with the specified inventoryID if the item is present in inventory,
	// is of a type that can be equipped, and all other criteria are met for that specific
	// item type (like, nothing else is already equipped, not oversatiated, etc.)
	public bool EquipIfAble(int inventoryID)
	{
		InventoryCount inventoryCount;
		if (inventory.TryGetValue(inventoryID, out inventoryCount))
		{
			switch (InventoryLookup.GetInventoryCategory(inventoryID))
			{
				case InventoryCategory.Ammunition:
					return false;
				case InventoryCategory.Armor:
					return EquipArmorIfAble(inventoryID, inventoryCount);
				case InventoryCategory.Equipment:
					return false;
				case InventoryCategory.Ingestible:
					return EquipIngestibleIfAble(inventoryID, inventoryCount);
				case InventoryCategory.Weapon:
					return EquipWeaponIfAble(inventoryID, inventoryCount);
				case InventoryCategory.Booster:
					return EquipBoosterIfAble(inventoryID, inventoryCount);
				default:
					return false;
			}
		}
		return false;
	}

	bool EquipArmorIfAble(int inventoryID, InventoryCount inventoryCount)
	{
		InventoryEntry returnedArmor = armor.InstallArmor(new InventoryEntry(inventoryCount));
		int numRemoved = inventoryCount.count - returnedArmor.count;
		RemoveItem(inventoryID, numRemoved);
		return numRemoved > 0;
	}

	bool EquipIngestibleIfAble(int inventoryID, InventoryCount inventoryCount)
	{
		Ingestible ingestible = inventoryCount.item.GetComponent<Ingestible>();
		if (ingestible != null && physique.Consume(ingestible))
		{
			RemoveItem(inventoryID, 1);
			return true;
		}
		return false;
	}

	bool EquipWeaponIfAble(int inventoryID, InventoryCount inventoryCount)
	{
		if (weapons.EquipWeapon(inventoryCount.item))
		{
			RemoveItem(inventoryID, 1);
			return true;
		}
		return false;
	}

	bool EquipBoosterIfAble(int inventoryID, InventoryCount inventoryCount)
	{
		Booster booster = inventoryCount.item.GetComponent<Booster>();
		if (booster != null)
		{
			booster.ApplyBooster(controller);
			RemoveItem(inventoryID, 1);
		}
		return false;
	}

	// TODO: implement armor unequipping
	public void UnequipArmor()
	{

	}

	public bool UnequipIfAble(int inventoryID)
	{
		switch (InventoryLookup.GetInventoryCategory(inventoryID))
		{
			case InventoryCategory.Ammunition:
				return false;
			case InventoryCategory.Armor:
				return UnequipArmorIfAble(inventoryID);
			case InventoryCategory.Equipment:
				return false;
			case InventoryCategory.Ingestible:
				return false;
			case InventoryCategory.Weapon:
				return UnequipWeaponIfAble(inventoryID);
			case InventoryCategory.Booster:
				return false;
			default:
				return false;
		}
	}

	// TODO: implement armor unequipping
	bool UnequipArmorIfAble(int inventoryID)
	{
		return false;
	}

	bool UnequipWeaponIfAble(int inventoryID)
	{
		GameObject go = InventoryLookup.GetInventoryObject(inventoryID);
		if (go != null)
		{
			if (weapons.UnequipWeapon(go))
			{
				AddItem(inventoryID, 1);
				return true;
			}
		}
		return false;
	}

	// removes the weapon from the specified holster, placing it in inventory
	public void ClearHolster(HolsterName holsterName)
	{
		AddItem(weapons.ClearHolster(holsterName), 1);
	}

	// equips a weapon to the specified holster, if possible
	public bool EquipToHolsterIfAble(int inventoryID, HolsterName holsterName)
	{
		InventoryCount inventoryCount;
		if (inventory.TryGetValue(inventoryID, out inventoryCount))
		{
			if (weapons.EquipWeapon(inventoryCount.item, holsterName))
			{
				RemoveItem(inventoryID, 1);
				return true;
			}
		}
		return false;
	}
}
