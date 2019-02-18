using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// data for individual bones, created by ArmorManager from registered bodyparts
[System.Serializable]
public class SkeletonBone
{
	public string name;
	public float maxCondition;

	public float condition
	{
		get { return _condition; }
		set
		{
			_condition = Mathf.Clamp(value, 0f, maxCondition);
		}
	}
	float _condition;

	public bool broken
	{
		get { return _condition <= 0f; }
	}

	public SkeletonBone(string name, float maxCondition)
	{
		this.name = name;
		this.maxCondition = maxCondition;
		this.condition = maxCondition;
	}
}

// condition component that manages a skeleton of SkeletonBones
// when damaged, takes the bodyPart passed by the damager and applies the damage to the associated bone
// TODO: implement a proper skeleton model composed of individual bones (would make this class and passing bodyPart obsolete)
public class Skeleton : ConditionComponent {
	public float boneHealth;

	SortedList<BodyPart, SkeletonBone> bones;

	public override string componentName
	{
		get { return "Skeleton"; }
	}

	public delegate void OnBoneBreakChange(BodyPart bonePart, bool isBroken);
	public event OnBoneBreakChange onBoneBreakChange;

	// damage applied to the skeleton from beams, projectiles, and eventually melee
	public override float DamageCondition(float energy, float area, BodyPart bodyPart, int cellIndex)
	{
		// find the bone associated with the specified body part, and damage it
		SkeletonBone hitBone;
		if (bones.TryGetValue(bodyPart, out hitBone))
		{
			Debug.Log(hitBone.name + " hit!");
			float damage = Mathf.Min(energy, hitBone.condition);
			bool boneAlreadyBroken = hitBone.broken;
			hitBone.condition -= damage;
			if (hitBone.broken && !boneAlreadyBroken && onBoneBreakChange != null)
			{
				// send an event if the bone was broken
				onBoneBreakChange(bodyPart, true);
			}
			return energy - damage;
		}
		Debug.Log("Hit skeleton with code: " + bodyPart);
		return energy;
	}

	public override float GetConditionFraction()
	{
		return 1f;
	}

	public override float GetCellConditionFraction(int cellIndex)
	{
		return 1f;
	}

	// add a bone to the skeleton (called by ArmorManager for each registered bodyPart)
	public void AddBone(BodyPart bodyPart)
	{
		if (bones == null)
		{
			bones = new SortedList<BodyPart, SkeletonBone>();
		}

		bones.Add(bodyPart, new SkeletonBone(ArmorManager.GetBoneName(bodyPart), boneHealth));
	}

	// get a list of bones and their healths for UI display purposes
	public List<ConditionComponentName> GetBoneList()
	{
		List<ConditionComponentName> boneList = new List<ConditionComponentName>();
		foreach(KeyValuePair<BodyPart, SkeletonBone> bone in bones)
		{
			boneList.Add(new ConditionComponentName(bone.Value.name, bone.Value.condition / bone.Value.maxCondition));
		}
		return boneList;
	}

	public void FinalizeBones()
	{
		bones.TrimExcess();
	}

	// restore all bones to ful health, sending an event if a bone was broken and healed
	public void Heal()
	{
		foreach(KeyValuePair<BodyPart, SkeletonBone> bone in bones)
		{
			bool boneWasBroken = bone.Value.broken;
			bone.Value.condition = bone.Value.maxCondition;
			if (boneWasBroken && onBoneBreakChange != null)
			{
				onBoneBreakChange(bone.Key, false);
			}
		}
	}

	// is the specified bone broken?
	public bool IsBroken(BodyPart bodyPart)
	{
		SkeletonBone bone;
		if (bones.TryGetValue(bodyPart, out bone))
		{
			return bone.broken;
		}
		return false;
	}
}
