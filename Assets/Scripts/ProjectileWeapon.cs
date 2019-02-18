using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// weapon that fires a projectile, like a bow or gun
public class ProjectileWeapon : Weapon {
	[Tooltip("Node where projectiles are spawned.")]
	public GameObject projectileNode;
	[Tooltip("Minimum time between shots.")]
	public float fireDelay;
	[Tooltip("Does the weapon fire repeatedly when you hold down the trigger?")]
	public bool automatic;
	[Tooltip("Maximum deviation from centerline in degrees.")]
	public float spread;

	float fireTimer;
	bool triggerPressed;

	bool triggered;
	AudioSource fireAudio;

	private void Start()
	{
		fireAudio = GetComponent<AudioSource>();
	}

	// fire the weapon if:
	//	it's fully automatic and fireDelay has elapsed and trigger is pulled
	// or 
	//	it's semiauto, fireDelay has elapsed, and a new trigger press is detected
	void Update()
	{
		if (fireTimer > 0f)
		{
			fireTimer -= Time.deltaTime;
		}
		if (triggered)
		{
			if (fireTimer <= 0f && (automatic || !triggerPressed))
			{
				UseWeapon();
			}
			triggerPressed = true;
		}
		else
		{
			triggerPressed = false;
		}
	}


	// fires the weapon.
	// TODO: get rid of this function in base class
	public override void UseWeapon()
	{
		fireTimer = fireDelay;
		for (int i = 0; i < GetAmmunition().projectileCount * GetAmmoPerShot(); i++)
		{
			Vector3 spreadDirection = Vector3.Slerp(projectileNode.transform.rotation * Vector3.forward, Random.onUnitSphere, spread / 180f);
			Quaternion spreadRotation = Quaternion.LookRotation(spreadDirection);
			GameObject.Instantiate(GetAmmunition().projectile, projectileNode.transform.position, spreadRotation);
		}
		fireAudio.Play();
	}

	public override void SetTriggered(bool triggered)
	{
		this.triggered = triggered;
	}

	public override float GetDamagePerShot()
	{
		return GetAmmunition().projectileCount * GetAmmoPerShot() * GetAmmunition().GetProjectile().GetDamageEstimate();
	}

	public override float GetDamagePerSecond()
	{
		return GetDamagePerShot() / fireDelay;
	}
}
