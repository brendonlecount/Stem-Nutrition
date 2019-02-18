using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO: implement bar justifications (for now it's always "Center")
public enum BarJustification { Min, Center, Max }

// Attached to a UI parent gameobject with children labels and bar images
// allows health bars, etc., to be updated easily by menu scripts
public class Bar : MonoBehaviour {
	public bool clamp = true;		// stop the bar from overflowing?
	public bool vertical = false;	// is the bar horizontal or vertical?
	public BarJustification justification = BarJustification.Center;	// bar justification (not used)

	RectTransform barObject;
	Text barLabel;
	Text barTextObject;
	Text barTextObject2;
	Vector2 fullScale;

	bool initialized = false;


	// Use this for initialization
	void Awake () {
		InitializeBar();
	}

	// get ahold of child components (label, value text, bar image)
	public void InitializeBar()
	{
		if (!initialized)
		{
			initialized = true;
			RectTransform[] transforms = GetComponentsInChildren<RectTransform>(true);
			foreach (RectTransform rect in transforms)
			{
				if (rect.name == "Bar")
				{
					barObject = rect;
					fullScale = barObject.sizeDelta;
					break;
				}
			}

			Text[] texts = GetComponentsInChildren<Text>(true);
			foreach (Text text in texts)
			{
				if (text.name == "Label")
				{
					barLabel = text;
				}
				else if (text.name == "Value")
				{
					barTextObject = text;
				}
				else if (text.name == "Value 2")
				{
					barTextObject2 = text;
				}
			}
		}
	}

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

	// getter/setter for text value 1
	public string text
	{
		set
		{
			if (barTextObject != null)
			{
				barTextObject.text = value;
			}
		}

		get
		{
			if (barTextObject == null)
			{
				return "";
			}
			else
			{
				return barTextObject.text;
			}
		}
	}

	// getter/setter for text value 2
	public string text2
	{
		set
		{
			if (barTextObject2 != null)
			{
				barTextObject2.text = value;
			}
		}

		get
		{
			if (barTextObject2 == null)
			{
				return "";
			}
			else
			{
				return barTextObject2.text;
			}
		}
	}

	// getter/setter for bar
	public float barValue
	{
		set
		{
			if (clamp)
			{
				_barValue = Mathf.Clamp(value, 0f, 1f);
			}
			else
			{
				_barValue = value;
			}
			if (barObject != null)
			{
				if (vertical)
				{
					barObject.sizeDelta = new Vector2(fullScale.x, fullScale.y * _barValue);
				}
				else
				{
					barObject.sizeDelta = new Vector2(fullScale.x * _barValue, fullScale.y);
				}
			}
		}
		get
		{
			return _barValue;
		}
	}
	float _barValue = 1f;
}
