using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to destroy spawned sound effects after they have finished playing
public class SoundDestroyer : MonoBehaviour {
	AudioSource source;
	// Use this for initialization
	void Start () {
		source = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!source.isPlaying)
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
