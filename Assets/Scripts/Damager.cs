using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for items that cause damage (beams, projectiles, and eventually melee), most of which will need to
// sort lists of raycasts when determining what was hit
public abstract class Damager : MonoBehaviour
{
	// Sorts hits by distance, low to high, using insertion sort
	// which should be more efficient that O(nlogn) sorts for small
	// lists. Might blow up if too many items were hit.
	// TODO: Consider also implementing O(nlogn) sort for when hits is longer than ~20.
	public static void SortHits(RaycastHit[] hits)
	{
		RaycastHit swapTemp;
		for (int i = 1; i < hits.Length; i++)
		{
			int j = i;
			while (j > 0 && hits[j-1].distance > hits[j].distance)
			{
				swapTemp = hits[j];
				hits[j] = hits[j - 1];
				hits[j - 1] = swapTemp;
				j--;
			}
		}
	}
}
