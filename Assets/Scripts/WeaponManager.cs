using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// tags for holsters
public enum HolsterName { UnarmedRight, UnarmedLeft, ThighRight, ThighLeft, BackRight, BackLeft, ShoulderRight, ShoulderLeft, Chest, Head }
// tags for in-use weapon attachment points
public enum UseSlotType { NotDrawn, HandRight, HandLeft, BothHands }

// Class used to manage weapons and other equipped gear, like headlamps and targeters.
// Weapons get equipped to holsters defined by GearHolster components attached to nodes in the target body.
// Weapons are either drawn (equipped to left and right hand GearUsePoints) or, in the case of things like
// headlamps and shoulder cannons, left in their holsters. Drawn weapons are oriented towards where the character
// is currently looking.
[RequireComponent(typeof(ArmorManager))]
[RequireComponent(typeof(CharacterInventory))]
public class WeaponManager : MonoBehaviour, IMovementControllerReliant {
	[Tooltip("Default fist.")]
	public GameObject defaultFist;

	// mass of holstered and drawn weapons
	public float equippedMass {	get; private set; }

	// is the character in iron sights mode?
	public bool isIronSights { get; private set; }

	// what is the current sight magnification? (1x if not in iron sights or no scope)
	public float magnification
	{
		get
		{
			if (isIronSights)
			{
				return rightHand.drawHolster.GetWeapon().GetScopeMagnification();
			}
			else
			{
				return 1f;
			}
		}
	}

	CharacterInventory inventory;
	ArmorManager armor;
	MovementController controller;

	GearUsePoint leftHand;
	GearUsePoint rightHand;

	// holsters where weapons can be stored
	SortedList<HolsterName, GearHolster> gearHolsters;

	bool weaponsOut;

	// Use this for initialization
	void Start() {
		inventory = GetComponent<CharacterInventory>();
		armor = GetComponent<ArmorManager>();

		// register for arm damage events
		armor.onLeftArmChanged += OnLeftArmChanged;
		armor.onRightArmChanged += OnRightArmChanged;

		// get ahold of left and right hand GearUsePoints
		leftHand = GetComponentInChildren<LeftHandUseSlot>();
		rightHand = GetComponentInChildren<RightHandUseSlot>();

		// find gear holsters
		gearHolsters = new SortedList<HolsterName, GearHolster>();
		GearHolster[] gearHolstersTemp = GetComponentsInChildren<GearHolster>();
		foreach (GearHolster nextHolster in gearHolstersTemp)
		{
			gearHolsters.Add(nextHolster.holsterName, nextHolster);
			switch (nextHolster.GetUseSlot())
			{
				case UseSlotType.BothHands:
					nextHolster.drawnNode = rightHand.useNode;
					nextHolster.ironSightsNode = controller.ironSightsNode.transform;
					break;
				case UseSlotType.HandLeft:
					nextHolster.drawnNode = leftHand.useNode;
					nextHolster.ironSightsNode = controller.ironSightsNode.transform;
					break;
				case UseSlotType.HandRight:
					nextHolster.drawnNode = rightHand.useNode;
					nextHolster.ironSightsNode = controller.ironSightsNode.transform;
					break;
			}
		}
		gearHolsters.TrimExcess();

		weaponsOut = false;

		// equip default fists, if specified
		if (defaultFist != null)
		{
			GetGearHolster(HolsterName.UnarmedRight).EquipIfAble(defaultFist, controller);
			GetGearHolster(HolsterName.UnarmedLeft).EquipIfAble(defaultFist, controller);
			ToggleHolster(HolsterName.UnarmedRight);
			ToggleHolster(HolsterName.UnarmedLeft);
		}

	}

	// callback functions for arm damage - prevent triggering of weapons in damaged hands
	private void OnRightArmChanged(bool isCompromised)
	{
		rightHand.isCompromised = isCompromised;
	}

	private void OnLeftArmChanged(bool isCompromised)
	{
		leftHand.isCompromised = isCompromised;
	}

	// takes a weapon prefab and instantiates it in an appropriate holster, if empty
	public bool EquipWeapon(GameObject weaponPrefab)
	{
		if (weaponPrefab != null)
		{
			foreach (KeyValuePair<HolsterName, GearHolster> holster in gearHolsters)
			{
				if (holster.Value.EquipIfAble(weaponPrefab, controller))
				{
					equippedMass += holster.Value.GetWeapon().mass;
					return true;		// open holster found, done trying to equip
				}
			}
		}
		return false;
	}

	// takes a weapon prefab and instantiates it in the specified holster, if empty and appropriate
	public bool EquipWeapon(GameObject weaponPrefab, HolsterName holsterName)
	{
		if (weaponPrefab != null)
		{
			GearHolster holster;
			if (gearHolsters.TryGetValue(holsterName, out holster))
			{
				if (holster.EquipIfAble(weaponPrefab, controller))
				{
					equippedMass += holster.GetWeapon().mass;
					return true;
				}
			}
		}
		return false;
	}

	// unequips (destroys) the specified weapon in the first matching holster found
	public bool UnequipWeapon(GameObject weaponPrefab)
	{
		InventoryItem item = weaponPrefab.GetComponent<InventoryItem>();
		if (item != null)
		{
			foreach (KeyValuePair<HolsterName, GearHolster> holster in gearHolsters)
			{
				if (holster.Value.GetWeaponID() == item.inventoryID)
				{
					equippedMass -= item.mass;
					holster.Value.DestroyWeapon();
					return true;
				}
			}
		}
		return false;
	}

	// unequips (destroys) the weapon in the specified holster, returning
	// the weapon's inventoryID (or -1 if no weapon or holster was found)
	public int ClearHolster(HolsterName holsterName)
	{
		GearHolster holster;
		if (gearHolsters.TryGetValue(holsterName, out holster))
		{
			int weaponID = holster.GetWeaponID();
			if (weaponID != -1)
			{
				equippedMass -= holster.GetWeapon().mass;
				holster.DestroyWeapon();
				return weaponID;
			}
		}
		return -1;
	}

	// returns the inventory ID of the weapon in the equipped holster,
	// or -1 if holster or weapon not found
	public int GetHolsterContents(HolsterName holsterName)
	{
		GearHolster holster;
		if (gearHolsters.TryGetValue(holsterName, out holster))
		{
			return holster.GetWeaponID();
		}
		return -1;
	}

	// returns the gearHolster object corresponding to the specified holster name
	public GearHolster GetGearHolster(HolsterName name)
	{
		GearHolster holster;
		if (gearHolsters.TryGetValue(name, out holster))
		{
			return holster;
		}
		return null;
	}

	// returns whether the character has the specified holster
	public bool HasHolster(HolsterName holsterName)
	{
		return (gearHolsters.ContainsKey(holsterName));
	}

	// draws left and/or right hand weapons from holster(s) if holsters are
	// specified and have weapons in them
	void DrawWeapons()
	{
		weaponsOut = false;
		if (rightHand.drawHolster != null)
		{
			rightHand.drawHolster.DrawWeapon();
			weaponsOut = true;
		}
		if (leftHand.drawHolster != null)
		{
			leftHand.drawHolster.DrawWeapon();
			weaponsOut = true;
		}
	}

	// holsters left and/or right hand weapons
	void HolsterWeapons()
	{
		if (rightHand.drawHolster != null)
		{
			rightHand.drawHolster.HolsterWeapon();
		}
		if (leftHand.drawHolster != null)
		{
			leftHand.drawHolster.HolsterWeapon();
		}
		weaponsOut = false;
	}

	bool primaryPressed;

	// fires right hand weapon if drawn, or draws weapons (if present)
	public void SetPrimaryAttack(bool trigger)
	{
		if (weaponsOut)
		{
			if (rightHand.drawHolster != null && rightHand.drawHolster.GetWeapon() != null)
			{
				rightHand.drawHolster.GetWeapon().SetTriggered(trigger && !primaryPressed && !rightHand.isCompromised);
			}
			if (!trigger)
			{
				primaryPressed = false;
			}
		}
		else if (trigger)
		{
			DrawWeapons();
			primaryPressed = trigger;
		}
	}

	// fires left hand weapon if drawn and present, or two handed alt behavior, or draws weapons (if present)
	public void SetSecondaryAttack(bool trigger)
	{
		if (weaponsOut)
		{
			if (leftHand.drawHolster != null && leftHand.drawHolster.GetWeapon() != null)
			{
				leftHand.drawHolster.GetWeapon().SetTriggered(trigger && !leftHand.isCompromised);
			}
			else if (leftHand.drawHolster == null && rightHand.drawHolster != null && rightHand.drawHolster.GetWeapon() != null)
			{
				if (trigger && !leftHand.isCompromised)
				{
					isIronSights = true;
					rightHand.drawHolster.GetWeapon().SetAltTriggered(true);
					rightHand.drawHolster.RaiseIronSights();
				}
				else
				{
					isIronSights = false;
					rightHand.drawHolster.GetWeapon().SetAltTriggered(false);
					rightHand.drawHolster.LowerIronSights();
				}
			}
		}
		else if (trigger)
		{
			DrawWeapons();
		}
	}

	// TODO: implement melee button for bayonets and melee power attacks
	public void SetMeleeAttack(bool trigger)
	{
		// it would be cool if your currently equipped gauntlet behavior
		// was applied if your weapons are holstered
	}

	// TODO: implement blocking
	public void SetBlock(bool trigger)
	{
		// possibly also a dodge mechanic?
	}

	// takes an appropriate action depending on the holster specified
	public void ToggleHolster(HolsterName holster)
	{
		// find the holster
		GearHolster newHolster = GetGearHolster(holster);

		// if the holster exists and there's a weapon in the specified holster...
		if (newHolster != null && newHolster.GetWeapon() != null)
		{
			switch (newHolster.GetUseSlot())
			{
				case UseSlotType.NotDrawn:
					// fire or toggle active (determined by weapon)
					newHolster.GetWeapon().SetTriggered(true);
					break;
				case UseSlotType.BothHands:
					// weapons that occupy both hands
					if (newHolster == rightHand.drawHolster)
					{
						if (weaponsOut)
						{
							HolsterWeapons();
						}
						else
						{
							DrawWeapons();
						}
					}
					else
					{
						HolsterWeapons();
						leftHand.drawHolster = null;
						rightHand.drawHolster = newHolster;
						controller.ironSightsNode.Align(newHolster.GetWeapon().GetIronSightsOffset());
						DrawWeapons();
					}
					break;
				case UseSlotType.HandRight:
					// weapons that occupy the right hand
					if (newHolster == rightHand.drawHolster)
					{
						if (weaponsOut)
						{
							HolsterWeapons();
						}
						else
						{
							DrawWeapons();
						}
					}
					else
					{
						HolsterWeapons();
						if (leftHand.drawHolster == null)
						{
							leftHand.drawHolster = GetGearHolster(HolsterName.UnarmedLeft); // ok if null
						}
						rightHand.drawHolster = newHolster;
						DrawWeapons();
					}
					break;
				case UseSlotType.HandLeft:
					// weapons that occupy the left hand
					if (newHolster == leftHand.drawHolster)
					{
						if (weaponsOut)
						{
							HolsterWeapons();
						}
						else
						{
							DrawWeapons();
						}
					}
					else
					{
						HolsterWeapons();
						if (rightHand.drawHolster.GetUseSlot() == UseSlotType.BothHands)
						{
							rightHand.drawHolster = GetGearHolster(HolsterName.UnarmedRight); // ok if null
						}
						leftHand.drawHolster = newHolster;
						DrawWeapons();
					}
					break;
			}
		}
	}

	// Aligns use slots to point at crosshair target. Should be called by InputSource script every frame in LateUpdate.
	public void OrientWeapons(Vector3 targetPosition)
	{
		foreach (KeyValuePair<HolsterName, GearHolster> gearHolster in gearHolsters)
		{
			gearHolster.Value.OrientWeapon(targetPosition);
		}
	}

	// returns a list of weapon slots associated with the specified holster,
	// used to determine weapon compatibility
	public WeaponSlot[] GetHolsterSlots(HolsterName holsterName)
	{
		GearHolster gearHolster = GetGearHolster(holsterName);
		if (gearHolster != null)
		{
			return gearHolster.GetWeaponSlots();
		}
		return null;
	}

	// returns a sorted list of equipped weapons (inventoryID, count)
	// used to prepare a list of equipped weapons for UI display
	public SortedList<int, int> GetHolsteredWeaponIDs()
	{
		SortedList<int, int> weaponIDs = new SortedList<int, int>();
		foreach (KeyValuePair<HolsterName, GearHolster> holster in gearHolsters)
		{
			int weaponID = holster.Value.GetWeaponID();

			if (weaponID != -1 && weaponID != defaultFist.GetComponent<InventoryItem>().inventoryID)
			{
				if (weaponIDs.ContainsKey(weaponID))
				{
					weaponIDs[weaponID] += 1;
				}
				else
				{
					weaponIDs.Add(weaponID, 1);
				}
			}
		}
		return weaponIDs;
	}

	public void SetMovementController(MovementController movementController)
	{
		controller = movementController;
	}
}
