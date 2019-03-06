using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityActivator : Stem.Activator
{
	[SerializeField] string activityName = "";

	private void Start()
	{
		GetComponent<MeshRenderer>().enabled = false;
	}

	public override void Activate(InputSource user)
	{
		ActivityTimeMenu.GetInstance().onTimeSelected += OnTimeSelected;
		ActivityTimeMenu.GetInstance().ShowMenu(activityName);
	}

	public override void StopActivate()
	{
		
	}

	public void OnTimeSelected(float activityTime, bool canceled)
	{
		ActivityTimeMenu.GetInstance().onTimeSelected -= OnTimeSelected;
	}
}
