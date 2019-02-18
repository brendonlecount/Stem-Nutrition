using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Activator that acts as a door between scenes
public class SceneDoor : Stem.Activator {
	public string scene;

	public override void Activate(InputSource user)
	{
		SceneManager.LoadScene(scene, LoadSceneMode.Single);
	}

	public override void StopActivate() { }
}