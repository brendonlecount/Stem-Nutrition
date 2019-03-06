using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodButton : MonoBehaviour
{
	[SerializeField] FoodNI foodNI = null;
	[SerializeField] Text buttonText = null;

	private void OnEnable()
	{
		buttonText.text = foodNI.foodName;
	}

	public FoodNI GetFoodNI() { return foodNI; }
}
