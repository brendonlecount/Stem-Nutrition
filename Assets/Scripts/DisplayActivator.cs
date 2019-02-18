using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class for display screens in the game world that can be interacted with by pressing activate.
// attach to activator.
// switches the menumode to Display and starts the display
public class DisplayActivator : Stem.Activator {
	public Display display;

	public override void Activate(InputSource user)
	{
		MenuManager.menuMode = MenuModes.display;
		display.StartDisplay();
	}

	public override void StopActivate()
	{
		MenuManager.menuMode = MenuModes.none;
		display.StopDisplay();
	}
}
