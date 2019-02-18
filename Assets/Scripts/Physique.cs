using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class used for specifying action data, basically power consumption for upper and lower body for
// things like running, swinging a sword, firing a gun, etc.
// TODO: consider storing in scriptable objects?
[System.Serializable]
public class ActionData
{
	public string name;
	public float powerUpper;
	public float powerLower;

	public ActionData()
	{
		this.name = "";
		this.powerUpper = 0f;
		this.powerLower = 0f;
	}

	public ActionData(string name, float powerUpper, float powerLower)
	{
		this.name = name;
		this.powerUpper = powerUpper;
		this.powerLower = powerLower;
	}
}

// class used to store data relating to a given ambulation (running, sprinting, etc.)
// includes power output, the amount of noise generated, the name, the pose
// TODO: consider storing in scriptable objects
[System.Serializable]
public class AmbulationData
{
	[Tooltip("Ambulation identifier.")]
	public Ambulation ambulationTag;
	[Tooltip("Display name of ambulation.")]
	public string ambulationName = "Still";
	[Tooltip("Associated pose.")]
	public Pose pose;
	[Tooltip("Is the player not moving?")]
	public bool isStill = false;
	[Tooltip("Auditory detection radius.")]
	public float auditoryDetectionRadius = 20f;
	[Tooltip("Fraction of upper body aerobic muscle used.")]
	public float upperAerobicDraw = 0f;
	[Tooltip("Fraction of upper body anaerobic muscle used.")]
	public float upperAnaerobicDraw = 0f;
	[Tooltip("Fraction of lower body aerobic muscle used.")]
	public float lowerAerobicDraw = 0f;
	[Tooltip("Fraction of lower body anaerobic muscle used.")]
	public float lowerAnaerobicDraw = 0f;
	[Tooltip("Multiplier relating movement speed to power output and mass.")]
	public float powerToSpeed = 8f;

	public ActionData GetActionData(MuscleParams muscleParams)
	{
		float powerUpper = (muscleParams.upperAerobicThreshold * upperAerobicDraw +
							muscleParams.upperAnaerobicThreshold * upperAnaerobicDraw) * Physique.MUSCLE_EFFICIENCY;
		float powerLower = (muscleParams.lowerAerobicThreshold * lowerAerobicDraw + 
							muscleParams.lowerAnaerobicThreshold * lowerAnaerobicDraw) * Physique.MUSCLE_EFFICIENCY;
		return new ActionData(ambulationName, powerUpper, powerLower);
	}
}

// struct used to pass current muscle parameters to AmbulationData's GetActionData function
// (data is used for calculating power outputs)
public struct MuscleParams
{
	public float upperAerobicThreshold;
	public float upperAnaerobicThreshold;
	public float lowerAerobicThreshold;
	public float lowerAnaerobicThreshold;

	public MuscleParams(float upperAerobicThreshold, float upperAnaerobicThreshold, float lowerAerobicThreshold, float lowerAnaerobicThreshold)
	{
		this.upperAerobicThreshold = upperAerobicThreshold;
		this.upperAnaerobicThreshold = upperAnaerobicThreshold;
		this.lowerAerobicThreshold = lowerAerobicThreshold;
		this.lowerAnaerobicThreshold = lowerAnaerobicThreshold;
	}
}

// class that keeps track of a character's physique - hunger/thirst/sleep stats, run speeds, amounts of muscle.
// lots of math and research in the comments
// primarily used to calculate how fast a character moves, but will eventually be used for leveling up
// and hunger/thirst/sleep penalties/bonuses
// TODO: implement hunger/thirst/sleep penalties/bonuses
// TODO: implement muscle fiber allocation level-ups
[RequireComponent(typeof(ArmorManager))]
public class Physique : MonoBehaviour {
	[SerializeField] AmbulationData[] ambulations = null;

	public Ambulation ambulation
	{
		get
		{
			if (ambulationIndex == -1)
			{
				return Ambulation.Unset;
			}
			else
			{
				return ambulations[ambulationIndex].ambulationTag;
			}
		}
	}
	int ambulationIndex = -1;

	public delegate void OnAmbulationChanged(AmbulationData newAmbulation);
	public event OnAmbulationChanged onAmbulationChanged;

	public bool metabolismActive = false;

	// http://jeb.biologists.org/content/jexbio/108/1/377.full.pdf
	// power density of slow twitch muscle is between 430 W/kg and 860 W/kg, in birds (seems way high)
	// http://bodytransform.co/Blog/Power+output+during+exercise.html
	// average person can produce 3 W/kg for about an hour (so slow twitch)
	// average person has 7.1kg + 9.3kg = 16.4kg slow twitch muscle
	// 3 W/kg * 79.9kg / 16.4kg = 14.6 W/kg (slow twitch)
	// for balance reasons, make fast twitch more powerful
	const float POWER_PER_KG_FAST_TWITCH = 21f;
	const float POWER_PER_KG_SLOW_TWITCH = 14.6f;

	public float interval = 0.2f;

	// https://answers.yahoo.com/question/index?qid=20101118192723AAbO8el
	// 18% to 26% efficiency - make fast twitch less efficient for balance reasons
	public const float MUSCLE_EFFICIENCY = 0.25f;

	// https://fitness.stackexchange.com/questions/22260/average-muscle-breakdown-by-percentage-by-weight-of-the-total
	// weight of muscle: 18.5 kg upper, 14.3 kg lower @ 79.9 kg, 1.78 m
	// https://answers.yahoo.com/question/index?qid=20080531233320AAWwJLF
	// ratio of fast twitch to slow twitch: 50/50 (average person), or 70/30 (body builder) to 20/80 (marathon runner)
	public float height = 1.78f;
	public float fastTwitchUpper = 7.1f;
	public float fastTwitchLower = 9.3f;
	public float slowTwitchUpper = 7.1f;
	public float slowTwitchLower = 9.3f;

	// https://en.wikipedia.org/wiki/Glycogen
	// liver contains ~100g glycogen
	// muscle contains ~400g glycogen (in fast twitch)
	// 400g / (7.1kg upper + 9.3kg lower) = 0.0244 kg glycogen / kg fast twitch muscle
	public float glycogenUpper = 0.173f;        // 0.0244 * 7.1f
	public float glycogenLower = 0.227f;        // 0.0244 * 9.3f
	public float glycogenLiver = 0.1f;
	// https://en.wikipedia.org/wiki/Body_fat_percentage
	// 42.3% muscle, ~18% fat, so ~40% other (bones & organs) 
	public float massFat = 14.4f;
	public float massBase = 32f;

	const float REFERENCE_MASS = 79.9f;
	const float REFERENCE_MASS_LEAN = 65.5f;

	// https://en.wikipedia.org/wiki/Body_water
	// ~60% water by weight
	public float hydration = 47.9f;

	public float upperLactate = 0f;
	public float bloodLactate = 0f;
	public float lowerLactate = 0f;


	// https://dioxyme.com/protein-absorption/
	// protein digestion rate: 2-10 g/hour
	// body doesn't store protein beyond what's in the digestive track, except as tissue
	// so just take ~60g/day
	public float proteinDigestionRate
	{
		get { return 0.0025f * massLean / (3600f * REFERENCE_MASS_LEAN); }
	}

	// https://diyps.org/2014/05/29/determining-your-carbohydrate-absorption-rate-diyps-lessons-learned/
	// carb digestion rate: about 30 g/hour
	public float carbDigestionRate
	{
		get { return 0.030f * massLean / (3600f * REFERENCE_MASS_LEAN); }
	}

	// fat digestion rate: guesstimate about 15 g/hour
	public float fatDigestionRate
	{
		get { return 0.015f * massLean / (3600f * REFERENCE_MASS_LEAN); }
	}

	// http://sweatscience.com/how-quickly-is-water-absorbed-after-you-drink-it/
	// water digestion rate: 300mL in ~20 minutes => 900g/hour
	public float waterDigestionRate
	{
		get { return 0.900f * massLean / (3600f * REFERENCE_MASS_LEAN); }
	}

	// satiety rate: 100% (1.0) in 4 hours => 6.94e-5 /s
	public float satietyRate
	{
		get { return 0.0000694f; }
	}

	public float satiety = 0f;
	public float waterDigesting = 0f;
	public float carbDigesting = 0f;
	public float proteinDigesting = 0.020f;
	public float fatDigesting = 0f;


	List<ActionData> actions;
	SortedList<Ambulation, ActionData> ambulationActions;
	ArmorManager armorManager;

	// https://en.wikipedia.org/wiki/Lactic_acid
	// blood concentrations up to 25 mmol/liter
	// 90.08 g/mol
	// https://www.livescience.com/32213-how-much-blood-is-in-the-human-body.html
	// 5 liters of blood
	// 0.025 mol/liter * 5 liters * 0.09008 kg/mol = 0.01126 kg lactate in body @ 65.5kg lean => 0.0001719 kg/kg
	const float KG_BLOOD_LACTATE_MAX_PER_KG = 0.0001719f;
	public float bloodLactateMax
	{
		get { return massLean * KG_BLOOD_LACTATE_MAX_PER_KG; }
	}

	const float KG_LACTATE_MAX_PER_KG_MUSCLE = 0.002f;	// no numbers available, so adjusted empirically
	public float upperLactateMax
	{
		get { return fastTwitchUpper * KG_LACTATE_MAX_PER_KG_MUSCLE; }
	}
	public float lowerLactateMax
	{
		get { return fastTwitchLower * KG_LACTATE_MAX_PER_KG_MUSCLE; }
	}

	// https://www.ncbi.nlm.nih.gov/pmc/articles/PMC1270348/?page=1
	// lactate gluconeogenesis rate: 1.06 micromoles/min/gram
	// https://en.wikipedia.org/wiki/Glucose
	// glucose molar mass: 180.16 g/mol (double that of lactate)
	// https://en.wikipedia.org/wiki/Liver#Gross_anatomy
	// liver mass: 1.5kg
	// lactate conversion rate: 1.5kg * 0.180 kg/mol * 0.00106 mol/min/kg / 60 sec/min = 0.00000477 kg / sec
	// per 65.5kg lean => 0.00000007282 kg/kg/s
	// but that recovery rate is too slow for a game, plus there are other sources of lactate removal,
	// so...
	// https://www.ncbi.nlm.nih.gov/pubmed/25920410
	// 43% reduction in 10 minutes (active recovery) actually seems in line with reality?
	const float KG_BLOOD_LACTATE_PER_KG_PER_S = 0.000002f;
	public float gluconeogenesisRate
	{
		get { return massLean * KG_BLOOD_LACTATE_PER_KG_PER_S; }
	}
	const float KG_MUSCLE_LACTATE_PER_KG_PER_S = 0.00005f;
	public float upperLactateRecoveryRate
	{
		get { return fastTwitchUpper * KG_MUSCLE_LACTATE_PER_KG_PER_S; }
	}
	public float lowerLactateRecoveryRate
	{
		get { return fastTwitchLower * KG_MUSCLE_LACTATE_PER_KG_PER_S; }
	}

	// maximum transfer rate of glucose from liver to muscle, relative to max glycogen store
	// can't get a number for it, so estimate about 1% per second
	const float GLUCOSE_TRANSFER_RATE = 0.01f;

	public float massMuscle
	{
		get { return fastTwitchUpper + fastTwitchLower + slowTwitchUpper + slowTwitchLower; }
	}

	// approximately 79.9 - 14.4 = 65.5
	public float massLean
	{
		get { return massMuscle + massBase; }
	}

	public float massTotal
	{
		get { return massLean + massFat; }
	}

	public float upperAerobicThreshold
	{
		get { return slowTwitchUpper * POWER_PER_KG_SLOW_TWITCH; }
	}

	public float lowerAerobicThreshold
	{
		get { return slowTwitchLower * POWER_PER_KG_SLOW_TWITCH; }
	}

	public float upperAnaerobicThreshold
	{
		get { return fastTwitchUpper * POWER_PER_KG_FAST_TWITCH; }
	}

	public float lowerAnaerobicThreshold
	{
		get { return fastTwitchLower * POWER_PER_KG_FAST_TWITCH; }
	}

	public float upperMaxPower
	{
		get { return upperAerobicThreshold + upperAnaerobicThreshold; }
	}

	public float lowerMaxPower
	{
		get { return lowerAerobicThreshold + lowerAnaerobicThreshold; }
	}

	// https://en.wikipedia.org/wiki/Dehydration
	// Level of hydration 
	// 1.00: fully hydrated
	// 0.98: performance begins to be diminished
	// 0.80: death
	// 79.9kg total - 14.4kg fat = 65.5kg lean
	// 47.9kg water / 65.5kg lean = 0.73
	const float WATER_PER_KG_LEAN = 0.73f;
	public float hydrationFraction
	{
		get { return hydration / hydrationMax; }
	}
	public float hydrationMax
	{
		get { return massLean * WATER_PER_KG_LEAN; }
	}

	const float GLYCOGEN_PER_KG_MUSCLE = 0.0244f;
	public float upperGlycogenMax
	{
		get { return GLYCOGEN_PER_KG_MUSCLE * fastTwitchUpper; }
	}

	public float lowerGlycogenMax
	{
		get { return GLYCOGEN_PER_KG_MUSCLE * fastTwitchLower; }
	}

	// 0.1kg glycogen / 65.5kg lean = 0.00153f;
	const float GLYCOGEN_PER_KG_LIVER = 0.00153f;
	public float liverGlycogenMax
	{
		get { return GLYCOGEN_PER_KG_LIVER * massLean; }
	}

	// http://clinicalnutritionespen.com/article/S1751-4991(11)00006-0/pdf
	// ...the human body gets a little more than half of its energy from fat at rest
	// 
	// BMR is about 1600 kCal/day
	// BMR glycogen is 720 kCal/day @ 79.9kg = 0.00833 kCal/s = 34.85 W => 0.436 W/kg
	// BMR fat is 880 kCal/day @ 79.9kg = 0.0102 kCal/s = 42.68 W => 0.534 W/kg

	// 4000 kCal/kg * 4184 J/kCal = 16,736,000 J/kg
	const float KCAL_PER_KG_CARB = 4000f;
	const float JOULES_PER_KG_CARB = 16736000f;
	const float BMR_CARB_WATTS_PER_KG = 0.436f;
	public float glycogenBMR
	{
		get { return massTotal * BMR_CARB_WATTS_PER_KG / JOULES_PER_KG_CARB; }
	}

	// 9000 kCal/kg * 4184 J/kCal = 37,656,000 J/kg
	const float KCAL_PER_KG_FAT = 9000f;
	const float JOULES_PER_KG_FAT = 37656000f;
	const float BMR_FAT_WATTS_PER_KG = 0.534f;
	public float fatBMR
	{
		get { return massTotal * BMR_FAT_WATTS_PER_KG / JOULES_PER_KG_FAT; }
	}

	private void Awake()
	{
		armorManager = GetComponent<ArmorManager>();
		armorManager.onMobilityChanged += OnMobilityChanged;
		InitializeStrengthDependent();
	}

	public void OnMobilityChanged(bool isCompromised)
	{
		mobilityCompromised = isCompromised;
	}

	// are the legs broken? forces ambulation to crawling if so.
	bool mobilityCompromised
	{
		get { return _mobilityCompromised; }
		set
		{
			if (value)
			{
				if (ambulations[ambulationIndex].isStill)
				{
					TryToSetAmbulation(Ambulation.Lay);
				}
				else
				{
					TryToSetAmbulation(Ambulation.Crawl);
				}
			}
			_mobilityCompromised = value;
		}
	}
	bool _mobilityCompromised = false;

	// recalculates values that depend on the amount of muscle
	// TODO: implement muscle fiber allocation level-ups
	void InitializeStrengthDependent()
	{
		actions = new List<ActionData>();
		MuscleParams muscleParams = new MuscleParams(upperAerobicThreshold, upperAnaerobicThreshold, lowerAerobicThreshold, lowerAnaerobicThreshold);
		ambulationActions = new SortedList<Ambulation, ActionData>();
		foreach (AmbulationData ambulationData in ambulations)
		{
			ambulationActions.Add(ambulationData.ambulationTag, ambulationData.GetActionData(muscleParams));
		}
		ambulationActions.TrimExcess();
	}

	float caloriesConsumed = 0f;
	float calorieRate = 0f;
	float intervalTimer;

	// process the metabolism at regular intervals
	private void Update()
	{
		if (metabolismActive)
		{
			if (intervalTimer <= 0f)
			{
				intervalTimer = interval;

				caloriesConsumed = 0f;      // set by Process functions

				Digest(interval);

				DistributeGlycogen(interval);

				// apply actions
				ProcessBMR(interval);
				ProcessMovement(interval);
				ProcessActions(interval);

				calorieRate = caloriesConsumed / interval;
			}
			else
			{
				intervalTimer -= Time.deltaTime;
			}
		}
	}

	// digest food - transfer from "digesting" buffers and distribute it
	void Digest(float deltaTime)
	{
		float delta = Mathf.Min(satiety, satietyRate * deltaTime);
		satiety -= delta;

		delta = Mathf.Min(waterDigesting, waterDigestionRate * deltaTime);
		waterDigesting -= delta;
		hydration = Mathf.Min(hydrationMax, hydration + delta);

		delta = Mathf.Min(proteinDigesting, proteinDigestionRate * deltaTime);
		proteinDigesting -= delta;

		delta = Mathf.Min(fatDigesting, fatDigestionRate * deltaTime);
		fatDigesting -= delta;
		massFat += delta;

		delta = Mathf.Min(carbDigesting, carbDigestionRate * deltaTime);
		carbDigesting -= delta;
		float surplus = delta - (liverGlycogenMax - glycogenLiver) + (upperGlycogenMax - glycogenUpper) + (lowerGlycogenMax - glycogenLower);
		if (surplus > 0)
		{
			// there's a surplus, store excess as fat
			massFat += surplus * KCAL_PER_KG_CARB / KCAL_PER_KG_FAT;
			delta -= surplus;
		}
		if (delta > 0f)
		{
			float upperCapacity = upperGlycogenMax - glycogenUpper;
			float lowerCapacity = lowerGlycogenMax - glycogenLower;
			float liverCapacity = liverGlycogenMax - glycogenLiver;
			float totalCapacity = upperCapacity + lowerCapacity + liverCapacity;
			glycogenUpper += delta * upperCapacity / totalCapacity;
			glycogenLower += delta * lowerCapacity / totalCapacity;
			glycogenLiver += delta * liverCapacity / totalCapacity;
		}
	}

	// distribute glycogen from liver to muscle tissue, also handling gluconeogenesis
	void DistributeGlycogen(float deltaTime)
	{
		// distribute glycogen to muscle tissue
		float glucoseTransferred;
		float difference = GetLiverGlycogenFraction() - GetUpperGlycogenFraction();
		if (difference > 0f)
		{
			glucoseTransferred = difference * GLUCOSE_TRANSFER_RATE * upperGlycogenMax * deltaTime;
			glycogenUpper += glucoseTransferred;
			glycogenLiver -= glucoseTransferred;
		}

		difference = GetLiverGlycogenFraction() - GetLowerGlycogenFraction();
		if (GetLowerGlycogenFraction() < GetLiverGlycogenFraction())
		{
			glucoseTransferred = difference * GLUCOSE_TRANSFER_RATE * lowerGlycogenMax * deltaTime;
			glycogenLower += glucoseTransferred;
			glycogenLiver -= glucoseTransferred;
		}

		// calculate gluconeogenesis
		float lactateConsumed = Mathf.Min(bloodLactate, gluconeogenesisRate * deltaTime / GLUCOSE_PER_LACTATE);
		float glucoseCreated = lactateConsumed * GLUCOSE_PER_LACTATE;

		// deduct from blood lactate
		bloodLactate -= lactateConsumed;

		float upperLactateRecovered = Mathf.Clamp(upperLactateRecoveryRate * deltaTime * (1f - GetBloodLactateFraction()), 0f, upperLactate);
		float lowerLactateRecovered = Mathf.Clamp(lowerLactateRecoveryRate * deltaTime * (1f - GetBloodLactateFraction()), 0f, lowerLactate);
		upperLactate -= upperLactateRecovered;
		lowerLactate -= lowerLactateRecovered;
		bloodLactate += upperLactateRecovered + lowerLactateRecovered;

		// add to liver glycogen (surplus goes to fat stores)
		float glycogenSurplus = glucoseCreated - (liverGlycogenMax - glycogenLiver);
		if (glycogenSurplus > 0f)
		{
			glycogenLiver = liverGlycogenMax;
			massFat += glycogenSurplus * KCAL_PER_KG_CARB / KCAL_PER_KG_FAT;
		}
		else
		{
			glycogenLiver += glucoseCreated;
		}

	}

	// calculate and apply Basal Metabolic Rate (the bare minimum energy consumption to keep the body going)
	void ProcessBMR(float deltaTime)
	{
		float kgCarb = deltaTime * glycogenBMR;
		float kgFat = deltaTime * fatBMR;

		glycogenLiver = Mathf.Max(0f, glycogenLiver - kgCarb);
		massFat = Mathf.Max(0f, massFat - kgFat);

		caloriesConsumed += kgCarb * KCAL_PER_KG_CARB + kgFat * KCAL_PER_KG_FAT;
	}

	// apply the current movement calorie consumption
	void ProcessMovement(float deltaTime)
	{
		ActionData ambulationAction;
		if (ambulationActions.TryGetValue(ambulation, out ambulationAction))
		{
			ProcessAction(ambulationAction, deltaTime);
			if (ambulation == Ambulation.Sprint)
			{
				if (GetLowerLactateFraction() > 1.0f)   // don't allow sprinting if lactate is too high
				{
					TryToSetAmbulation(Ambulation.Run);
				}
			}

		}
	}

	// https://answers.yahoo.com/question/index?qid=20071216193442AADkktZ
	// 2 ATP produced per glucose during glycolisis. fermentation of pyruvate produces
	// no further ATP.
	// https://en.wikipedia.org/wiki/Cellular_respiration
	// 38 total ATP produced during complete respiration
	// means each kilogram of muscle glycogen yields 2 ATP / 38 ATP * 4000 kCal/kg * 4184 Joules/kCal = 880,842 J/kg
	// https://en.wikipedia.org/wiki/Cori_cycle
	// 6 ATP are consumed going from lactate back to glucose
	// means each kilogram of muscle glycogen yields 30 ATP / 38 ATP = 0.789 kg liver glycogen / kg muscle glycogen
	const float JOULES_PER_KG_MUSCLE_GLYCOGEN = 880842f;
	const float GLUCOSE_PER_LACTATE = 0.789f;

	// apply any currently queued actions
	// TODO: add an action when using weapons
	void ProcessActions(float deltaTime)
	{
		foreach (ActionData action in actions)
		{
			ProcessAction(action, deltaTime);
		}
	}

	// calculate and apply the calorie consumption from an action
	void ProcessAction(ActionData action, float deltaTime)
	{
		// calculate fast and slow twitch power consumption
		float powerAerobicUpper = Mathf.Min(action.powerUpper / MUSCLE_EFFICIENCY, upperAerobicThreshold);
		float powerAnaerobicUpper = Mathf.Clamp(action.powerUpper / MUSCLE_EFFICIENCY - powerAerobicUpper, 0f, upperAnaerobicThreshold);
		float powerAerobicLower = Mathf.Min(action.powerLower / MUSCLE_EFFICIENCY, lowerAerobicThreshold);
		float powerAnaerobicLower = Mathf.Clamp(action.powerLower / MUSCLE_EFFICIENCY - powerAerobicLower, 0f, lowerAnaerobicThreshold);

		// calculate fat and glycogen cost/production
		float fatCost = (powerAerobicUpper + powerAerobicLower) / JOULES_PER_KG_FAT;
		float upperGlycogenCost = powerAnaerobicUpper / JOULES_PER_KG_MUSCLE_GLYCOGEN;
		float lowerGlycogenCost = powerAnaerobicLower / JOULES_PER_KG_MUSCLE_GLYCOGEN;
		float liverGlycogenProduced = (upperGlycogenCost + lowerGlycogenCost) * GLUCOSE_PER_LACTATE;

		// adjust fat and glycogen levels
		massFat = Mathf.Max(0f, massFat - fatCost);
		glycogenUpper = Mathf.Max(0f, glycogenUpper - upperGlycogenCost);
		glycogenLower = Mathf.Max(0f, glycogenLower - lowerGlycogenCost);
		upperLactate += upperGlycogenCost;
		lowerLactate += lowerGlycogenCost;

		// calculate consumed calories
		caloriesConsumed += fatCost * KCAL_PER_KG_FAT + (upperGlycogenCost + lowerGlycogenCost - liverGlycogenProduced) * KCAL_PER_KG_CARB;
	}

	// called by MovementController to set the current ambulation.
	// Fails if there is insufficient stamina or compromised mobility, or if the character
	// doesn't have the specified ambulation
	public void TryToSetAmbulation(Ambulation attemptedAmbulation)
	{
		int newAmbulationIndex = GetAmbulationIndex(attemptedAmbulation);
		if (newAmbulationIndex != -1 && newAmbulationIndex != ambulationIndex)
		{
			if (mobilityCompromised && !(attemptedAmbulation == Ambulation.Crawl || attemptedAmbulation == Ambulation.Lay))
			{
				if (ambulations[newAmbulationIndex].isStill)
				{
					newAmbulationIndex = GetAmbulationIndex(Ambulation.Lay);
					attemptedAmbulation = Ambulation.Lay;
				}
				else
				{
					newAmbulationIndex = GetAmbulationIndex(Ambulation.Crawl);
					attemptedAmbulation = Ambulation.Crawl;
				}
			}
			if (attemptedAmbulation == Ambulation.Sprint)
			{
				if (GetLowerLactateFraction() > 1.0f)   // don't allow sprinting if lactate is too high
				{
					newAmbulationIndex = GetAmbulationIndex(Ambulation.Run);
					attemptedAmbulation = Ambulation.Run;
				}
			}
			if (newAmbulationIndex != ambulationIndex)
			{
				ambulationIndex = newAmbulationIndex;
				if (onAmbulationChanged != null)
				{
					onAmbulationChanged(ambulations[ambulationIndex]);
				}
			}
		}
	}

	// finds the index of the specified ambulation, if present
	int GetAmbulationIndex(Ambulation ambulation)
	{
		for (int i = 0; i < ambulations.Length; i++)
		{
			if (ambulations[i].ambulationTag == ambulation)
			{
				return i;
			}
		}
		return -1;
	}

	// calculates the speed of the current ambulation
	// TODO: pass in inventory weight to make fuller inventories slow the player
	public float GetAmbulationSpeed()
	{
		ActionData ambulationAction;
		if (ambulationActions.TryGetValue(ambulation, out ambulationAction))
		{
			return ambulations[ambulationIndex].powerToSpeed * (ambulationAction.powerUpper + ambulationAction.powerLower) / massTotal;
		}
		return 0f;
	}

	// ingest and ingestible, if the player isn't too full
	// returns false if the character was too full to eat it
	public bool Consume(Ingestible ingestible)
	{
		float satiety = AdjustSatiety(ingestible.satiety);
		if (satiety < 1f - this.satiety)
		{
			this.satiety += satiety;
			proteinDigesting += ingestible.protein / 1000f;
			carbDigesting += ingestible.carbs / 1000f;
			fatDigesting += ingestible.fat / 1000f;
			waterDigesting += ingestible.water / 1000f;
			return true;
		}
		else
		{
			return false;
		}
	}

	// scales calculated food satiety to the player's satiety scale
	public float AdjustSatiety(float satiety)
	{
		return satiety * REFERENCE_MASS_LEAN / massLean;
	}

	// getters for UI display
	public float GetCalorieRate()
	{
		return calorieRate;
	}

	public float GetUpperGlycogenFraction()
	{
		return glycogenUpper / upperGlycogenMax;
	}

	public float GetLowerGlycogenFraction()
	{
		return glycogenLower / lowerGlycogenMax;
	}

	public float GetLiverGlycogenFraction()
	{
		return glycogenLiver / liverGlycogenMax;
	}

	public float GetHydrationFraction()
	{
		return (hydrationFraction - 0.8f) / 0.2f;
	}

	public float GetBloodLactateFraction()
	{
		return bloodLactate / bloodLactateMax;
	}

	public float GetUpperLactateFraction()
	{
		return upperLactate / upperLactateMax;
	}

	public float GetLowerLactateFraction()
	{
		return lowerLactate / lowerLactateMax;
	}

	public float GetProteinFraction()
	{
		return Mathf.Clamp(proteinDigesting / (proteinDigestionRate * 3600f * 8f), 0f, 1f);
	}
}
