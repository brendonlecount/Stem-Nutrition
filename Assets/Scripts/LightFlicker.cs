using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// causes lights to flicker in intensity and position.
// not used for performance reasons (worked fine in the editor, but not when built)
public class LightFlicker : MonoBehaviour {
	public Light source;
	public bool flickerPosition;
	public float flickerPositionInterval;
	public float flickerPositionSpeed;
	public float flickerRadius;
	public bool flickerIntensity;
	public float flickerIntensityInterval;
	public float flickerIntensityMax;
	public float flickerIntensityMin;

	float flickerPositionTimer;
	float flickerIntensityTimer;
	Vector3 targetPosition;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// periodically calculate a random position on a sphere, and move towards that position
		if (flickerPosition)
		{
			if (flickerPositionTimer <= 0)
			{
				targetPosition = Random.onUnitSphere * flickerRadius;
				flickerPositionTimer = flickerPositionInterval;
			}
			else
			{
				flickerPositionTimer -= Time.deltaTime;
			}
			source.transform.localPosition = Vector3.MoveTowards(source.transform.localPosition, targetPosition, flickerPositionSpeed * Time.deltaTime);
		}

		// periodically randomize intensity within the specified range
		if (flickerIntensity)
		{
			if (flickerIntensityTimer <= 0f)
			{
				source.intensity = Random.Range(flickerIntensityMin, flickerIntensityMax);
				flickerIntensityTimer = flickerIntensityInterval;
			}
			else
			{
				flickerIntensityTimer -= Time.deltaTime;
			}
		}
	}
}
