using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// implements a holster that weapons can be equipped to and drawn from
// managed by WeaponManager.
// attach to node where the weapon will be displayed when holstered
// TODO: implement a holster transform node so that different weapon orientations can be handled (like for bows)
public class GearHolster : MonoBehaviour {
	public HolsterName holsterName;							// tag used to look up holster
	[SerializeField] public UseSlotType useSlot;			// use slot type associated with holster
	[SerializeField] WeaponSlot[] weaponSlots = null;       // weapon slots associated with holster

	// where the weapon gets attached when not drawn
	public Transform holsterNode
	{
		get { return this.transform; }
	}

	public Transform drawnNode { get; set; }			// where the weapon gets attached when drawn
	public Transform ironSightsNode { get; set; }		// where the weapon gets attached when in iron sights

	Weapon weapon;
	bool isDrawn;           // is the weapon drawn?
	bool isIronSights;

	// returns weapon currently in holster, if any
	public Weapon GetWeapon()
	{
		return weapon;
	}

	public int GetWeaponID()
	{
		if (weapon == null)
		{
			return -1;
		}
		else
		{
			return weapon.inventoryID;
		}
	}

	// true if this holster is for weapons of that slot type
	public bool EquipsSlot(WeaponSlot slot)
	{
		foreach (WeaponSlot nextSlot in weaponSlots)
		{
			if (slot == nextSlot)
			{
				return true;
			}
		}
		return false;
	}

	public bool UsesSlot(UseSlotType slot)
	{
		return useSlot == slot;
	}

	public UseSlotType GetUseSlot()
	{
		return useSlot;
	}

	// instantiates "weapon" in the holster, if the holster is empty
	// also disables collisions
	public bool EquipIfAble(GameObject weaponPrefab, MovementController controller)
	{
		Weapon equippedWeapon = weaponPrefab.GetComponent<Weapon>();
		if (this.weapon == null && equippedWeapon != null && EquipsSlot(equippedWeapon.GetSlot()))
		{
			GameObject go = GameObject.Instantiate(weaponPrefab, holsterNode.position, holsterNode.rotation, holsterNode);
			this.weapon = go.GetComponent<Weapon>();
			if (this.weapon == null)
			{
				GameObject.Destroy(go);
				return false;
			}
			else
			{
				this.weapon.SetMovementController(controller);
				Rigidbody rb = go.GetComponent<Rigidbody>();
				if (rb != null)
				{
					rb.isKinematic = true;
					rb.detectCollisions = false;
				}
				go.transform.localScale = new Vector3(1f / transform.lossyScale.x, 1f / transform.lossyScale.y, 1f / transform.lossyScale.z);
				return true;
			}
		}
		return false;
	}

	// draws the weapon (attaches it to its useSlot node)
	public void DrawWeapon()
	{
		if (weapon != null && drawnNode != null && !isDrawn)
		{
			isDrawn = true;
			isIronSights = false;
			weapon.transform.parent = drawnNode;
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localRotation = Quaternion.identity;
		}
	}

	// raises teh weapon to the iron sights node
	public void RaiseIronSights()
	{
		if (weapon != null && ironSightsNode != null && isDrawn && !isIronSights)
		{
			isIronSights = true;
			weapon.transform.parent = ironSightsNode;
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localRotation = Quaternion.identity;
		}
	}

	// lowers the weapon from iron sights node
	public void LowerIronSights()
	{
		if (weapon != null && isDrawn && isIronSights)
		{
			isIronSights = false;
			weapon.transform.parent = drawnNode;
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localRotation = Quaternion.identity;
		}
	}

	// holsters the weapon (attaches it to its holster node)
	public void HolsterWeapon()
	{
		if (weapon != null && isDrawn)
		{
			weapon.transform.parent = holsterNode;
			weapon.transform.localPosition = Vector3.zero;
			weapon.transform.localRotation = Quaternion.identity;
			this.isDrawn = false;
		}
	}

	// is the weapon currently drawn?
	public bool GetIsDrawn()
	{
		return isDrawn || useSlot == UseSlotType.NotDrawn;
	}

	// drops the weapon from the hand or holster into the game world
	public void DropWeapon()
	{
		if (weapon != null)
		{
			weapon.OnWeaponUnequipped();
			weapon.transform.parent = null;
			Rigidbody rb = weapon.GetComponent<Rigidbody>();
			if (rb != null)
			{
				rb.isKinematic = false;
				rb.detectCollisions = true;
			}
			weapon = null;
			isDrawn = false;
		}
	}

	// destroys weapon
	public void DestroyWeapon()
	{
		if (weapon != null)
		{
			weapon.OnWeaponUnequipped();
			GameObject.Destroy(weapon.gameObject);
			weapon = null;
			isDrawn = false;
		}
	}

	// returns weapon to inventory
	public void StoreWeapon()
	{
		if (weapon != null)
		{
			weapon.OnWeaponUnequipped();
			GameObject.Destroy(weapon.gameObject);
			weapon = null;
			isDrawn = false;
		}
	}

	// orients the weapon towards the current target if the weapon is drawn
	// also orients the scope camera if the weapon has a scope and is in iron sights mode
	public void OrientWeapon(Vector3 targetPosition)
	{
		if (weapon != null && GetIsDrawn() && weapon.GetAlignWhenDrawn())
		{
			weapon.transform.rotation = Quaternion.LookRotation(targetPosition - weapon.transform.position);
			if (isIronSights && weapon.GetScopeCamera() != null)
			{
				Transform scopeTransform = weapon.GetScopeCamera().gameObject.transform;
				scopeTransform.rotation = Quaternion.LookRotation(targetPosition - scopeTransform.position);
			}
		}
	}

	public WeaponSlot[] GetWeaponSlots()
	{
		return weaponSlots;
	}
}
