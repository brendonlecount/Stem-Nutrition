using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// organs trigger an OnDeathChange event when their health is depleted or reset
// usually used to determine if a character has been killed
public class Organ : ConditionComponent {
	public float conditionMax;
	public float regen;

	public GameObject deathPrefab;
	public GameObject deathSound;


	public override string componentName
	{
		get { return organName; }
	}
	[SerializeField] string organName;

	public bool isDead
	{
		get { return condition <= 0f; }
	}

	public delegate void OnDeathChange(Organ organ, bool isDead);
	public event OnDeathChange onDeathChange;

	protected float condition;

	private void Start()
	{
		condition = conditionMax;
	}

	// damages the condition, triggering an onDeathChange event if the organ was destroyed
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

				if (onDeathChange != null)
				{
					onDeathChange(this, true);
				}
			}

			return energy - damage;
		}
	}

	public override float GetConditionFraction()
	{
		return condition / conditionMax;
	}

	public override float GetCellConditionFraction(int cellIndex)
	{
		return GetConditionFraction();
	}

	// heals the organ and triggers an onDeathChange event if the organ was dead
	public void Heal()
	{
		bool wasDead = isDead;
		condition = conditionMax;
		if (wasDead && onDeathChange != null)
		{
			onDeathChange(this, false);
		}
	}
}
