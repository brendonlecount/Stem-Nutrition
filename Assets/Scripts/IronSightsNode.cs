using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// tag for node that weapons get attached to when drawn in iron sights mode
// also aligns itself depending on the weapon's iron sight offset
public class IronSightsNode : MonoBehaviour {

	// align node to accomodate weapon size;
	// weapons have different offsets from their origin to the point that
	// should be aligned with the camera in iron sights mode, this accounts
	// for that, but requires the weapon to have a node that represents the 
	// point that should line up (either the sight or the scope lens)
	// works surprisingly well!
	public void Align(Transform weaponOffset)
	{
		transform.localPosition = -weaponOffset.localPosition;
	}
}
