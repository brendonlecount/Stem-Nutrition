using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// button that resets the player's position to the scene entry after death
public class RestartButton : MonoBehaviour {
	SceneEntry sceneEntry;
	MovementController player;

	// Use this for initialization
	void Start () {
		GameObject go = GameObject.FindWithTag("SceneEntry");
		if (go != null)
		{
			sceneEntry = go.GetComponent<SceneEntry>();
		}

		go = GameObject.FindWithTag("Player");
		if (go != null)
		{
			player = go.GetComponent<MovementController>();
		}
	}
	// heals player, moves them to scene entry point, exits menu mode
	public void OnRestartClicked()
	{
		if (player != null)
		{
			player.armor.Heal();
			if (sceneEntry != null)
			{
				sceneEntry.MovePlayerToEntry();
			}
		}
		MenuManager.menuMode = MenuModes.none;
	}
}
