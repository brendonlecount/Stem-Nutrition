using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// implements a projectile, like a bullet or an arrow, by performing raycasts forward
// velocity * elapsed time, updating position to the end of that vector each frame,
// applying damage to any ConditionComponents hit along the way. Projectile destroys
// itself after lifetime seconds, when it hits non-condition comonents, or when it
// runs out of kinetic energy after hitting ConditionComponents.
// TODO: implement drag?
public class Projectile : Damager {
	[Tooltip("Damage of projectile in Joules (set via editor tool).")]
	public float damage;
	[Tooltip("Maximum muzzle velocity in FPS.")]
	public float fps;
	[Tooltip("Weight in grains.")]
	public float grain;
	[Tooltip("Diameter (after impact) in inches.")]
	public float diameter;
	[Tooltip("Seconds before projectile spontaneously disappears.")]
	public float lifetime;
	[Tooltip("Should the projectile be effected by gravity?")]
	public bool useGravity;
	[Tooltip("Should the projectile align with velocity? Best for arrows.")]
	public bool align;

	public const float GRAIN2KG = 0.0000648f;
	public const float FPS2MPS = 0.3048f;

	float lifeTimer;
	Vector3 velocity;
	float mass;			// mass in kg
	float speed;		// speed in m/s
	float area;         // area in m^2, used for calculating penetration
	BodyPart targetedPart;

	// convert parameters from English units to metric
	// also initialize starting parameters
	void Start()
	{
		mass = grain * GRAIN2KG;
		speed = fps * FPS2MPS;
		area = Mathf.PI * Mathf.Pow(diameter * 0.5f * 0.0254f, 2f);

		lifeTimer = lifetime;
		velocity = transform.rotation * Vector3.forward * speed;
		targetedPart = BodyPart.None;
	}

	private void Update()
	{
		// destroy if lifetime has expired
		if (lifeTimer <= 0f)
		{
			GameObject.Destroy(gameObject);
		}
		else
		{
			lifeTimer -= Time.deltaTime;
		}

		// apply gravity (accelerate down) if useGravity is true
		if (useGravity)
		{
			velocity += Physics.gravity * Time.deltaTime;
			if (align)
			{
				transform.rotation = Quaternion.LookRotation(velocity);
			}
		}

		// cast ahead along velocity vector over a distance of velocity * elapsed time
		RaycastHit[] hits = Physics.RaycastAll(transform.position, velocity, velocity.magnitude * Time.deltaTime, Masks.Raycast.hitsProjectiles);
		if (hits.Length > 0)
		{
			// if stuff was hit, sort the hits array by distance
			SortHits(hits);
			// calculate current kinetic energy
			float energy = GetEnergy();
			// step through the hits array, dealing damage to ConditionComponents and subtracting from kinetic energy
			for (int i = 0; i < hits.Length && energy > 0f; i++)
			{
				ConditionComponent targetComponent = hits[i].collider.GetComponent<ConditionComponent>();
				if (targetComponent == null)
				{
					energy = 0f;
				}
				else
				{
					if (targetComponent.bodyPart != BodyPart.None)
					{
						targetedPart = targetComponent.bodyPart;
					}
					energy = targetComponent.DamageCondition(energy, area, targetedPart, hits[i].triangleIndex / 3);
				}
			}
			// update the projectile position
			transform.position += velocity * Time.deltaTime;
			// calculate the new speed from the resulting kinetic energy, destroying the projectile if energy is zero
			SetEnergy(energy);
		}
		else
		{
			// nothing was hit, update the projectile position
			transform.position += velocity * Time.deltaTime;
		}
	}

	// adjusts the velocity to match the specified kinetic energy
	protected void SetEnergy(float energy)
	{
		if (energy == 0f)
		{
			GameObject.Destroy(gameObject);
		}
		else
		{
			// ke = m * v^2 / 2
			// v = sqrt(2 * ke / m)
			velocity = velocity.normalized * Mathf.Sqrt(2f * energy / mass);
		}
	}

	// calculate the current kinetic energy
	protected float GetEnergy()
	{
		// ke = m * v^2 / 2
		return 0.5f * velocity.sqrMagnitude * mass;
	}

	// estimate projectile damage based on starting mass and speed (for weapon DPS calcs)
	public float GetDamageEstimate()
	{
		float mass = grain * GRAIN2KG;
		float speed = fps * FPS2MPS;

		return 0.5f * speed * speed * mass;
	}
}
