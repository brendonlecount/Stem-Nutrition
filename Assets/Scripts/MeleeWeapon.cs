using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class used for melee weapons, like the default fist.
// TODO: implement functionality beyond simply playing the attack sound
public class MeleeWeapon : Weapon {
	public float fireDelay;
	public float damageEnergy;
	float fireTimer;
	bool triggered;
	bool triggerPressed;
	AudioSource fireAudio;

	private void Start()
	{
		fireAudio = GetComponent<AudioSource>();
	}

	// use the weapon if triggered and firing delay has expired
	void Update()
	{
		if (fireTimer > 0f)
		{
			fireTimer -= Time.deltaTime;
		}
		if (triggered)
		{
			if (fireTimer <= 0f && !triggerPressed)
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

	// for now, just play the attack sound
	public override void UseWeapon()
	{
		fireTimer = fireDelay;
		fireAudio.Play();
	}

	public override void SetTriggered(bool triggered)
	{
		this.triggered = triggered;
	}

	public override float GetDamagePerShot()
	{
		return damageEnergy;
	}

	public override float GetDamagePerSecond()
	{
		return GetDamagePerShot() / fireDelay;
	}
}
