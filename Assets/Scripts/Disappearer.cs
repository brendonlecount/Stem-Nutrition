using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disappearer : Organ {
	public override float DamageCondition(float energy, float area, BodyPart targetedPart, int cellIndex)
	{
		if (isDead)
		{
			return energy;
		}
		else
		{
			float damage = Mathf.Min(energy, condition);
			condition -= damage;

			if (isDead)
			{
				if (deathPrefab != null)
				{
					GameObject.Instantiate(deathPrefab, transform.position, Quaternion.identity);
				}

				if (deathSound != null)
				{
					GameObject.Instantiate(deathSound, transform.position, Quaternion.identity);
				}
				gameObject.SetActive(false);
			}

			return energy - damage;
		}
	}
}
