using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "Foods", order = 1)]
public class FoodNI : ScriptableObject
{
	// macronutrient information
	[SerializeField] string foodNameText = "";
	[SerializeField] float proteinGrams = 0f;
	[SerializeField] float carbsGrams = 0f;
	[SerializeField] float fatGrams = 0f;
	[SerializeField] float waterGrams = 0f;

	// http://nutritiondata.self.com/facts/fast-foods-generic/8053/2
	// big mac fullness factor: 2.0
	// big mac calories: 563
	// big mac fullness units: 1126
	// fullness units to 100% full: ~2500

	// http://nutritiondata.self.com/facts/legumes-and-legume-products/4389/2
	// soy protein isolate fullness factor: 2.9
	// per gram: 2.9 /kCal * 4 kCal/g = 11.6
	// satiety per gram: 11.6 / 2500 = 0.00464
	const float proteinSatiety = 0.00464f;

	// http://nutritiondata.self.com/facts/sweets/5592/2
	// sugar fullness factor: 1.3
	// per gram: 1.3 /kCal * 4 kCal/g = 5.2
	// satiety per gram: 5.2 / 2500 = 0.00208
	const float carbSatiety = 0.00208f;

	// http://nutritiondata.self.com/facts/fats-and-oils/483/2
	// lard fullness factor: 0.5
	// per gram: 0.5 /kCal * 9 kCal/g = 4.5
	// satiety per gram: 4.5 / 2500 = 0.00180
	const float fatSatiety = 0.00180f;

	// http://nutritiondata.self.com/facts/beverages/3872/2
	// cola fullness factor: 3.7
	// per gram: 3.7 /kCal * 0.41 kCal/g = 1.5
	// satiety per gram: 11.6 / 2500 = 0.00060
	const float waterSatiety = 0.00060f;

	// calculates how filling the food is
	public float satiety
	{
		get { return proteinSatiety * protein + carbSatiety * carbs + fatSatiety * fat + waterSatiety * water; }
	}

	public string foodName
	{
		get { return foodNameText; }
	}

	// calculates how many calories the food has
	public float calories
	{
		get { return protein * 4f + fat * 9f + carbs * 4f; }
	}

	public float protein
	{
		get { return proteinGrams; }
	}

	public float fat
	{
		get { return fatGrams; }
	}

	public float carbs
	{
		get { return carbsGrams; }
	}

	public float water
	{
		get { return waterGrams; }
	}
}
