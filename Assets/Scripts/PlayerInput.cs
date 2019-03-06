using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controls a MovementController via player input
// also instantiates the MovementController (see InputSource base class)
// should be attached to a prefab that contains a camera setup, Crosshair script and CameraController
public class PlayerInput : InputSource
{
	public float mouseSensitivity;	// TODO: make adjustable

	CameraController cameraController;
	Crosshair crosshair;

	bool walkToggled;
	bool is1stPerson = true;

	bool switchPressed;
	bool jumpPressed;
	bool crouchPressed;
	bool walkPressed;
	bool activatePressed;
	bool escapePressed;
	bool tabPressed;

	bool hotkey00Pressed;
	bool hotkey01Pressed;
	bool hotkey02Pressed;
	bool hotkey03Pressed;
	bool hotkey04Pressed;
	bool hotkey05Pressed;
	bool hotkey06Pressed;
	bool hotkey07Pressed;
	bool hotkey08Pressed;
	bool hotkey09Pressed;

	bool freezeControls = false;

	// Use this for initialization
	void Awake()
	{
		base.StartUp();

		crosshair = GetComponent<Crosshair>();
		cameraController = GetComponent<CameraController>();
		cameraController.InitializeCamera(controller);

		controller.armor.onOrganChange += OnOrganChange;

		controller.ambulation = Ambulation.Stand;
	}

	// input is processed differently depending on menumode
	void Update()
	{
		switch (MenuManager.menuMode)
		{
			case MenuModes.none:
				ProcessGameControls();
				break;
			case MenuModes.display:
				ProcessDisplayControls();
				break;
			case MenuModes.tab:
				ProcessTabControls();
				break;
			case MenuModes.pause:
				ProcessPauseControls();
				break;
			case MenuModes.death:
				ProcessDeathControls();
				break;
		}
	}

	// crosshair update, body target positioning, and weapon orientation
	// need to happen in the following order or jitters develop
	private void LateUpdate()
	{
		switch (MenuManager.menuMode)
		{
			case MenuModes.none:
				crosshair.UpdateCrosshair();
				controller.PositionTargets();
				controller.weapons.OrientWeapons(crosshair.GetTargetPosition());
				break;
			default:
				crosshair.HideCrosshair();
				break;
		}
	}

	// helper function that reads a hotkey and notifies the WeaponManager
	// if it is newly pressed
	bool ReadHotkey(bool pressed, bool hotkeyPressed, HolsterName holster)
	{
		if (pressed)
		{
			if (!hotkeyPressed)
			{
				controller.weapons.ToggleHolster(holster);
			}
			return true;
		}
		return false;
	}

	// apply game mode controls (menuMode == None)
	void ProcessGameControls()
	{
		// escape key triggers pause menu
		if (Input.GetAxisRaw("Cancel") > 0f)
		{
			if (!escapePressed)
			{
				escapePressed = true;
				MenuManager.menuMode = MenuModes.pause;
			}
		}
		else
		{
			escapePressed = false;
		}

		// tab key triggers tab menu
		if (Input.GetAxisRaw("Tab") > 0f)
		{
			if (!tabPressed)
			{
				tabPressed = true;
				MenuManager.menuMode = MenuModes.tab;
			}
		}
		else
		{
			tabPressed = false;
		}

		// activates currently selected activator, if any
		if (Input.GetAxisRaw("Activate") > 0f && !freezeControls)
		{
			if (!activatePressed)
			{
				activatePressed = true;
				if (crosshair.GetTargetActivator() != null)
				{
					crosshair.GetTargetActivator().Activate(this);
				}
			}
		}
		else
		{
			activatePressed = false;
		}

		// read camera switch key and apply camera perspective
		if (Input.GetAxisRaw("Switch Camera") > 0f)
		{
			if (!switchPressed)
			{
				switchPressed = true;
				is1stPerson = !is1stPerson;
			}
		}
		else
		{
			switchPressed = false;
		}

		// switch to 1st person if iron sights are raised
		if (is1stPerson || controller.weapons.isIronSights)
		{
			cameraController.perspective = CameraPerspective.First;
		}
		else
		{
			cameraController.perspective = CameraPerspective.Third;
		}

		if (freezeControls)
		{
			// freeze WASD movement and ambulation changes
			controller.SetMovement(Vector2.zero);

			// freeze hotkeys
			hotkey01Pressed = ReadHotkey(false, hotkey01Pressed, HolsterName.UnarmedRight);
			hotkey02Pressed = ReadHotkey(false, hotkey02Pressed, HolsterName.UnarmedLeft);
			hotkey03Pressed = ReadHotkey(false, hotkey03Pressed, HolsterName.ThighRight);
			hotkey04Pressed = ReadHotkey(false, hotkey04Pressed, HolsterName.ThighLeft);
			hotkey05Pressed = ReadHotkey(false, hotkey05Pressed, HolsterName.BackRight);
			hotkey06Pressed = ReadHotkey(false, hotkey06Pressed, HolsterName.BackLeft);
			hotkey07Pressed = ReadHotkey(false, hotkey07Pressed, HolsterName.ShoulderRight);
			hotkey08Pressed = ReadHotkey(false, hotkey08Pressed, HolsterName.ShoulderLeft);
			hotkey09Pressed = ReadHotkey(false, hotkey09Pressed, HolsterName.Chest);
			hotkey00Pressed = ReadHotkey(false, hotkey00Pressed, HolsterName.Head);

			// freeze primary attack
			controller.weapons.SetPrimaryAttack(false);
			// freeze secondary attack
			controller.weapons.SetSecondaryAttack(false);
		}
		else
		{
			// WASD movement
			Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			bool isMoving = false;
			if (movement.sqrMagnitude > Mathf.Epsilon)
			{
				controller.SetMovement(movement);
				isMoving = true;
			}

			// determine and apply current ambulation
			Ambulation updatedAmbulation = controller.ambulation;
			Pose updatedPose = controller.pose;
			// read jump key
			if (Input.GetAxisRaw("Jump") > 0f)
			{
				if (!jumpPressed)
				{
					jumpPressed = true;
					switch (updatedPose)
					{
						case Pose.Standing:
							controller.TriggerJump();
							break;
						case Pose.Crouching:
							updatedPose = Pose.Standing;
							break;
						case Pose.Prone:
							updatedPose = Pose.Crouching;
							break;
					}
				}
			}
			else
			{
				jumpPressed = false;
			}

			// read crouch key
			if (Input.GetAxisRaw("Crouch") > 0f)
			{
				if (!crouchPressed)
				{
					crouchPressed = true;
					switch (updatedPose)
					{
						case Pose.Standing:
							updatedPose = Pose.Crouching;
							break;
						case Pose.Crouching:
							updatedPose = Pose.Prone;
							break;
					}
				}
			}
			else
			{
				crouchPressed = false;
			}

			// read walk toggle
			if (Input.GetAxisRaw("Walk") > 0f)
			{
				if (!walkPressed)
				{
					walkPressed = true;
					walkToggled = !walkToggled;
				}
			}
			else
			{
				walkPressed = false;
			}

			// read sprint key
			if (Input.GetAxisRaw("Sprint") > 0f)
			{
				// select sprinting and stand up if sprint pressed
				updatedAmbulation = Ambulation.Sprint;
				updatedPose = Pose.Standing;
			}
			else
			{
				// otherwise, determine ambulation from pose and directional input
				switch (updatedPose)
				{
					case Pose.Standing:
						if (isMoving)
						{
							if (walkToggled)
							{
								updatedAmbulation = Ambulation.Walk;
							}
							else
							{
								updatedAmbulation = Ambulation.Run;
							}
						}
						else
						{
							updatedAmbulation = Ambulation.Stand;
						}
						break;
					case Pose.Crouching:
						if (isMoving)
						{
							updatedAmbulation = Ambulation.Sneak;
						}
						else
						{
							updatedAmbulation = Ambulation.Crouch;
						}
						break;
					case Pose.Prone:
						if (isMoving)
						{
							updatedAmbulation = Ambulation.Crawl;
						}
						else
						{
							updatedAmbulation = Ambulation.Lay;
						}
						break;
				}
			}
			// apply determined ambulation (pose set by MovementController based on ambulation)
			controller.ambulation = updatedAmbulation;

			// read hotkeys
			hotkey01Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey01") > 0f, hotkey01Pressed, HolsterName.UnarmedRight);
			hotkey02Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey02") > 0f, hotkey02Pressed, HolsterName.UnarmedLeft);
			hotkey03Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey03") > 0f, hotkey03Pressed, HolsterName.ThighRight);
			hotkey04Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey04") > 0f, hotkey04Pressed, HolsterName.ThighLeft);
			hotkey05Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey05") > 0f, hotkey05Pressed, HolsterName.BackRight);
			hotkey06Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey06") > 0f, hotkey06Pressed, HolsterName.BackLeft);
			hotkey07Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey07") > 0f, hotkey07Pressed, HolsterName.ShoulderRight);
			hotkey08Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey08") > 0f, hotkey08Pressed, HolsterName.ShoulderLeft);
			hotkey09Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey09") > 0f, hotkey09Pressed, HolsterName.Chest);
			hotkey00Pressed = ReadHotkey(Input.GetAxisRaw("Hotkey00") > 0f, hotkey00Pressed, HolsterName.Head);


			// primary attack
			controller.weapons.SetPrimaryAttack(Input.GetAxisRaw("Fire1") > 0f);
			// secondary attack
			controller.weapons.SetSecondaryAttack(Input.GetAxisRaw("Fire2") > 0f);
		}
		// mouselook
		float adjustedSensitivity = mouseSensitivity / controller.weapons.magnification;
		// up/down
		cameraController.pitch = cameraController.pitch - Input.GetAxis("Mouse Y") * adjustedSensitivity;
		// left/right
		controller.SetYaw(controller.GetYaw() + Input.GetAxis("Mouse X") * adjustedSensitivity);

		cameraController.isScoped = controller.weapons.isIronSights;
	}

	// when using an in-game display...
	void ProcessDisplayControls()
	{
		// tell the movement controller to stand still
		SetStill();

		// exit the display if activate is pressed again
		if (Input.GetAxisRaw("Activate") > 0f)
		{
			if (!activatePressed)
			{
				activatePressed = true;
				if (crosshair.GetTargetActivator() != null)
				{
					crosshair.GetTargetActivator().StopActivate();
				}
			}
		}
		else
		{
			activatePressed = false;
		}

		// also exit the display if escape is pressed
		if (Input.GetAxisRaw("Cancel") > 0f)
		{
			if (!escapePressed)
			{
				escapePressed = true;
				if (crosshair.GetTargetActivator() != null)
				{
					crosshair.GetTargetActivator().StopActivate();
				}
			}
		}
		else
		{
			escapePressed = false;
		}

	}

	// tells the MovementController to stand still
	private void SetStill()
	{
		switch (controller.pose)
		{
			case Pose.Standing:
				controller.ambulation = Ambulation.Stand;
				break;
			case Pose.Crouching:
				controller.ambulation = Ambulation.Crouch;
				break;
			case Pose.Prone:
				controller.ambulation = Ambulation.Lay;
				break;
		}
	}

	// when in the tab menu...
	void ProcessTabControls()
	{
		// stand still...
		SetStill();

		// exit if tab pressed again
		if (Input.GetAxisRaw("Tab") > 0f)
		{
			if (!tabPressed)
			{
				tabPressed = true;
				MenuManager.menuMode = MenuModes.none;
			}
		}
		else
		{
			tabPressed = false;
		}

		// exit if escape pressed
		if (Input.GetAxisRaw("Cancel") > 0f)
		{
			if (!escapePressed)
			{
				escapePressed = true;
				MenuManager.menuMode = MenuModes.none;
			}
		}
		else
		{
			escapePressed = false;
		}
	}

	// when in pause menu
	void ProcessPauseControls()
	{
		// maybe not needed since game is paused?
		SetStill();

		// exit menu if escape pressed again
		if (Input.GetAxisRaw("Cancel") > 0f)
		{
			if (!escapePressed)
			{
				escapePressed = true;
				MenuManager.menuMode = MenuModes.none;
			}
		}
		else
		{
			escapePressed = false;
		}
	}

	// do nothing...
	void ProcessDeathControls()
	{

	}

	// trigger death menu if an organChange event is sent and isDead
	public void OnOrganChange(bool isDead)
	{
		if (isDead)
		{
			MenuManager.menuMode = MenuModes.death;
		}
	}

	// implement the visionMode getter and setter by passing it along to the camera controller
	// (AI controlled characters will use it to determine detection parameters)
	public override CameraVisionMode visionMode
	{
		get { return cameraController.visionMode; }
		set
		{
			if (value != cameraController.visionMode)
			{
				cameraController.visionMode = value;
				SendVisionModeChangedEvent(cameraController.visionMode);
			}
		}
	}

	public void FreezeControls(bool freeze)
	{
		freezeControls = freeze;
	}
}
