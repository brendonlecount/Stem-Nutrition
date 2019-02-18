using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// turns on/off lights depending on how far they are from the player.
// created for performance reasons, ended up going with "mixed" lighting type instead.
public class LightCuller : MonoBehaviour {
	public float onRadius = 20f;
	public Light lightObject;

	GameObject player;

	// Use this for initialization
	void Start () {
		player = GameObject.FindWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
		if ((transform.position - player.transform.position).sqrMagnitude > onRadius * onRadius)
		{
			lightObject.enabled = false;
		}
		else
		{
			lightObject.enabled = true;
		}
	}
}
