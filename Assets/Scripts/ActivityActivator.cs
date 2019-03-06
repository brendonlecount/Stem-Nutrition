using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityActivator : Stem.Activator
{
	[SerializeField] ActivityOverride activityOverride = null;
	[SerializeField] float duration = 10f;

	PlayerInput player;
	Physique physique;

	private void Start()
	{
		GetComponent<MeshRenderer>().enabled = false;
	}

	public override void Activate(InputSource user)
	{
		ActivityTimeMenu.GetInstance().onTimeSelected += OnTimeSelected;
		ActivityTimeMenu.GetInstance().ShowMenu(activityOverride.GetActionData().name);
	}

	public override void StopActivate()
	{
		
	}

	public void OnTimeSelected(float activityTime, bool canceled)
	{
		ActivityTimeMenu.GetInstance().onTimeSelected -= OnTimeSelected;

		GameObject go = GameObject.FindGameObjectWithTag("PlayerController");
		if (go != null)
		{
			player = go.GetComponent<PlayerInput>();
			physique = player.controller.physique;

			physique.SetActivityOverride(activityOverride, activityTime * 3600f, activityTime * 3600f / duration);
			physique.onActivityOverrideEnded += OnActivityOverrideCompleted;
			player.FreezeControls(true);
		}
	}

	public void OnActivityOverrideCompleted(ActivityOverride activityOverride)
	{
		physique.onActivityOverrideEnded -= OnActivityOverrideCompleted;
		if (activityOverride == this.activityOverride)
		{
			player.FreezeControls(false);
		}
	}
}
