using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// button used to resume gameplay from pause menu
public class ResumeButton : MonoBehaviour {

	public void OnClick()
	{
		MenuManager.menuMode = MenuModes.none;
	}
}
