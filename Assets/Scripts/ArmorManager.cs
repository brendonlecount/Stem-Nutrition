using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used for specifying humanoid body parts (used by skeleton and armor components)
public enum BodyPart { Head, UpperTorso, LowerTorso, LeftUpperArm, RightUpperArm, LeftLowerArm, RightLowerArm, LeftHand, RightHand, LeftThigh, RightThigh, LeftCalf, RightCalf, LeftFoot, RightFoot, None }

// used for retrieving lists of condition components
public enum ComponentCategory { Armor, Bone, Organ }

// used for categorizing skeletal damage penalties
public enum LimbCategory { Mobility, LeftArm, RightArm, None }

// used by the UI for generating lists of components and their health
public struct ConditionComponentName
{
	public string componentName;
	public float conditionFraction;

	public ConditionComponentName(string componentName, float conditionFraction)
	{
		this.componentName = componentName;
		this.conditionFraction = conditionFraction;
	}
}

// Keeps track of armor, skeleton and organ health (TODO: rename to ConditionManager or HealthManager)
// Attach to top level parent of skeleton and mesh (looks for a Skeleton component and ArmorComponentHumanoid subcomponents)
public class ArmorManager : MonoBehaviour {
	InventoryEntry armorEntry;		// currently equipped armor, as an inventory entry (keeps track of name and count)
	Armor armor;					// currently equipped armor
	// skeleton, armor, and organ ConditionComponents
	Skeleton skeleton;
	ArmorComponentHumanoid[] armorComponents;
	Organ[] organs;

	// total mass of equipped armor
	public float equippedMass
	{
		get { return _equippedMass; }
	}
	float _equippedMass;

	// maximum possible volume of armor components
	public float armorMaxVolume
	{
		get { return _armorMaxVolume; }
	}
	float _armorMaxVolume = 0f;

	// are the leg bones broken?
	public bool isMobilityCompromised
	{
		get { return isMobilityCompromised; }
		protected set
		{
			if (value != _isMobilityCompromised)
			{
				_isMobilityCompromised = value;
				if (onMobilityChanged != null)
				{
					// send out event (listened to by Physique)
					onMobilityChanged(_isMobilityCompromised);
				}
			}
		}
	}
	bool _isMobilityCompromised = false;
	List<BodyPart> compromisedMobilityParts;

	// is the left arm broken?
	public bool isLeftArmCompromised
	{
		get { return _isLeftArmCompromised; }
		protected set
		{
			if (value != _isLeftArmCompromised)
			{
				_isLeftArmCompromised = value;
				if (onLeftArmChanged != null)
				{
					// send out event (listened to by WeaponManager)
					onLeftArmChanged(_isLeftArmCompromised);
				}
			}
		}
	}
	bool _isLeftArmCompromised = false;
	List<BodyPart> compromisedLeftArmParts;

	// is the right arm broken?
	public bool isRightArmCompromised
	{
		get { return _isRightArmCompromised; }
		set
		{
			if (value != _isRightArmCompromised)
			{
				_isRightArmCompromised = value;
				if (onRightArmChanged != null)
				{
					// send out event (listened to by WeaponManager)
					onRightArmChanged(_isRightArmCompromised);
				}
			}
		}
	}
	bool _isRightArmCompromised = false;
	List<BodyPart> compromisedRightArmParts;

	// has an organ been destroyed?
	public bool isDead
	{
		get { return _isDead; }
		protected set
		{
			if (value != _isDead)
			{
				_isDead = value;
				if (onOrganChange != null)
				{
					// send out event (listened to by InputSource)
					onOrganChange(_isDead);
				}
			}
		}
	}
	bool _isDead = false;
	List<string> deadOrgans;

	public delegate void OnOrganChange(bool isDead);
	public event OnOrganChange onOrganChange;

	public delegate void OnMobilityChanged(bool isCompromised);
	public event OnMobilityChanged onMobilityChanged;

	public delegate void OnLeftArmChanged(bool isCompromised);
	public event OnLeftArmChanged onLeftArmChanged;

	public delegate void OnRightArmChanged(bool isCompromised);
	public event OnRightArmChanged onRightArmChanged;

	// Use this for initialization
	void Awake() {
		// initialize armor InventoryEntry to none
		ClearArmor();

		// get ahold of the skeleton and register for bone break events
		skeleton = GetComponentInChildren<Skeleton>();
		compromisedMobilityParts = new List<BodyPart>();
		compromisedLeftArmParts = new List<BodyPart>();
		compromisedRightArmParts = new List<BodyPart>();
		skeleton.onBoneBreakChange += OnBoneBreakChange;

		// get ahold of organs and register for organ damage events
		deadOrgans = new List<string>();
		organs = GetComponentsInChildren<Organ>();
		foreach (Organ organ in organs)
		{
			organ.onDeathChange += OnOrganDeathChange;
		}

		// get ahold of armor components and initialize them
		// also create bones for Skeleton to manage
		armorComponents = GetComponentsInChildren<ArmorComponentHumanoid>();
		foreach (ArmorComponent armorComponent in armorComponents)
		{
			armorComponent.InitializeComponent();
			_armorMaxVolume += armorComponent.maxVolume;
			if (skeleton != null)
			{
				skeleton.AddBone(armorComponent.bodyPart);
			}
		}
		// finalize skeleton's sorted list of bones
		if (skeleton != null)
		{
			skeleton.FinalizeBones();
		}

		// enable all mesh renderers (opaque armor renderers were disabled in the editor because it looks cooler)
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in renderers)
		{
			renderer.enabled = true;
		}
	}

	// callback function for organ damage events (sets and clears isDead bool, triggering events)
	public void OnOrganDeathChange(Organ organ, bool isDead)
	{
		if (isDead)
		{
			deadOrgans.Add(organ.componentName);
			this.isDead = true;
		}
		else
		{
			deadOrgans.Remove(organ.componentName);
			if (deadOrgans.Count == 0)
			{
				this.isDead = false;
			}
		}
	}

	// callback function for bone break events (sets and clears isCompromised bools, triggering events)
	public void OnBoneBreakChange(BodyPart bonePart, bool isBroken)
	{
		switch (GetLimbCategory(bonePart))
		{
			case LimbCategory.Mobility:
				if (isBroken)
				{
					compromisedMobilityParts.Add(bonePart);
					isMobilityCompromised = true;
				}
				else
				{
					compromisedMobilityParts.Remove(bonePart);
					if (compromisedMobilityParts.Count == 0)
					{
						isMobilityCompromised = false;
					}
				}
				break;
			case LimbCategory.LeftArm:
				if (isBroken)
				{
					compromisedLeftArmParts.Add(bonePart);
					isLeftArmCompromised = true;
				}
				else
				{
					compromisedLeftArmParts.Remove(bonePart);
					if (compromisedLeftArmParts.Count == 0)
					{
						isLeftArmCompromised = false;
					}
				}
				break;
			case LimbCategory.RightArm:
				if (isBroken)
				{
					compromisedRightArmParts.Add(bonePart);
					isRightArmCompromised = true;
				}
				else
				{
					compromisedRightArmParts.Remove(bonePart);
					if (compromisedRightArmParts.Count == 0)
					{
						isRightArmCompromised = false;
					}
				}
				break;
		}
	}

	public InventoryEntry GetEquippedArmor()
	{
		return armorEntry;
	}
	
	// sets the armor on each armor subcomponent and specifies the amount to distribute
	// returns leftover armor (all of it if an armor was already equipped)
	// TODO: redistribute armor if it's a match for the already equipped armor
	public InventoryEntry InstallArmor(InventoryEntry armorEntry)
	{
		if (this.armorEntry.inventoryID == -1)
		{
			Debug.Log("No armor equipped :)");
			armor = armorEntry.item.GetComponent<Armor>();
			if (armor == null)
			{
				return armorEntry;
			}
			else
			{
				float volumeApplied = Mathf.Min(armorMaxVolume, armorEntry.count * Armor.UNIT_VOLUME);
				armorEntry.count = (int)(Mathf.Max(0f, armorEntry.count * Armor.UNIT_VOLUME - volumeApplied) / Armor.UNIT_VOLUME);
				foreach (ArmorComponentHumanoid armorComponent in armorComponents)
				{
					armorComponent.DistributeArmor(armor, volumeApplied * armorComponent.maxVolume / armorMaxVolume);
				}
				this.armorEntry.activatorName = armorEntry.activatorName;
				this.armorEntry.inventoryID = armorEntry.inventoryID;
				this.armorEntry.item = armorEntry.item;
				this.armorEntry.count = (int)(volumeApplied / Armor.UNIT_VOLUME);
				_equippedMass = this.armorEntry.count * armor.mass;
				return armorEntry;
			}
		}
		else if (this.armorEntry.inventoryID == armorEntry.inventoryID)
		{
			Debug.Log("Specified armor already equipped :(");
			return armorEntry;
		}
		else
		{
			Debug.Log("Different armor already equipped :(");
			return armorEntry;
		}
	}

	// unequips armor, returning an InventoryEntry representing the unequipped armor
	public InventoryEntry UninstallArmor()
	{
		if (armorEntry.inventoryID == -1)
		{
			return armorEntry;
		}
		else
		{
			float volume = 0f;
			foreach (ArmorComponentHumanoid armorComponent in armorComponents)
			{
				volume += armorComponent.RecoverArmor();
			}
			InventoryEntry removedEntry = armorEntry;
			removedEntry.count = (int)(volume / Armor.UNIT_VOLUME);
			ClearArmor();
			_equippedMass = 0f;
			return removedEntry;
		}
	}

	// initializes the armor InventoryEntry to none
	void ClearArmor()
	{
		armorEntry.activatorName = "No Armor Equipped";
		armorEntry.inventoryID = -1;
		armorEntry.item = null;
		armorEntry.count = 0;
		armor = null;
	}

	// returns a list of condition components (armor, skeleton, or organ) for use by the UI
	public List<ConditionComponentName> GetComponentCategory(ComponentCategory componentCategory)
	{
		switch (componentCategory)
		{
			case ComponentCategory.Armor:
				List<ConditionComponentName> armorList = new List<ConditionComponentName>();
				foreach (ArmorComponent armorComponent in armorComponents)
				{
					armorList.Add(new ConditionComponentName(armorComponent.componentName, armorComponent.GetConditionFraction()));
				}
				return armorList;
			case ComponentCategory.Bone:
				return skeleton.GetBoneList();
			case ComponentCategory.Organ:
				List<ConditionComponentName> organList = new List<ConditionComponentName>();
				foreach (Organ organ in organs)
				{
					organList.Add(new ConditionComponentName(organ.componentName, organ.GetConditionFraction()));
				}
				return organList;
			default:
				List<ConditionComponentName> emptyList = new List<ConditionComponentName>();
				return emptyList;
		}
	}

	// repair all damage to bones and organs
	public void Heal()
	{
		HealOrgans();
		HealBone();
	}

	public void HealOrgans()
	{
		foreach (Organ organ in organs)
		{
			organ.Heal();
		}
	}

	public void HealBone()
	{
		skeleton.Heal();
	}

	// determine bone name from body part
	public static string GetBoneName(BodyPart bodyPart)
	{
		switch (bodyPart)
		{
			case BodyPart.Head:
				return "Skull";
			case BodyPart.LeftCalf:
				return "Left Tibia";
			case BodyPart.LeftFoot:
				return "Left Metatarsals";
			case BodyPart.LeftHand:
				return "Left Metacarpals";
			case BodyPart.LeftLowerArm:
				return "Left Ulna";
			case BodyPart.LeftThigh:
				return "Left Femur";
			case BodyPart.LeftUpperArm:
				return "Left Humerus";
			case BodyPart.LowerTorso:
				return "Pelvic Bone";
			case BodyPart.RightCalf:
				return "Right Tibia";
			case BodyPart.RightFoot:
				return "Right Metatarsals";
			case BodyPart.RightHand:
				return "Right Metacarpals";
			case BodyPart.RightLowerArm:
				return "Right Ulna";
			case BodyPart.RightThigh:
				return "Right Femur";
			case BodyPart.RightUpperArm:
				return "Right Humerus";
			case BodyPart.UpperTorso:
				return "Ribcage";
			default:
				return "Bone";
		}
	}

	// determine armor component name from body part
	public static string GetArmorComponentName(BodyPart bodyPart)
	{
		switch (bodyPart)
		{
			case BodyPart.Head:
				return "Helmet";
			case BodyPart.LeftCalf:
				return "Left Shin Guard";
			case BodyPart.LeftFoot:
				return "Left Boot";
			case BodyPart.LeftHand:
				return "Left Glove";
			case BodyPart.LeftLowerArm:
				return "Left Forearm Guard";
			case BodyPart.LeftThigh:
				return "Left Thigh Guard";
			case BodyPart.LeftUpperArm:
				return "Left Arm Guard";
			case BodyPart.LowerTorso:
				return "Pelvis Guard";
			case BodyPart.RightCalf:
				return "Right Shin Guard";
			case BodyPart.RightFoot:
				return "Right Boot";
			case BodyPart.RightHand:
				return "Right Glove";
			case BodyPart.RightLowerArm:
				return "Right Forearm Guard";
			case BodyPart.RightThigh:
				return "Right Thigh Guard";
			case BodyPart.RightUpperArm:
				return "Right Arm Guard";
			case BodyPart.UpperTorso:
				return "Breastplate";
			default:
				return "Armor";
		}
	}

	// determine limb category from bodypart
	public LimbCategory GetLimbCategory(BodyPart bodyPart)
	{
		switch (bodyPart)
		{
			case BodyPart.LeftCalf:
			case BodyPart.LeftFoot:
			case BodyPart.LeftThigh:
			case BodyPart.RightCalf:
			case BodyPart.RightFoot:
			case BodyPart.RightThigh:
			case BodyPart.LowerTorso:
				return LimbCategory.Mobility;
			case BodyPart.LeftHand:
			case BodyPart.LeftLowerArm:
			case BodyPart.LeftUpperArm:
				return LimbCategory.LeftArm;
			case BodyPart.RightHand:
			case BodyPart.RightLowerArm:
			case BodyPart.RightUpperArm:
				return LimbCategory.RightArm;
			default:
				return LimbCategory.None;
		}
	}
}
