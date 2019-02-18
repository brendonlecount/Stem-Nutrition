using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// categories of InventoryItems, used when generating inventoryIDs
public enum InventoryCategory { Equipment, Ammunition, Ingestible, Weapon, Armor, Booster }

// generates a list of items deriving from InventoryItem, a dictionary associating inventoryIDs
// with prefabs, so that items can be deleted when moved to an inventory, and reinstantiated
// from inventories when needed
// note: uses the singleton pattern, so classes can access it via static methods
// note: InventoryItems must be placed in their associated Resources subfolders
public class InventoryLookup : MonoBehaviour {
	private static InventoryLookup Instance { get; set; }

	private SortedList<int, GameObject> entries;

	private void Awake()
	{
		ApplySingletonPattern();
		PopulateEntriesList();
	}

	private void ApplySingletonPattern()
	{
		if (Instance != null && Instance != this)
		{
			GameObject.Destroy(Instance.gameObject);
		}

		Instance = this;
	}

	// populate the dictionary of inventoryIDs and prefabs from what is found in the Resources folder
	// uses a sorted list rather than a sorted dictionary because there are no adds or removes after creation
	private void PopulateEntriesList()
	{
		GameObject[] items = Resources.LoadAll<GameObject>("Inventory Items");

		entries = new SortedList<int, GameObject>();

		for (int i = 0; i < items.Length; i++)
		{
			InventoryItem item = items[i].GetComponent<InventoryItem>();
			if (item != null)
			{
				//				Debug.Log("Inventory Item " + i + ": " + item.inventoryID + " " + item.activatorName);
				entries.Add(item.inventoryID, items[i]);
			}
		}

		entries.TrimExcess();
	}

	// looks up a prefab from an inventoryID
	public static GameObject GetInventoryObject(int inventoryID)
	{
		GameObject go;
		if (Instance.entries.TryGetValue(inventoryID, out go))
		{
			return go;
		}
		Debug.Log("InventoryID " + inventoryID + " not found!");
		return null;
	}

	// looks up a prefab from an inventoryID, casting it to an InventoryItem
	public static InventoryItem GetInventoryItem(int inventoryID)
	{
		GameObject go = GetInventoryObject(inventoryID);
		if (go != null)
		{
			return go.GetComponent<InventoryItem>();
		}
		return null;
	}

	// The first digit of inventoryIDs is a prefix determined by the category
	public static int GetCategoryPrefix(InventoryCategory category)
	{
		switch (category)
		{
			case InventoryCategory.Equipment:
				return 0;
			case InventoryCategory.Ammunition:
				return 1;
			case InventoryCategory.Ingestible:
				return 2;
			case InventoryCategory.Weapon:
				return 3;
			case InventoryCategory.Armor:
				return 4;
			case InventoryCategory.Booster:
				return 5;
			default:
				return 0;
		}
	}

	// determines a category from an inventoryID (looks at the most significant digits)
	public static InventoryCategory GetInventoryCategory(int inventoryID)
	{
		switch (inventoryID / GetCategoryPrefixMult())
		{
			case 0:
				return InventoryCategory.Equipment;
			case 1:
				return InventoryCategory.Ammunition;
			case 2:
				return InventoryCategory.Ingestible;
			case 3:
				return InventoryCategory.Weapon;
			case 4:
				return InventoryCategory.Armor;
			case 5:
				return InventoryCategory.Booster;
			default:
				return InventoryCategory.Equipment;
		}
	}

	// specifies which digits are used for the category prefix
	// note: at 10000, it allows 10000 objectIDs for each category
	public static int GetCategoryPrefixMult()
	{
		return 10000;
	}
}
