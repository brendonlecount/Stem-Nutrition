using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// implements the tab menu panel switcher 
// TODO: consider moving panel/button SetActives and interactables to a function here
public class TabMenu : MonoBehaviour {
	public GameObject inventoryPanel;
	public GameObject gearPanel;
	public GameObject healthPanel;
	public GameObject physiquePanel;

	public Button inventoryButton;
	public Button gearButton;
	public Button healthButton;
	public Button physiqueButton;


	private void Start()
	{
		inventoryButton.interactable = false;
		gearButton.interactable = true;
		healthButton.interactable = true;
		physiqueButton.interactable = true;

		inventoryPanel.SetActive(true);
		gearPanel.SetActive(false);
		healthPanel.SetActive(false);
		physiquePanel.SetActive(false);
	}
}
