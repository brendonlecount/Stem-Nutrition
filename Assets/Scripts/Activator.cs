using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class that items that can be activated inherit from
namespace Stem
{
	public abstract class Activator : MonoBehaviour
	{
		public string activatorName;	// display name of the activator

		public abstract void Activate(InputSource user);

		public abstract void StopActivate();
	}
}
