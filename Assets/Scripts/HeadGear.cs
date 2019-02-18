using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// targeting monocle that lets the wearer switch between three vision modes.
// meant to equip to the head slot, but could in theory be equipped anywhere
public class HeadGear : Weapon {
	// restore default vision mode if headgear is destroyed (unequipped)
	public override void OnWeaponUnequipped()
	{
		base.OnWeaponUnequipped();
		if (controller != null)
		{
			controller.inputSource.visionMode = CameraVisionMode.Default;
		}
	}

	public override void UseWeapon() { }

	// cycle vision mode when triggered
	// causes visual effects to be applied if the wearer is the player character
	// TODO: effect detection behavior when equipped by NPC
	public override void SetTriggered(bool triggered)
	{
		if (triggered)
		{
			switch (controller.inputSource.visionMode)
			{
				case CameraVisionMode.Default:
					controller.inputSource.visionMode = CameraVisionMode.XRay;
					break;
				case CameraVisionMode.XRay:
					controller.inputSource.visionMode = CameraVisionMode.Amplification;
					break;
				case CameraVisionMode.Amplification:
					controller.inputSource.visionMode = CameraVisionMode.Default;
					break;
			}
		}
		else
		{
			controller.inputSource.visionMode = CameraVisionMode.Default;
		}
	}

	public override float GetDamagePerShot() { return 0f; }

	public override float GetDamagePerSecond() { return 0f; }
}
