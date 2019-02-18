using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used by LightManager to keep track of light sources in dim scenes
public class IlluminationSource : MonoBehaviour {
	[SerializeField] Light lightSource;

	// return the range of the illumination source
	// TODO: implement full GetLightLevel calculation instead of isIlluminated boolean
	public float GetIlluminationRadius()
	{
		return lightSource.range;
	}
}
