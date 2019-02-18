using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Acts as an entry point into a scene, spawning a PlayerInput if one is not already found,
// or moving the player to the entry point if one was found. Place on the ground at the position
// and orientation the player should spawn at, and specify an appropriate PlayerInput prefab.
// TODO: create a manager that allows for multiple scene entries, selecting the appropriate
// one from the door used to enter the scene
public class SceneEntry : MonoBehaviour {
	[Tooltip("Player Camera prefab found in Inputs prefab folder.")]
	public GameObject controllerPrefab;

	PlayerInput playerInput;
	bool pollForPlayer = false;

	private void Start()
	{
		GameObject go = GameObject.FindWithTag("PlayerController");
		if (go == null)
		{
			go = GameObject.Instantiate(controllerPrefab, transform.position, transform.rotation);
			playerInput = go.GetComponent<PlayerInput>();
			pollForPlayer = true;
		}
		else
		{
			playerInput = go.GetComponent<PlayerInput>();
			MovePlayerToEntry();
		}
	}

	// poll for the player if one was newly instantiated (it might take a frame for everything to be fully instantiated)
	private void Update()
	{
		if (pollForPlayer && playerInput.controller != null && playerInput.controller.isAwake)
		{
			pollForPlayer = false;
			MovePlayerToEntry();
		}
	}

	// moves the player to the position and orientation of the scene entry gameobject
	public void MovePlayerToEntry()
	{
		playerInput.controller.SetLocation(transform.position, transform.rotation.eulerAngles.y);
	}
}
