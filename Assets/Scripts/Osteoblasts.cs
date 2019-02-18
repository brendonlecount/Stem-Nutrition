using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// booster that heals all bones to full health
public class Osteoblasts : Booster
{
	public override void ApplyBooster(MovementController controller)
	{
		controller.armor.HealBone();
	}
}
