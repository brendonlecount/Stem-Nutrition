using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MultiBarBar
{
	public Image barImage = null;
	public float barValue = 0f;
}

public class MultiBar : MonoBehaviour
{
	[SerializeField] Text barLabel = null;
	[Tooltip("Bars should be entered in the order they show up in the heirarchy.")]
	[SerializeField] MultiBarBar[] bars;

	// getter/setter for label
	public string label
	{
		set
		{
			if (barLabel != null)
			{
				barLabel.text = value;
			}
		}

		get
		{
			if (barLabel == null)
			{
				return "";
			}
			else
			{
				return barLabel.text;
			}
		}
	}

	public void SetBarValue(int index, float barValue)
	{
		if (index >= 0 && index < bars.Length)
		{
			bars[index].barValue = barValue;
		}
		else
		{
			Debug.Log("Sub-bar " + index + " not found!");
		}
	}

	public void UpdateBarFills()
	{
		float barSum = 0f;
		for (int i = bars.Length - 1; i >= 0; i--)
		{
			barSum += bars[i].barValue;
			bars[i].barImage.fillAmount = barSum;
		}
	}
}
