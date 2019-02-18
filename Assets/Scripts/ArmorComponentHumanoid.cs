using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Armor component variant for humanoids (has an associated bodypart, stats derive from equipped armor)
public class ArmorComponentHumanoid : ArmorComponent {

	[Tooltip("Body part this armor component covers.")]
	public BodyPart armorPart;

	public override string componentName
	{
		get { return ArmorManager.GetArmorComponentName(armorPart); }
	}

	public override BodyPart bodyPart
	{
		get { return armorPart; }
	}

	private void Awake()
	{
		InitializeComponent();
	}

	// equips an armor (distributes the nanites)
	public void DistributeArmor(Armor armor, float quantity)
	{
		isDynamic = armor.dynamic;
		ratingM3 = armor.ratingM3;
		InitializeArmor(quantity / maxVolume);
	}

	// unequips an armor, returning the quantity of armor removed
	public float RecoverArmor()
	{
		float volume = 0;
		for (int i = 0; i < cellCount; i++)
		{
			volume += cellVolumes[i];
		}
		cellVolumes = null;
		return volume;
	}

	// calculates the total volume of this armor component
	public float GetArmorVolume()
	{
		float volume = 0;
		for (int i = 0; i < cellCount; i++)
		{
			volume += cellVolumes[i];
		}
		return volume;
	}
}
