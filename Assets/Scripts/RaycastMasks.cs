using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// centralizes raycast layer masks
[CreateAssetMenu(fileName = "RaycastMasks", menuName = "Managers/RaycastMasks", order = 1)]
public class RaycastMasks : ScriptableObject
{
	[SerializeField] LayerMask blocksAudioMask;
	[SerializeField] LayerMask blocksLightMask;
	[SerializeField] LayerMask blocksRadarMask;
	[SerializeField] LayerMask blocksBeamsMask;
	[SerializeField] LayerMask blocksProjectilesMask;
	[SerializeField] LayerMask hitsProjectilesMask;
	[SerializeField] LayerMask hitsBeamsMask;
	[SerializeField] LayerMask standableMask;
	[SerializeField] LayerMask crosshairMask;

	public int blocksAudio
	{
		get { return blocksAudioMask.value; }
	}

	public int blocksLight
	{
		get { return blocksLightMask.value; }
	}

	public int blocksRadar
	{
		get { return blocksRadarMask.value; }
	}

	public int blocksBeams
	{
		get { return blocksBeamsMask.value; }
	}

	public int blocksProjectiles
	{
		get { return blocksProjectilesMask.value; }
	}

	public int hitsProjectiles
	{
		get { return hitsProjectilesMask.value; }
	}

	public int hitsBeams
	{
		get { return hitsBeamsMask.value; }
	}

	public int standable
	{
		get { return standableMask.value; }
	}

	public int crosshair
	{
		get { return crosshairMask.value; }
	}
}
