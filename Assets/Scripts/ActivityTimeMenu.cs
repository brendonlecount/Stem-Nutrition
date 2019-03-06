using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivityTimeMenu : MonoBehaviour
{
	[SerializeField] GameObject menuPanel = null;
	[SerializeField] Text activityText = null;
	[SerializeField] Text timeText = null;
	[SerializeField] Slider timeSlider = null;

	public delegate void OnTimeSelected(float time, bool canceled);
	public event OnTimeSelected onTimeSelected;

	private static ActivityTimeMenu instance = null;

	private void Awake()
	{
		instance = this;
	}

	public static ActivityTimeMenu GetInstance() { return instance; }

	public void ShowMenu(string activityName)
	{
		activityText.text = activityName;
		timeSlider.value = 1f;
		SetTimeText();
		menuPanel.SetActive(true);
		MenuManager.menuMode = MenuModes.display;
	}

	public void OnValueChanged()
	{
		SetTimeText();
	}

	public void BeginClicked()
	{
		MenuManager.menuMode = MenuModes.none;
		menuPanel.SetActive(false);
		if (onTimeSelected != null)
		{
			onTimeSelected(timeSlider.value, false);
		}
	}

	public void CancelClicked()
	{
		MenuManager.menuMode = MenuModes.none;
		menuPanel.SetActive(false);
		if (onTimeSelected != null)
		{
			onTimeSelected(0f, true);
		}
	}

	void SetTimeText()
	{
		timeText.text = timeSlider.value.ToString("N2") + " hours";
	}
}
