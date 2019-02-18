using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// button used to keep track of ingestible information on the Physique menu
public class IngestibleButton : MonoBehaviour {
	public Button button;
	public Text text;

	PhysiqueMenu physiqueMenu;
	int inventoryID;
	Ingestible ingestible;
	int count;

	public void InitializeButton(PhysiqueMenu physiqueMenu, InventoryEntry entry)
	{
		this.physiqueMenu = physiqueMenu;
		text.text = entry.activatorName;
		this.inventoryID = entry.inventoryID;
		this.ingestible = entry.item.GetComponent<Ingestible>();
		this.count = entry.count;
	}

	public void OnClick()
	{
		physiqueMenu.IngestiblePressed(this);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	public Ingestible GetIngestible()
	{
		return ingestible;
	}

	public int GetInventoryID()
	{
		return inventoryID;
	}

	public int GetCount()
	{
		return count;
	}
}
