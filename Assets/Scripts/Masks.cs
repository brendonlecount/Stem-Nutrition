using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// singleton used to centralize layer masks for raycasts and camera culling.
// note: unity should really start using a different type of layer for raycasts, camera culling, 
// collision matrix, and camera effects, since those things don't necessarily always correspond /rant
public class Masks : MonoBehaviour {
	[SerializeField] RaycastMasks castingMasks = null;
	[SerializeField] CameraMasks cameraMasks = null;

	public static RaycastMasks Raycast { get; private set; }
	public static CameraMasks Camera { get; private set; }
	private static Masks MasksInstance { get; set; }

	// apply singleton pattern
	private void Awake()
	{
		if (MasksInstance != null && MasksInstance != this)
		{
			GameObject.Destroy(MasksInstance.gameObject);
		}

		MasksInstance = this;
		Raycast = this.castingMasks;
		Camera = this.cameraMasks;
	}
}
