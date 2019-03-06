using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Activity", menuName = "Activities/Activity Overrides", order = 2)]
public class ActivityOverride : ScriptableObject
{
	[SerializeField] ActionData actionData = null;
	[SerializeField] float dutyCycle = 1f;

	// For weight lifting:
	// https://www.catalystathletics.com/resources/power-output/

	public ActionData GetActionData() { return actionData; }
	public float GetDutyCycle() { return dutyCycle; }
}
