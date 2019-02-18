using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// positions a hand node based on current pose (standing, crouching, or prone)
// TODO: replace with actual hand animations and/or IK
public class HandPoser : MonoBehaviour, IMovementControllerReliant {
	// position associated with each pose
	[SerializeField] Transform standingNode;
	[SerializeField] Transform crouchingNode;
	[SerializeField] Transform proneNode;

	// event handler that applies the hand position for the new pose
	public void PositionHand(Pose pose)
	{
		switch (pose)
		{
			case Pose.Standing:
				transform.localPosition = standingNode.localPosition;
				break;
			case Pose.Crouching:
				transform.localPosition = crouchingNode.localPosition;
				break;
			case Pose.Prone:
				transform.localPosition = proneNode.localPosition;
				break;
		}
	}

	// lets it respond to changes in pose via the onPoseChange event
	void IMovementControllerReliant.SetMovementController(MovementController movementController)
	{
		PositionHand(movementController.pose);
		movementController.onPoseChange += PositionHand;
	}

}
