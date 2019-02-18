using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// not used?
public class UICameraGrabber : MonoBehaviour {
	private void Awake()
	{
		Canvas canvas = GetComponent<Canvas>();
		if (canvas != null)
		{
			GameObject go = GameObject.FindWithTag("UICamera");
			if (go != null)
			{
				Camera uiCamera = go.GetComponent<Camera>();
				if (uiCamera != null)
				{
					canvas.worldCamera = uiCamera;
					canvas.renderMode = RenderMode.ScreenSpaceCamera;
					Debug.Log("Camera assigned.");
				}
			}
		}
	}
}
