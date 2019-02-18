using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// UI button that causes a scene to load
// used for start menu, could also be used for elevator panels that provide access to multiple scenes
public class LoadSceneButton : MonoBehaviour {
	public string sceneName;

	public void OnClick()
	{
		SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
	}
}
