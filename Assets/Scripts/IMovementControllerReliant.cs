using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface used to pass a movement controller to any items dependent upon it
interface IMovementControllerReliant {
	void SetMovementController(MovementController movementController); 
}
