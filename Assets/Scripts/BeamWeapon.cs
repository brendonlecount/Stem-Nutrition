using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// does the beam fire continuously, or as automatic or semiauto pulses?
public enum BeamType{ Continuous, Automatic, Semiautomatic }

// Energy weapon
public class BeamWeapon : Weapon {
	[Tooltip("Node where projectiles are spawned.")]
	public GameObject projectileNode;
	[Tooltip("Minimum time between shots, if auto or semiauto.")]
	public float fireDelay;
	[Tooltip("Duration of the beam, if auto or semiauto.")]
	public float duration;
	[Tooltip("Is the beam continuous, auto, or semiauto?")]
	public BeamType beamType;
	[Tooltip("Prefab for beam fired by this weapon.")]
	public GameObject beamPrefab;

	float fireTimer;
	bool triggerPressed;

	bool triggered;
	AudioSource fireAudio;
	Beam beam;
	private void Start()
	{
		duration = Mathf.Min(duration, fireDelay);
		fireAudio = GetComponent<AudioSource>();
	}

	// Keeps track of spawned beam, and trigger input
	void Update()
	{
		switch (beamType)
		{
			case BeamType.Continuous:
				if (triggered && beam == null)
				{
					SpawnBeam();
				}
				else if (triggered)
				{
					MaintainBeam();
				}
				else if (!triggered && beam != null)
				{
					KillBeam();
				}
				break;
			case BeamType.Automatic:
				fireTimer += Time.deltaTime;
				if (triggered && beam == null)
				{
					if (fireTimer > fireDelay)
					{
						fireTimer = 0f;
						SpawnBeam();
					}
				}
				else if (beam != null)
				{
					if (fireTimer > duration)
					{
						KillBeam();
					}
					else
					{
						MaintainBeam();
					}
				}
				break;
			case BeamType.Semiautomatic:
				fireTimer += Time.deltaTime;
				if (beam == null)
				{
					if (triggered && !triggerPressed && fireTimer > fireDelay)
					{
						triggerPressed = true;
						fireTimer = 0f;
						SpawnBeam();
					}
					if (fireTimer > fireDelay && !triggered)
					{
						triggerPressed = false;
					}
				}
				else
				{
					if (fireTimer > duration)
					{
						KillBeam();
					}
					else
					{
						MaintainBeam();
					}
				}
				break;
		}
	}

	// create a beam and play fireAudio
	void SpawnBeam()
	{
		GameObject go = GameObject.Instantiate(beamPrefab);
		if (go != null)
		{
			beam = go.GetComponent<Beam>();
			if (beam == null)
			{
				GameObject.Destroy(go);
			}
			else
			{
				beam.SetSourceNode(projectileNode);
			}
			fireAudio.Play();
		}
	}

	// TODO: implement ammunition use
	void MaintainBeam()
	{
		// subtract ammo
	}

	// destroy to beam and stop beam audio
	void KillBeam()
	{
		if (fireAudio.loop)
		{
			fireAudio.Stop();
		}
		beam.Kill();
		beam = null;
	}

	// not used
	public override void UseWeapon()
	{
		
	}

	// tell the weapon whether the trigger is pulled or not
	public override void SetTriggered(bool triggered)
	{
		this.triggered = triggered;
	}

	// calculate the damage per shot
	public override float GetDamagePerShot()
	{
		Beam beam = beamPrefab.GetComponent<Beam>();
		if (beam == null)
		{
			return 0f;
		}
		else
		{
			if (beamType == BeamType.Continuous)
			{
				return beam.energyPerSecond;
			}
			else
			{
				return beam.energyPerSecond * duration;
			}
		}
	}

	// calculate the average damage per second
	public override float GetDamagePerSecond()
	{
		if (beamType == BeamType.Continuous)
		{
			return GetDamagePerShot();
		}
		else
		{
			return GetDamagePerShot() / fireDelay;
		}
	}
}
