using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// door that automatically gets out of the way (disables itself) when the
// player encounters the door trigger, also playing a sound on entry and exit
public class LaserDoor : MonoBehaviour {
	public GameObject model;
	public float closeDelay;

	float occupiedTimer;
	bool occupied;
	AudioSource openSound;

	private void Start()
	{
		openSound = GetComponent<AudioSource>();
	}

	// If the trigger has been unoccupied for closeDelay seconds and is currently open,
	// close the door. If it is occupied and unopened, open it.
	void Update () {
		if (occupiedTimer > 0f)
		{
			occupiedTimer -= Time.deltaTime;
			if (!occupied)
			{
				OpenDoor();
			}
		}
		else if (occupied)
		{
			CloseDoor();
		}
	}

	// use OnTriggerStay rather than enter/exit, since
	// enter/exit can be called multiple times.
	private void OnTriggerStay(Collider other)
	{
		occupiedTimer = closeDelay;
	}

	void OpenDoor()
	{
		model.SetActive(false);
		occupied = true;
		openSound.Play();
	}

	void CloseDoor()
	{
		occupied = false;
		model.SetActive(true);
		openSound.Play();
	}
}
