using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to communicate whether this Center of Mass is detected and able to be shot at
public struct TargetingMatrix
{
	public bool canDetect;
	public bool canHitBeam;
	public bool canHitProjectile;
}

// used to communicate targeting parameters determining whether this COM can be detected and hit
[System.Serializable]
public class TargetingParams
{
	public bool hears;
	public bool seesVisual;
	public bool seesRadar;
	public bool hasRadar;
	public bool shootsBeams;
	public bool shootsProjectiles;
}

// detection categories (also used for indexing detection arrays, sloppy but efficient)
public enum DetectionMode { Auditory = 0, PassiveVisual = 1, ActiveVisual = 2, PassiveRadar = 3, ActiveRadar = 4 }

// Center of Mass node - it's what enemies find and target
// TODO: make it manage a collection of nodes for different
// body parts, so that peaking over cover lets enemies target the head, etc.
public class COMNode : MonoBehaviour {

	SortedDictionary<int, float>[] detectionModifiers;	// TODO use priority queue

	float[] detectionRadii;

	private void Awake()
	{
		// initialize detection modifier arrays
		detectionModifiers = new SortedDictionary<int, float>[5];
		detectionModifiers[(int)DetectionMode.Auditory] = new SortedDictionary<int, float>();
		detectionModifiers[(int)DetectionMode.PassiveVisual] = new SortedDictionary<int, float>();
		detectionModifiers[(int)DetectionMode.ActiveVisual] = new SortedDictionary<int, float>();
		detectionModifiers[(int)DetectionMode.PassiveRadar] = new SortedDictionary<int, float>();
		detectionModifiers[(int)DetectionMode.ActiveRadar] = new SortedDictionary<int, float>();

		// initialize detection radii array
		detectionRadii = new float[5];
	}

	// stores the largest detection radius out of all of the current detection multipliers
	public float GetDetectionRadius(DetectionMode mode)
	{
		return detectionRadii[(int)mode];
	}

	// adds a detection modifier
	public int AddDetectionModifier(DetectionMode mode, float modifier)
	{
		int categoryIndex = (int)mode;
		int modifierKey = GetModifierKey(detectionModifiers[categoryIndex]);
		detectionModifiers[categoryIndex].Add(modifierKey, modifier);
		detectionRadii[categoryIndex] = Mathf.Max(detectionRadii[categoryIndex], modifier);
		return modifierKey;
	}

	// removes a detection modifier
	public bool RemoveDetectionModifier(DetectionMode mode, int modifierKey)
	{
		int categoryIndex = (int)mode;
		if (modifierKey >= 0 && detectionModifiers[categoryIndex].Remove(modifierKey))
		{
			float newDetectionRadius = 0f;
			foreach (KeyValuePair<int, float> detectionModifier in detectionModifiers[categoryIndex])
			{
				newDetectionRadius = Mathf.Max(newDetectionRadius, detectionModifier.Value);
			}
			detectionRadii[categoryIndex] = newDetectionRadius;
			return true;
		}
		return false;
	}

	// determines whether this center of mass can be detected or hit
	// accomplishes this by checking distance from targeter to COM and comparing them to detection radii,
	// performing raycasts where appropriate to determine LOS. Also communicates with the LightManager
	// to determine if the target COM is illuminated when calculating passive visual detection
	public TargetingMatrix GetTargetingMatrix(Vector3 origin, TargetingParams targetingParams)
	{
		TargetingMatrix detectionMatrix = new TargetingMatrix();
		Vector3 heading = transform.position - origin;
		float range = heading.magnitude;

		if (targetingParams.shootsBeams)
		{
			detectionMatrix.canHitBeam = !Physics.Raycast(origin, heading, range, Masks.Raycast.blocksBeams);
		}
		else
		{
			detectionMatrix.canHitBeam = false;
		}

		if (targetingParams.shootsProjectiles)
		{
			detectionMatrix.canHitProjectile = !Physics.Raycast(origin, heading, range, Masks.Raycast.blocksProjectiles);
		}
		else
		{
			detectionMatrix.canHitProjectile = false;
		}

		if (targetingParams.hears)       // target hears
		{
			if (range < GetDetectionRadius(DetectionMode.Auditory))
			{
				if (!Physics.Raycast(origin, heading, range, Masks.Raycast.blocksAudio))
				{
					detectionMatrix.canDetect = true;
					return detectionMatrix;
				}
			}
		}

		if (targetingParams.hasRadar)	// targeter is actively broadcasting radar
		{
			if (range < GetDetectionRadius(DetectionMode.PassiveRadar))
			{
				if (!Physics.Raycast(origin, heading, range, Masks.Raycast.blocksRadar))
				{
					detectionMatrix.canDetect = true;
					return detectionMatrix;
				}
			}
		}

		if (targetingParams.seesRadar)	// targeter sees radar broadcasts
		{
			if (range < GetDetectionRadius(DetectionMode.ActiveRadar))
			{
				if (!Physics.Raycast(origin, heading, range, Masks.Raycast.blocksRadar))
				{
					detectionMatrix.canDetect = true;
					return detectionMatrix;
				}
			}
		}


		if (targetingParams.seesVisual)  // targeter sees active light
		{
			if (range < GetDetectionRadius(DetectionMode.ActiveVisual))
			{
				if (!Physics.Raycast(origin, heading, range, Masks.Raycast.blocksLight))
				{
					detectionMatrix.canDetect = true;
					return detectionMatrix;
				}
			}
		}

		if (targetingParams.seesVisual)  // targeter sees passive light
		{
			if (range < GetDetectionRadius(DetectionMode.PassiveVisual) && LightManager.GetIsIlluminated(transform.position))
			{
				if (!Physics.Raycast(origin, heading, range, Masks.Raycast.blocksLight))
				{
					detectionMatrix.canDetect = true;
					return detectionMatrix;
				}
			}
		}

		return detectionMatrix;
	}

	// calculates a unique key for the specified detection modifier category
	// TODO: create a detection modifier wrapper class and try tracking those with a list,
	// should allow you to add and remove them without needing a key
	int GetModifierKey(SortedDictionary<int, float> modifiers)
	{
		int modifierKey = 0;
		while (true)
		{
			if (modifiers.ContainsKey(modifierKey))
			{
				modifierKey++;
			}
			else
			{
				break;
			}
		}
		return modifierKey;
	}

	// is the COM illuminated?
	bool IsIlluminated()
	{
		return LightManager.GetIsIlluminated(transform.position);
	}

	// draws detection radii when gizmos are enabled
	private void OnDrawGizmos()
	{
		if (detectionRadii != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, GetDetectionRadius(DetectionMode.Auditory));
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, GetDetectionRadius(DetectionMode.PassiveVisual));
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(transform.position, GetDetectionRadius(DetectionMode.ActiveVisual));
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, GetDetectionRadius(DetectionMode.PassiveRadar));
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, GetDetectionRadius(DetectionMode.ActiveRadar));
		}
	}
}
