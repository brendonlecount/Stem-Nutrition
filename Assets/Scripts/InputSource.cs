using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for input sources (either player input or AI control)
// instantiates and drives a MovementController
public abstract class InputSource : MonoBehaviour {
	// MovementController prefab to instantiate
	[SerializeField] GameObject controllerPrefab = null;

	// getter/setter for currently controlled MovementController
	public MovementController controller { get; private set; }

	// Use this for initialization
	protected void StartUp () {
		// spawn and wake controller, and give it a reference to this input source
		GameObject go = GameObject.Instantiate(controllerPrefab, transform.position, transform.rotation);
		controller = go.GetComponent<MovementController>();
		controller.Wake();  // instantiates the target body and sets controller to awake
		controller.SetInputSource(this);
	}

	// used by gear that allow vision mode swapping
	public virtual CameraVisionMode visionMode
	{
		get; set;
	}

	public delegate void OnVisionModeChanged(CameraVisionMode visionMode);
	public event OnVisionModeChanged onVisionModeChanged;

	protected void SendVisionModeChangedEvent(CameraVisionMode visionMode)
	{
		if (onVisionModeChanged != null)
		{
			onVisionModeChanged(visionMode);
		}
	}
}
