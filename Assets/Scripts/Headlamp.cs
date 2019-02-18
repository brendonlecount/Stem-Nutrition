using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wearable headlamp. meant for the head, but in theory could be equipped anywhere
public class Headlamp : Weapon {
	public Light headLamp;

	bool lightOn = false;
	int visualDetectionModifierID = -1;

	// remove the active visual detection modifier when headlamp is destroyed (unequipped)
	public override void OnWeaponUnequipped()
	{
		base.OnWeaponUnequipped();
		if (controller != null)
		{
			controller.comNode.RemoveDetectionModifier(DetectionMode.ActiveVisual, visualDetectionModifierID);
		}
	}

	public override void UseWeapon() { }

	// toggle light when triggered
	public override void SetTriggered(bool triggered)
	{
		if (triggered)
		{
			if (lightOn)
			{
				HideLight();
			}
			else
			{
				ShowLight();
			}
		}
		else
		{
			HideLight();
		}
	}

	// turns the light on and creates an active visual detection modifier (makes you easier to detect visually even in darkness)
	private void ShowLight()
	{
		lightOn = true;
		headLamp.gameObject.SetActive(true);
		visualDetectionModifierID = controller.comNode.AddDetectionModifier(DetectionMode.ActiveVisual, headLamp.range);
	}

	// turns the light off and removes the active visual detection modifier
	private void HideLight()
	{
		lightOn = false;
		headLamp.gameObject.SetActive(false);
		controller.comNode.RemoveDetectionModifier(DetectionMode.ActiveVisual, visualDetectionModifierID);
		visualDetectionModifierID = -1;
	}

	public override float GetDamagePerShot() { return 0f; }

	public override float GetDamagePerSecond() { return 0f; }

}
