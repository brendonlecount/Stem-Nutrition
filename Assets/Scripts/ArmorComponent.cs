using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for armor that composed of subcomponents, like a breastplate or helmet
// Should be attached to a gameobject with a collider. TODO: make abstract
public class ArmorComponent : ConditionComponent {
	// how thick can the armor get, in meters
	public float maxThickness = 0.01f;

	// is it possible to punch holes in the armor?
	public override bool hasCells
	{
		get
		{
			return hasArmorCells || isDynamic;
		}
	}
	public bool hasArmorCells = true;

	// maximum armor volume (maxThickness * surface area)
	public float maxVolume
	{
		get { return _maxVolume; }
	}
	float _maxVolume;

	protected float ratingM3;	// armor rating per cubic meter
	protected bool isDynamic;	// does the armor fill in holes?

	protected float[] cellVolumes;
	protected int cellCount;
	protected float cellMaxCondition;
	protected float cellRegen;
	protected float maxCondition;
	protected float cellArea;

	// Use this for initialization
	public virtual void InitializeComponent()
	{
		float area = GetArea();
		_maxVolume = area * maxThickness;
	}

	// create armor cells and apply armor
	// equipped armor stats must be specified first (handled by derived classes)
	protected void InitializeArmor(float conditionFraction)
	{
		if (!hasCells)
		{
			cellCount = 1;
		}
		else
		{
			MeshCollider mesh = GetComponent<MeshCollider>();
			if (mesh == null)
			{
				cellCount = 1;
			}
			else
			{
				cellCount = mesh.sharedMesh.triangles.Length / 3;
			}
		}
		cellVolumes = new float[cellCount];
		for (int i = 0; i < cellCount; i++)
		{
			cellVolumes[i] = maxVolume * conditionFraction / cellCount;
		}
		maxCondition = maxVolume * ratingM3;
		cellMaxCondition = maxCondition / cellCount;
		cellArea = maxVolume / cellCount;
	}

	// condition of the armor
	public float GetCondition()
	{
		float condition = 0f;
		if (cellVolumes != null)
		{
			for (int i = 0; i < cellCount; i++)
			{
				condition += cellVolumes[i];
			}
		}
		return condition * ratingM3;
	}

	// maximum condition of the armor
	public float GetMaxCondition()
	{
		return maxCondition;
	}

	// condition / maxCondition
	public override float GetConditionFraction()
	{
		if (maxCondition == 0f)
		{
			return 0f;
		}
		else
		{
			return GetCondition() / maxCondition;
		}
	}

	// condition fraction of an individual armor cell
	public override float GetCellConditionFraction(int cellIndex)
	{
		return GetCellCondition(cellIndex) / cellMaxCondition;
	}

	// condition of an individual armor cell (corresponds to a mesh collider polygon)
	public float GetCellCondition(int cellIndex)
	{
		if (cellIndex < 0 || cellIndex >= cellCount)
		{
			cellIndex = 0;
		}
		if (cellVolumes == null)
		{
			return 0;
		}
		else
		{
			return cellVolumes[cellIndex] * ratingM3;
		}
	}

	// used to apply damage to this armor component, usually called by a beam or projectile
	public override float DamageCondition(float energy, float area, BodyPart bodyPart, int cellIndex)
	{
		if (cellIndex < 0 || cellIndex >= cellCount)
		{
			cellIndex = 0;
		}

		if (cellVolumes == null)
		{
			return energy;
		}
		// projectile area shouldn't exceed cell area, or weird things will happen
		area = Mathf.Min(area, cellArea);
		// damage shouldn't be negative, or returned energy will be off (sorry Mercy)
		energy = Mathf.Max(0f, energy);

		float damage = Mathf.Min(energy, cellVolumes[cellIndex] * ratingM3 * area / cellArea);
		cellVolumes[cellIndex] -= damage / ratingM3;
		return energy - damage;
	}

	// calculates the surface area of the collider, used to determine maxCondition and cell area
	protected float GetArea()
	{
		MeshCollider mesh = GetComponent<MeshCollider>();
		if (mesh == null)
		{
			Vector3 bounds = GetComponent<Collider>().bounds.size;
			return 2f * bounds.x * bounds.y + 2f * bounds.x * bounds.z + 2f * bounds.y * bounds.z;
		}
		else
		{
			// code courtesy of il_fantasticatore:
			// https://forum.unity.com/threads/figuring-out-the-volume-and-surface-area.117388/
			Vector3[] vertices = mesh.sharedMesh.vertices;
			int[] triangles = mesh.sharedMesh.triangles;

			float result = 0f;
			for (int p = 0; p < triangles.Length; p += 3)
			{
				result += (Vector3.Cross(vertices[triangles[p + 1]] - vertices[triangles[p]], vertices[triangles[p + 2]] - vertices[triangles[p]])).magnitude;
			}
			result *= 0.5f;
			float scaleMult = transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z;
			result *= scaleMult;
			return result;
		}
	}
}
