using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores an InventoryItem prefab and a count, used to specify a starting inventory
[System.Serializable]
public class InventoryCount
{
	public GameObject item;
	public int count;

	public InventoryCount(GameObject item, int count)
	{
		this.item = item;
		this.count = count;
	}
}

// form that items take when stored in inventory, consisting of a name string,
// inventoryID, gameobject to instantiate, and count
// can be created by explicitly specifying each piece of member data, or from an InventoryCount
public struct InventoryEntry
{
	public string activatorName;
	public int inventoryID;
	public GameObject item;
	public int count;

	public InventoryEntry(string activatorName, int inventoryID, GameObject item, int count)
	{
		this.activatorName = activatorName;
		this.inventoryID = inventoryID;
		this.item = item;
		this.count = count;
	}

	public InventoryEntry(InventoryCount inventoryCount)
	{
		InventoryItem item = inventoryCount.item.GetComponent<InventoryItem>();
		if (item != null)
		{
			this.activatorName = item.activatorName;
			this.inventoryID = item.inventoryID;
			this.item = inventoryCount.item;
			this.count = inventoryCount.count;
		}
		else
		{
			this.activatorName = "";
			this.inventoryID = -1;
			this.item = inventoryCount.item;
			this.count = inventoryCount.count;
		}
	}
}

// keeps track of inventory (used by containers). extended by CharacterInventory to allow
// the equipping of inventory items by characers.
// TODO: create some containers
public class Inventory : MonoBehaviour {
	// starting inventory of the container
	// TODO: consider moving to a manager object, since that makes more sense for CharacterInventory
	public InventoryCount[] startingInventory;

	Transform dropNode; // node where items will spawn when dropped from inventory

	protected SortedDictionary<int, InventoryCount> inventory;

	// total mass of inventory
	public float inventoryMass
	{
		get { return _inventoryMass; }
	}
	float _inventoryMass = 0f;

	// Use this for initialization
	void Awake () {
		RunStartup();
	}

	// must also be called by derived classes
	protected void RunStartup()
	{
		dropNode = GetComponentInChildren<DropNode>().transform;
		Respawn();
	}

	// initializes inventory list and spawns any specified inventory
	// (acts like a respawning container in Skyrim)
	public void Respawn()
	{
		inventory = new SortedDictionary<int, InventoryCount>();
		foreach (InventoryCount item in startingInventory)
		{
			AddSpawnItem(item.item, item.count);
		}
	}

	// returns a list of all the items, for UI display purposes
	public List<InventoryEntry> GetInventory()
	{
		List<InventoryEntry> items = new List<InventoryEntry>();
		foreach (KeyValuePair<int, InventoryCount> next in inventory)
		{
			AddSorted(items, new InventoryEntry(next.Value));
		}
		return items;
	}

	// returns a list of all items of the specified category, used for UI display purposes
	public List<InventoryEntry> GetCategory(InventoryCategory category)
	{
		List<InventoryEntry> items = new List<InventoryEntry>();
		foreach(KeyValuePair<int, InventoryCount> next in inventory)
		{
			if (InventoryLookup.GetInventoryCategory(next.Key) == category)
			{
				AddSorted(items, new InventoryEntry(next.Value));
			}
		}
		return items;
	}

	// removes the specified item and count from inventory
	// returns the number removed
	public int RemoveItem(int inventoryID, int count)
	{
		if (count <= 0)
		{
			return 0;
		}
		InventoryCount item;
		if (inventory.TryGetValue(inventoryID, out item))
		{
			if (count > item.count)
			{
				count = item.count;
			}
			if (count == item.count)
			{
				inventory.Remove(inventoryID);
			}
			else
			{
				item.count -= count;
			}
			float mass = 0f;
			Equipment equipment = item.item.GetComponent<Equipment>();
			if (equipment != null)
			{
				mass = equipment.mass;
			}
			_inventoryMass -= count * mass;
			return count;
		}
		return 0;
	}

	// drops the specified number of the item from inventory
	// returns a reference to the dropped item (if found)
	public GameObject DropItem(int inventoryID, int count)
	{
		GameObject instance = null;
		int numDropped = RemoveItem(inventoryID, count);
		if (numDropped > 0)
		{
			GameObject go = InventoryLookup.GetInventoryObject(inventoryID);
			if (go != null)
			{
				Stackable stackable = go.GetComponent<Stackable>();
				if (stackable == null)
				{
					for (int i = 0; i < numDropped; i++)
					{
						instance = GameObject.Instantiate<GameObject>(go, dropNode.position, dropNode.rotation);
					}
				}
				else
				{
					instance = GameObject.Instantiate<GameObject>(go, dropNode.position, dropNode.rotation);
					stackable = instance.GetComponent<Stackable>();
					stackable.stackCount = numDropped;
				}
			}
		}
		return instance;
	}

	// TODO: transfers an item from one inventory to another
	public void TransferItemFrom(int inventoryID, int count, Inventory source)
	{

	}

	// TODO: transfers all items from one inventory to another
	public void TranferAllFrom(Inventory source)
	{

	}

	// adds an item to inventory, if it is a valid InventoryItem
	public void AddItem(int inventoryID, int count)
	{
		if (count > 0)
		{
			GameObject item = InventoryLookup.GetInventoryObject(inventoryID);
			if (item != null)
			{
				InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
				if (inventoryItem != null)
				{
					InventoryCount inventoryCount;
					if (inventory.TryGetValue(inventoryID, out inventoryCount))
					{
						inventoryCount.count += count;
					}
					else
					{
						inventory.Add(inventoryID, new InventoryCount(item, count));
					}
					_inventoryMass += inventoryItem.mass * count;
				}
			}
		}
	}

	// adds an item to inventory (used when respawning)
	void AddSpawnItem(GameObject item, int count)
	{
		InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
		if (inventoryItem != null)
		{
			AddItem(inventoryItem.inventoryID, count);
		}
	}

	// used to create an alphabetically sorted list of inventory items for UI display purposes
	// TODO: implement a log(n) search for better performance
	public static void AddSorted(List<InventoryEntry> list, InventoryEntry entry)
	{
		int index = 0;
		foreach (InventoryEntry listEntry in list)
		{
			if (listEntry.activatorName.CompareTo(entry.activatorName) > 0)
			{
				break;
			}
			else
			{
				index++;
			}
		}
		list.Insert(index, entry);
	}
}
