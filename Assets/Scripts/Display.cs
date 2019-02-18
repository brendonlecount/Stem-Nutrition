using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for displays that can be interacted with
// extend and attach to the display canvas
public abstract class Display : MonoBehaviour {

	public abstract void StartDisplay();

	public abstract void StopDisplay();
}
