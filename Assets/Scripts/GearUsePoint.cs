using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for use points (left hand and right hand)
public abstract class GearUsePoint : MonoBehaviour {
	// transform where weapon will be equipped
	public Transform useNode
	{
		get { return this.transform; }
	}

	// current holster to draw from
	public GearHolster drawHolster
	{
		get; set;
	}

	// is the hand damaged? used to prevent firing when arms are broken.
	public bool isCompromised { get; set; }
}
