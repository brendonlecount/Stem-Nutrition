using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Armor component variant for non-humanoids (no associated body part, and armor values are specified directly)
public class ArmorComponentIndependent : ArmorComponent {
	public string armorComponentName;
	public float rating;
	public bool dynamic;

	public override string componentName
	{
		get
		{
			return armorComponentName;
		}
	}

	// initializes the armor based on the specified stats
	private void Awake()
	{
		InitializeComponent();
		ratingM3 = rating / Armor.UNIT_VOLUME;
		isDynamic = dynamic;
		InitializeArmor(1f);
	}
}
