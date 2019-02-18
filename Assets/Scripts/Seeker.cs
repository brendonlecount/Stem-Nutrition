using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// variant on the projectile that seeks a specified target, exploding on impact and generating shrapnel projectiles
public class Seeker : Damager {
	[Tooltip("Maximum muzzle velocity in FPS.")]
	public float fps;
	[Tooltip("Acceleration in FPS/S")]
	public float fpss;
	[Tooltip("Seconds before projectile spontaneously detonates.")]
	public float lifetime;
	[Tooltip("Maximum turn rate, in degrees per second.")]
	public float turnRate;
	[Tooltip("Organ condition component.")]
	public Organ vitals;
	[Tooltip("Origin point for explosion shrapnel.")]
	public GameObject explosionNode;
	[Tooltip("Explosion prefab.")]
	public GameObject explosionPrefab;
	[Tooltip("Explosion audio source.")]
	public GameObject explosionAudioPrefab;
	[Tooltip("Explosion shrapnel prefab (projectile).")]
	public GameObject shrapnelPrefab;
	[Tooltip("Explosion shrapnel count.")]
	public int shrapnelCount;

	float lifeTimer;
	Vector3 velocity;
	float targetSpeed;
	float acceleration;
	float speed;        // speed in m/s
	Transform target;

	// Use this for initialization
	void Start()
	{
		targetSpeed = fps * 0.3048f;
		acceleration = fpss * 0.3048f;
		speed = 0;

		lifeTimer = lifetime;
		velocity = Vector3.zero;
	}

	private void Update()
	{
		// check condition
		if (vitals.isDead)
		{
			Detonate();
			return;
		}

		// check lifetime
		if (lifeTimer <= 0f)
		{
			Detonate();
			return;
		}
		else
		{
			lifeTimer -= Time.deltaTime;
		}

		// rotate towards target
		if (target != null)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position, Vector3.up), turnRate * Time.deltaTime);
		}

		// accelerate towards targetSpeed
		speed = Mathf.Min(targetSpeed, speed + acceleration * Time.deltaTime);
		velocity = Vector3.forward * speed;

		// check for impact
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.rotation * velocity, out hit, velocity.magnitude * Time.deltaTime, Masks.Raycast.hitsProjectiles))
		{
			transform.position = hit.point;
			Detonate();
		}
		else
		{
			transform.position += transform.rotation * velocity * Time.deltaTime;
		}
	}

	// specify the target to seek
	public void SetTarget(Transform target)
	{
		this.target = target;
	}

	// estimate damage from generated shrapnel
	public float GetDamageEstimate()
	{
		Projectile shrapnel = shrapnelPrefab.GetComponent<Projectile>();
		if (shrapnel != null)
		{
			return shrapnel.GetDamageEstimate() * shrapnelCount;
		}
		return 0f;
	}

	// kill the seeker but instantiate an explosion, explosion sound, and shrapnel
	void Detonate()
	{
		GameObject.Instantiate(explosionPrefab, explosionNode.transform.position, Quaternion.identity);
		GameObject.Instantiate(explosionAudioPrefab, explosionNode.transform.position, Quaternion.identity);
		for (int i = 0; i < shrapnelCount; i++)
		{
			GameObject.Instantiate(shrapnelPrefab, explosionNode.transform.position, Random.rotation);
		}
		GameObject.Destroy(gameObject);
	}
}
