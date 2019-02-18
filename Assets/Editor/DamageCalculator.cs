using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Editor script to calculate projectile damage and weapon DPS, writing the values
// to their "damage" and "dps" properties. Calculated values are not used in-game, 
// but make it easier to balance weapon damage.
public class DamageCalculator : ScriptableWizard {
	[MenuItem("My Tools/Calculate Damage")]
	static void AssignIDs()
	{
		ScriptableWizard.DisplayWizard<DamageCalculator>("Calculate Damage");
	}

	private void OnWizardCreate()
	{
		List<Object> items = new List<Object>();
		foreach (string s in AssetDatabase.FindAssets("", new string[] { "Assets/Prefabs/Projectiles" }))
		{

			items.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(Object)));
        }

		Debug.Log(items.Count + " items loaded.");
		foreach (Object o in items)
		{
			GameObject go = o as GameObject;
			if (go != null)
			{
				Projectile projectile = go.GetComponent<Projectile>();
				if (projectile != null)
				{
					projectile.damage = 0.5f * (projectile.grain * Projectile.GRAIN2KG) * (projectile.fps * Projectile.FPS2MPS) * (projectile.fps * Projectile.FPS2MPS);
					EditorUtility.SetDirty(go);
				}
			}
		}
		AssetDatabase.SaveAssets();
	}

	private void OnWizardUpdate()
	{
		helpString = "Calculates projectile damage and weapon DPS.";
	}
}