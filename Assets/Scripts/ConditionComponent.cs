using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for things that can be damaged, like armor, bones, organs
// consists of a name, bodypart (sometimes unused), whether or not individual polygons healths are tracked,
// and functions for applying damage and reporting current condition
public abstract class ConditionComponent : MonoBehaviour {
	public virtual string componentName
	{
		get { return "Component"; }
	}

	public virtual BodyPart bodyPart
	{
		get { return BodyPart.None; }
	}

	public virtual bool hasCells
	{
		get { return false; }
	}


	public abstract float DamageCondition(float Energy, float area, BodyPart targetedPart, int cellIndex);
	public abstract float GetConditionFraction();
	public abstract float GetCellConditionFraction(int cellIndex);
}
