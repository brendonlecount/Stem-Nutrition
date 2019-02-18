using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Editor script to automatically assign unique, sequential object IDs to inventory items
public class IDAssigner : ScriptableWizard {
	[MenuItem ("My Tools/Assign Inventory IDs")]
	static void AssignIDs()
	{
		ScriptableWizard.DisplayWizard<IDAssigner>("Assign Inventory IDs");
	}

	private void OnWizardCreate()
	{
		int iAmmunition = 0;
		int iArmor = 0;
		int iEquipment = 0;
		int iIngestible = 0;
		int iWeapon = 0;
		int iBooster = 0;

		GameObject[] items = Resources.LoadAll<GameObject>("Inventory Items");

		foreach (GameObject prefab in items)
		{
			InventoryItem item = prefab.GetComponent<InventoryItem>();
			if (item != null)
			{
				switch (item.category)
				{
					case InventoryCategory.Ammunition:
						item.objectID = iAmmunition++;
						break;
					case InventoryCategory.Armor:
						item.objectID = iArmor++;
						break;
					case InventoryCategory.Equipment:
						item.objectID = iEquipment++;
						break;
					case InventoryCategory.Ingestible:
						item.objectID = iIngestible++;
						break;
					case InventoryCategory.Weapon:
						item.objectID = iWeapon++;
						break;
					case InventoryCategory.Booster:
						item.objectID = iBooster++;
						break;
				}
				EditorUtility.SetDirty(prefab);
			}
		}
		AssetDatabase.SaveAssets();
	}

	private void OnWizardUpdate()
	{
		helpString = "Assigns sequential, unique object IDs to all inventory items.";
	}
}
