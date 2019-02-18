using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// causes an item, like a light, to bob up and down like it's hovering
// not used, because of performance issues
public class Bobber : MonoBehaviour {
	public float bobDisplacement;	// how far it should move
	public float bobPeriod;			// how long it should take

	float bobTimer;
	Vector3 origin;

	// Use this for initialization
	void Start () {
		bobTimer = Random.Range(0f, bobPeriod);
		origin = transform.position;
	}
	
	// Update the object's position, based on sinusoidal oscillation
	// TODO: try writing a lookup table based quick-trig class for better performance
	void Update () {
		bobTimer += Time.deltaTime;
		if (bobTimer > bobPeriod)
		{
			bobTimer -= bobPeriod;
		}

		transform.position = origin + Vector3.up * bobDisplacement * Mathf.Sin(2f * Mathf.PI * bobTimer / bobPeriod);
	}
}
