using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// perspective of the camera
public enum CameraPerspective { First, Third }

// vision mode
public enum CameraVisionMode { Default, XRay, Amplification }

// manages cameras in the scene, applying up/down mouselook, vision modes, and camera perspectives
public class CameraController : MonoBehaviour {
	[SerializeField] Camera mainCamera = null;
	[SerializeField] Camera crosshairCamera = null;
	[SerializeField] Camera radarTargetCamera = null;

	[SerializeField] Light radarLight = null;

	[SerializeField] float clampMax = 80f;
	[SerializeField] float clampMin = -80f;

	[SerializeField] bool viewPivot = false;

	[SerializeField] float radarDetectionRadius = 35f;

	// is the player current using a weapon scope?
	// if so, disable the crosshair camera, since crosshair will be rendered by scope
	// also, disable special vision modes when scoped and reenable them when not scoped
	public bool isScoped
	{
		get { return _isScoped; }
		set
		{
			if (value != isScoped)
			{
				_isScoped = value;
				crosshairCamera.enabled = !value;
				if (isScoped)
				{
					if (visionMode != CameraVisionMode.Default)
					{
						ApplyVisionMode(CameraVisionMode.Default, mainCamera, radarTargetCamera, radarLight);
					}
				}
				else
				{
					if (visionMode != CameraVisionMode.Default)
					{
						ApplyVisionMode(visionMode, mainCamera, radarTargetCamera, radarLight);
					}
				}

			}
		}
	}
	bool _isScoped = false;

	// what perspective is currently applied?
	public CameraPerspective perspective
	{
		get { return _perspective; }
		set
		{
			if (value != _perspective)
			{
				if (value == CameraPerspective.First)
				{
					SwitchToFirstPerson();
				}
				else
				{
					SwitchToThirdPerson();
				}
			}
		}
	}
	CameraPerspective _perspective;

	// what vision mode is currently applied?
	// if scoped, do not apply non-default vision modes
	public CameraVisionMode visionMode
	{
		get { return _visionMode; }
		set
		{
			if (value != visionMode)
			{
				_visionMode = value;
				if (value == CameraVisionMode.Default || !isScoped)
				{
					ApplyVisionMode(value, mainCamera, radarTargetCamera, radarLight);
				}
				if (value == CameraVisionMode.XRay)
				{
					radarDetectionRadiusID = comNode.AddDetectionModifier(DetectionMode.ActiveRadar, radarDetectionRadius);
				}
				else
				{
					comNode.RemoveDetectionModifier(DetectionMode.ActiveRadar, radarDetectionRadiusID);
				}
			}
		}
	}
	CameraVisionMode _visionMode = CameraVisionMode.Default;

	// what is the up/down mouselook angle?
	public float pitch
	{
		get { return _pitch; }
		set
		{
			_pitch = Mathf.Clamp(value, clampMin, clampMax);
		}
	}
	float _pitch;

	Transform firstPersonNode;
	Transform thirdPersonNode;
	Transform thirdPersonPivot;

	COMNode comNode;
	int radarDetectionRadiusID = -1;

	Quaternion thirdRotation;

	bool isAwake = false;

	// Use this for initialization
	public void InitializeCamera(MovementController controller)
	{
		// apply camera masks
		mainCamera.cullingMask = Masks.Camera.main;
		crosshairCamera.cullingMask = Masks.Camera.crosshair;
		radarTargetCamera.cullingMask = Masks.Camera.radarTarget;

		// get camera nodes and com node from controller
		firstPersonNode = controller.gameObject.GetComponentInChildren<FirstPersonCameraNode>().transform;
		thirdPersonNode = controller.gameObject.GetComponentInChildren<ThirdPersonCameraNode>().transform;
		thirdPersonPivot = controller.gameObject.GetComponentInChildren<ThirdPersonCameraPivot>().transform;
		comNode = controller.GetComponentInChildren<COMNode>();

		// reset pitch
		pitch = 0f;

		// aim 3rd person camera at pivot point if viewPivot is checked
		float rotation = 0f;
		if (viewPivot)
		{
			rotation = Mathf.Atan(thirdPersonNode.transform.localPosition.y / thirdPersonNode.transform.localPosition.z) * Mathf.Rad2Deg;
		}
		thirdRotation = Quaternion.Euler(-rotation, 0f, 0f);

		// apply first person camera mode
		SwitchToFirstPerson();

		// apply default vision mode
		ApplyVisionMode(CameraVisionMode.Default, mainCamera, radarTargetCamera, radarLight);
		_visionMode = CameraVisionMode.Default;

		isAwake = true;
	}

	// Apply current up/down mouselook
	// TODO: try setting rotations directly in pitch setter (called by PlayerInput)
	void Update()
	{
		if (isAwake)
		{
			Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
			firstPersonNode.localRotation = rotation;
			thirdPersonPivot.localRotation = rotation;
		}
	}

	// Apply camera perspectives
	void SwitchToFirstPerson()
	{
		_perspective = CameraPerspective.First;
		transform.parent = firstPersonNode;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}

	void SwitchToThirdPerson()
	{
		_perspective = CameraPerspective.Third;
		transform.parent = thirdPersonNode;
		transform.localPosition = Vector3.zero;
		transform.localRotation = thirdRotation;
	}

	// applies vision modes to a set of cameras by setting their culling masks, clear flags, and post processing layer layermasks
	// static function called here and by weapon scopes
	public static void ApplyVisionMode(CameraVisionMode visionMode, Camera mainCamera, Camera targetCamera, Light radarLight)
	{
		switch (visionMode)
		{
			case CameraVisionMode.Default:
				mainCamera.cullingMask = Masks.Camera.main;
				mainCamera.clearFlags = CameraClearFlags.Skybox;
				mainCamera.GetComponent<PostProcessLayer>().volumeLayer = Masks.Camera.defaultEffect;
				targetCamera.enabled = false;
				radarLight.enabled = false;
				break;
			case CameraVisionMode.Amplification:
				mainCamera.cullingMask = Masks.Camera.main;
				mainCamera.clearFlags = CameraClearFlags.Skybox;
				mainCamera.GetComponent<PostProcessLayer>().volumeLayer = Masks.Camera.ampEffect;
				targetCamera.enabled = false;
				radarLight.enabled = false;
				break;
			case CameraVisionMode.XRay:
				mainCamera.cullingMask = Masks.Camera.radarBackground;
				mainCamera.clearFlags = CameraClearFlags.SolidColor;
				mainCamera.GetComponent<PostProcessLayer>().volumeLayer = Masks.Camera.xrayEffect;
				targetCamera.enabled = true;
				targetCamera.cullingMask = Masks.Camera.radarTarget;
				radarLight.enabled = true;
				break;
		}
	}
}
