using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ammunition for weapons, defines either a projectile or beam that the weapon will fire
public class Ammunition : InventoryItem {
	public GameObject projectile;
	public int projectileCount;
	public int count;

	public Projectile GetProjectile()
	{
		return projectile.GetComponent<Projectile>();
	}

	public Beam GetBeam()
	{
		return projectile.GetComponent<Beam>();
	}

	public override InventoryCategory category
	{
		get { return InventoryCategory.Ammunition; }
	}

	public override string type
	{
		get { return "Ammunition"; }
	}
}
