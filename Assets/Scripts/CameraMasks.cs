using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Centralizes camera culling layer masks
// TODO: use this to apply masks to a simpler set of cameras
[CreateAssetMenu(fileName = "CameraMasks", menuName = "Managers/CameraMasks", order = 2)]
public class CameraMasks : ScriptableObject {

	[SerializeField] LayerMask mainMask;
	[SerializeField] LayerMask radarBackgroundMask;
	[SerializeField] LayerMask radarTargetMask;
	[SerializeField] LayerMask UIMask;
	[SerializeField] LayerMask crosshairMask;

	[SerializeField] LayerMask defaultEffectMask;
	[SerializeField] LayerMask xrayEffectMask;
	[SerializeField] LayerMask ampEffectMask;

	public int main
	{
		get { return mainMask.value; }
	}

	public int radarBackground
	{
		get { return radarBackgroundMask.value; }
	}

	public int radarTarget
	{
		get { return radarTargetMask.value; }
	}

	public int UI
	{
		get { return UIMask.value; }
	}

	public int crosshair
	{
		get { return crosshairMask.value; }
	}

	public int defaultEffect
	{
		get { return defaultEffectMask; }
	}
	
	public int xrayEffect
	{
		get { return xrayEffectMask; }
	}

	public int ampEffect
	{
		get { return ampEffectMask; }
	}
}
