using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// implements the tab menu panel switcher 
// TODO: consider moving panel/button SetActives and interactables to a function here
public class TabMenu : MonoBehaviour {
	enum SwitchMode { Inventory, Gear, Health, Physique, Metabolism}

	[SerializeField] GameObject inventoryPanel = null;
	[SerializeField] GameObject gearPanel = null;
	[SerializeField] GameObject healthPanel = null;
	[SerializeField] GameObject physiquePanel = null;
	[SerializeField] GameObject metabolismPanel = null;

	[SerializeField] Button inventoryButton = null;
	[SerializeField] Button gearButton = null;
	[SerializeField] Button healthButton = null;
	[SerializeField] Button physiqueButton = null;
	[SerializeField] Button metabolismButton = null;


	private void OnEnable()
	{
		SetSwitchMode(SwitchMode.Inventory);
	}

	public void InventoryButtonClicked() { SetSwitchMode(SwitchMode.Inventory); }
	public void GearButtonClicked() { SetSwitchMode(SwitchMode.Gear); }
	public void HealthButtonClicked() { SetSwitchMode(SwitchMode.Health); }
	public void PhysiqueButtonClicked() { SetSwitchMode(SwitchMode.Physique); }
	public void MetabolismButtonClicked() { SetSwitchMode(SwitchMode.Metabolism); }

	void SetSwitchMode(SwitchMode switchMode)
	{
		inventoryButton.interactable = switchMode != SwitchMode.Inventory;
		gearButton.interactable = switchMode != SwitchMode.Gear;
		healthButton.interactable = switchMode != SwitchMode.Health;
		physiqueButton.interactable = switchMode != SwitchMode.Physique;
		metabolismButton.interactable = switchMode != SwitchMode.Metabolism;

		inventoryPanel.SetActive(switchMode == SwitchMode.Inventory);
		gearPanel.SetActive(switchMode == SwitchMode.Gear);
		healthPanel.SetActive(switchMode == SwitchMode.Health);
		physiquePanel.SetActive(switchMode == SwitchMode.Physique);
		metabolismPanel.SetActive(switchMode == SwitchMode.Metabolism);
	}

}
