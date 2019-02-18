using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to keep track of illumination in a scene.
// if "isBright", the scene is assumed to have enough ambient light to illuminate characters everywhere.
// if not, raycasts are performed to determine if characters are in light or shadow.
// TODO: switch from a boolean isIlluminated to a GetLightLevel function used to adjust detection radii
// TODO: use as base class for dim and bright versions (bright version doesn't need to collect light sources)
// note: uses singleton pattern
public class LightManager : MonoBehaviour {
	protected static LightManager Instance { get; set; }

	public bool isBright;

	IlluminationSource[] illuminationSources;

	private void Awake()
	{
		// apply singleton pattern
		if (Instance != null && Instance != this)
		{
			GameObject.Destroy(Instance.gameObject);
		}

		Instance = this;

		// get ahold of illumination sources via IlluminationSource component
		// does not necessarily need to happen in bright scenes, but some scenes might
		// end up switching from light to dark...
		illuminationSources = FindObjectsOfType<IlluminationSource>();
	}

	public static bool GetIsIlluminated(Vector3 position)
	{
		return Instance.isBright || CheckIllumination(position);
	}

	// does raycasts to determine line of sight on illumination sources within range, returning
	// true if any are within range and have LOS
	// could eventually calculate light level by summing light intensities/ranges
	private static bool CheckIllumination(Vector3 position)
	{
		foreach (IlluminationSource source in Instance.illuminationSources)
		{
			Vector3 heading = position - source.transform.position;
			float range = heading.magnitude;
			if (source.GetIlluminationRadius() >= range)
			{
				if (!Physics.Raycast(source.transform.position, heading, range, Masks.Raycast.blocksLight))
				{
					Debug.Log("Illuminated.");
					return true;
				}
			}
		}
		Debug.Log("Not illuminated.");
		return false;
	}
}
