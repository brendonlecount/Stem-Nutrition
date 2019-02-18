using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// menu modes (game states)
public enum MenuModes { none, display, tab, pause, death }

// singleton that keeps track of whether the player is in a menu,
// pausing/unpausing the game and hiding/unhiding the cursor and crosshair,
// and hiding/showing relevant menus
public class MenuManager : MonoBehaviour {
	public Canvas hudCanvas;
	public Canvas pauseMenuCanvas;
	public Canvas tabMenuCanvas;
	public Canvas deathMenuCanvas;

	public static MenuManager Instance { get; private set; }

	// getter/setter for menu mode
	public static MenuModes menuMode
	{
		get { return Instance._menuMode; }
		// when set, exit current menu mode and enter new one
		set
		{
			switch (Instance._menuMode)
			{
				case MenuModes.none:
					Instance.ExitNoneMode();
					break;
				case MenuModes.display:
					Instance.ExitDisplayMode();
					break;
				case MenuModes.tab:
					Instance.ExitTabMode();
					break;
				case MenuModes.pause:
					Instance.ExitPauseMode();
					break;
				case MenuModes.death:
					Instance.ExitDeathMode();
					break;
			}
			switch (value)
			{
				case MenuModes.none:
					Instance.EnterNoneMode();
					break;
				case MenuModes.display:
					Instance.EnterDisplayMode();
					break;
				case MenuModes.tab:
					Instance.EnterTabMode();
					break;
				case MenuModes.pause:
					Instance.EnterPauseMode();
					break;
				case MenuModes.death:
					Instance.EnterDeathMode();
					break;
			}
			Instance._menuMode = value;
		}
	}
	MenuModes _menuMode;

	// apply singelton pattern and initialize menu mode to none
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			GameObject.Destroy(Instance.gameObject);
		}

		Instance = this;

		menuMode = MenuModes.none;
	}

	// functions for entering and exiting menu modes
	void ExitNoneMode() { hudCanvas.gameObject.SetActive(false); }

	void ExitDisplayMode() { }

	void ExitTabMode() { tabMenuCanvas.gameObject.SetActive(false); }

	void ExitPauseMode()
	{
		pauseMenuCanvas.gameObject.SetActive(false);
		Time.timeScale = 1f;
	}

	void ExitDeathMode()
	{
		deathMenuCanvas.gameObject.SetActive(false);
		Time.timeScale = 1f;
	}

	void EnterNoneMode()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		hudCanvas.gameObject.SetActive(true);
	}

	void EnterDisplayMode()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	void EnterTabMode()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		tabMenuCanvas.gameObject.SetActive(true);
	}

	void EnterPauseMode()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 0f;
		pauseMenuCanvas.gameObject.SetActive(true);
	}

	void EnterDeathMode()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 0f;
		deathMenuCanvas.gameObject.SetActive(true);
	}
}
