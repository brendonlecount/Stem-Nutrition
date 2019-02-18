using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// weapon slot associated with weapon
public enum WeaponSlot { OneHanded, TwoHandedHip, TwoHandedShoulder, Unarmed, Melee, Shoulder, Grenade, Head }

// base class for weapons, things that equip to weapon slots managed by the WeaponManager
// TODO: get rid of UseWeapon function here, or consistently implement it in derived classes
public abstract class Weapon : InventoryItem {
	[Tooltip("Weapon's equip slot.")]
	[SerializeField] WeaponSlot slot = WeaponSlot.OneHanded;
	[Tooltip("Weapon's ammunition.")]
	[SerializeField] GameObject ammunition = null;
	[Tooltip("Weapon's clip size.")]
	[SerializeField] int clipSize = 10;
	[Tooltip("Weapon's ammo consumed per shot. Also determines projectile count.")]
	[SerializeField] int ammoPerShot = 1;
	[Tooltip("Point weapon towards crosshair target when drawn?")]
	[SerializeField] bool alignWhenDrawn = true;
	[Tooltip("Node for camera alignment in iron sights mode.")]
	[SerializeField] Transform ironSightsOffset = null;
	[Tooltip("Scope camera, if any (determines alt-fire behavior.)")]
	[SerializeField] Camera scopeCamera = null;
	[Tooltip("Scope targeting camera, if any.")]
	[SerializeField] Camera scopeTargetCamera = null;
	[Tooltip("Radar light.")]
	[SerializeField] Light radarLight = null;

	protected MovementController controller;
	bool isScoped = false;
	CameraVisionMode visionMode;

	public WeaponSlot GetSlot()
	{
		return slot;
	}

	public Camera GetScopeCamera()
	{
		return scopeCamera;
	}

	public int GetAmmoPerShot()
	{
		return ammoPerShot;
	}

	public bool GetAlignWhenDrawn()
	{
		return alignWhenDrawn;
	}

	public Transform GetIronSightsOffset()
	{
		return ironSightsOffset;
	}

	public Ammunition GetAmmunition()
	{
		return ammunition.GetComponent<Ammunition>();
	}

	public float GetScopeMagnification()
	{
		if (scopeCamera == null)
		{
			return 1f;
		}
		else
		{
			return 60f / scopeCamera.fieldOfView;
		}
	}

	public string GetAmmunitionName()
	{
		if (ammunition == null)
		{
			return "";
		}
		else
		{
			return GetAmmunition().activatorName;
		}
	}

	public abstract void UseWeapon();
	public abstract void SetTriggered(bool triggered);
	public abstract float GetDamagePerShot();
	public abstract float GetDamagePerSecond();

	// applies scope behavior if a scope is present
	// (raising of iron sights is handled by WeaponManager and GearHolster)
	public void SetAltTriggered(bool triggered)
	{
		if (controller.isPlayer)
		{
			if (triggered && !isScoped)
			{
				isScoped = true;
				if (scopeCamera != null)
				{
					scopeCamera.gameObject.SetActive(true);
					CameraController.ApplyVisionMode(visionMode, scopeCamera, scopeTargetCamera, radarLight);
				}
			}
			else if (!triggered && isScoped)
			{
				isScoped = false;
				if (scopeCamera != null)
				{
					scopeCamera.gameObject.SetActive(false);
				}
			}
		}
	}

	public override InventoryCategory category
	{
		get { return InventoryCategory.Weapon; }
	}

	public override string type
	{
		get { return "Weapon"; }
	}

	public void SetMovementController(MovementController controller)
	{
		this.controller = controller;
		controller.inputSource.onVisionModeChanged += OnVisionModeChange;
	}

	public void OnVisionModeChange(CameraVisionMode visionMode)
	{
		this.visionMode = visionMode;
		if (controller.isPlayer && isScoped)
		{
			CameraController.ApplyVisionMode(visionMode, scopeCamera, scopeTargetCamera, radarLight);
		}
	}

	public virtual void OnWeaponUnequipped()
	{
		controller.inputSource.onVisionModeChanged -= OnVisionModeChange;
	}
}
