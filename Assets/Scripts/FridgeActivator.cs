using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FridgeActivator : Stem.Activator
{
	private void Start()
	{
		GetComponent<MeshRenderer>().enabled = false;
	}

	public override void Activate(InputSource user)
	{
		FridgeMenu.GetInstance().ShowMenu();
	}

	public override void StopActivate()
	{
	}
}
