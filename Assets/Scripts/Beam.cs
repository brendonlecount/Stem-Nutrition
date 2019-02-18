using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawn to create an energy beam for an energy weapon. Causes energyPerSecond damage per second.
public class Beam : Damager {
	[Tooltip("Energy per second, in Watts.")]
	public float energyPerSecond;
	[Tooltip("Beam diameter, in inches.")]
	public float diameter;
	[Tooltip("Maximum beam length, in meters.")]
	public float maxLength;


	GameObject sourceNode;
	float area;

	// Use this for initialization
	void Start () {
		area = Mathf.PI * Mathf.Pow(diameter * 0.5f * 0.0254f, 2f);
	}

	// Draw the beam and apply damage
	void LateUpdate()
	{
		if (sourceNode != null)
		{
			// get a list of things the beam might be hitting
			RaycastHit[] hits = Physics.RaycastAll(sourceNode.transform.position, sourceNode.transform.rotation * Vector3.forward, maxLength, Masks.Raycast.hitsBeams);
			// sort that list by ascending distance from weapon
			SortHits(hits);
			float energy = energyPerSecond * Time.deltaTime;
			BodyPart targetedPart = BodyPart.None;
			int i = 0;
			// step through the list of potential hits, applying damage and continuing through if there is energy left over
			while (i < hits.Length && energy > 0f)
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
					if (energy > 0f)
					{
						i++;
					}
				}
			}

			// if there is energy left over, beam extends to maxLength distance
			if (energy > 0f)
			{
				transform.position = sourceNode.transform.position + sourceNode.transform.rotation * Vector3.forward * maxLength / 2f;
				transform.localScale = new Vector3(1f, 1f, maxLength / 2f);
			}
			else
			{
				// otherwise, beam extends to final collision
				transform.position = (sourceNode.transform.position + hits[i].point) / 2f;
				transform.localScale = new Vector3(1f, 1f, hits[i].distance / 2f);
			}

			transform.rotation = sourceNode.transform.rotation;
		}
	}

	// sets the source of the beam (projectile node of weapon)
	public void SetSourceNode(GameObject sourceNode)
	{
		this.sourceNode = sourceNode;
	}

	// tells the beam to stop beaming
	public void Kill()
	{
		GameObject.Destroy(this.gameObject);
	}
}
