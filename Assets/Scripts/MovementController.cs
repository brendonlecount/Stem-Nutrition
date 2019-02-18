using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// what type of movement is currently happening?
public enum Ambulation { Stand, Walk, Run, Sprint, Crouch, Sneak, Lay, Crawl, Unset }
// what pose is the character in?
public enum Pose { Standing, Crouching, Prone, Unset }

// keeps track of data specifying a pose
[System.Serializable]
public class PoseData
{
	[Tooltip("Pose identifier.")]
	public Pose poseTag;
	[Tooltip("Display name of pose.")]
	public string poseName = "Standing";
	[Tooltip("Overall height of character.")]
	public float height = 2.15f;
	[Tooltip("Distance from footCastNode to ground (hover height).")]
	public float groundDistance = 0.4f;
	[Tooltip("Extension beyond groundDistance where hover force continues to act.")]
	public float toeDistance = 0.5f;
	[Tooltip("Is the player horizontal?")]
	public bool isHorizontal = false;
	[Tooltip("Visual detection radius.")]
	public float visualDetectionRadius = 100f;
}

// movement controller for walking/running/crawling, etc.
// meant to be attached to a properly configured prefab, intantiated by an InputSource
// instantiates a target body, keeping track of associated components
// gets flagged as DontDestroyOnLoad if controlled by the player
// TODO: consider removing DontDestroyOnLoad when no longer controlled by player (how?) since
// this game would eventually implement body switching
public class MovementController : MonoBehaviour {
	[Tooltip("Target prefab from Targets folder.")]
	public GameObject targetPrefab;

	[Tooltip("Height data for stading, crouching, and prone.")]
	public PoseData[] poses;
	public AmbulationData[] ambulations;
	public GameObject pivot;
	public GameObject firstPersonCameraPivot;

	[Tooltip("X dimension of character box collider.")]
	public float colliderWidth;
	[Tooltip("Z dimension of character box collider.")]
	public float colliderDepth;
	[Tooltip("Distance from top of head to pivot point.")]
	public float pivotHeight;

	[Tooltip("Force magnitude at toeDIstance.")]
	public float toeGs = 0.5f;
	[Tooltip("Force magnitude at zero distance (maximum hover force).")]
	public float maxLegGs = 2f;
	[Tooltip("Hover force damping multiplier.")]
	public float legDampingGMult = 1f;
	[Tooltip("Radius of standing force spherecast.")]
	public float footCastHalfHeight = 0.05f;


	[Tooltip("Force mult for force responsible for accelerating player to target velocity.")]
	public float movementForceMult = 1500f;     // target velocity force multiplier
	[Tooltip("Jumping impulse.")]
	public float jumpForce = 400f;				// jumping force

	Rigidbody body;             // rigidbody
	BoxCollider boxCollider;    // collider	

	// getters and setters for various components on the target and controller
	public Physique physique { get; private set; }

	public CharacterInventory inventory { get; private set; }

	public WeaponManager weapons { get; private set; }

	public ArmorManager armor { get; private set; }

	public COMNode comNode { get; private set; }

	public IronSightsNode ironSightsNode { get; private set; }

	public InputSource inputSource { get; private set; }

	public bool isPlayer
	{
		get { return gameObject.tag == "Player"; }
	}

	// motion parameters set by input controller (player or AI based)
	Vector3 movementDirection;           // WASD movement
	float yaw;                  // horizontal rotation
	bool jump;                  // jump on next physics frame?
	float speed;
	float acceleration;
	Vector3 velocityLast;
	GameObject targets;
	public bool isAwake { get; private set; }

	public  bool isAlive
	{
		get { return _isAlive; }
	}
	bool _isAlive = true;

	// getter/setter for ambulation. Implementation move to Physique component,
	// since available ambulations depend on stamina.
	public Ambulation ambulation
	{
		get
		{
			return physique.ambulation;
		}

		set
		{
			physique.TryToSetAmbulation(value);
		}
	}

	// getter/setter for current pose (determined by ambulation)
	public Pose pose
	{
		get
		{
			if (poseIndex == -1)
			{
				return Pose.Unset;
			}
			else
			{
				return poses[poseIndex].poseTag;
			}
		}
	}
	int poseIndex = -1;

	// listener for Physique ambulation change events, updates pose and detection modifiers
	void OnAmbulationChanged(AmbulationData ambulation)
	{
		ApplyAuditoryDetectionRange(ambulation.auditoryDetectionRadius);
		if (this.pose != ambulation.pose)
		{
			for (int i = 0; i < poses.Length; i++)
			{
				if (poses[i].poseTag == ambulation.pose)
				{
					poseIndex = i;
					AdjustCollider();
					ApplyVisualDetectionRange();
					// fire pose change event
					if (onPoseChange != null)
					{
						onPoseChange(poses[i].poseTag);
					}
					return;
				}
			}
		}
	}

	private void ApplyAuditoryDetectionRange(float radius)
	{
		comNode.RemoveDetectionModifier(DetectionMode.Auditory, auditoryDetectionID);

		auditoryDetectionID = comNode.AddDetectionModifier(DetectionMode.Auditory, radius);
	}

	// adjusts collider and casting dimensions to accomodate the current pose
	// TODO: apply relevant animations to target body
	void AdjustCollider()
	{
		if (poses[poseIndex].isHorizontal)
		{
			boxCollider.size = new Vector3(colliderWidth, colliderDepth, poses[poseIndex].height);
			boxCollider.center = Vector3.back * (poses[poseIndex].height / 2f - pivotHeight);
			castDimensions = new Vector3(colliderWidth / 2f, footCastHalfHeight, poses[poseIndex].height / 2f);
			castOffset = new Vector3(0f, -colliderDepth / 2f + 2f * footCastHalfHeight, pivotHeight - poses[poseIndex].height / 2f);
			castDistance = poses[poseIndex].groundDistance + poses[poseIndex].toeDistance + footCastHalfHeight;
			castAdjust = -2f * footCastHalfHeight;

			pivot.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
			firstPersonCameraPivot.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0f, 0f));
			targetRotation = pivot.transform.localRotation;
			targetOffset = Vector3.back * (poses[poseIndex].height - pivotHeight);
		}
		else
		{
			float colliderHeight = poses[poseIndex].height - poses[poseIndex].groundDistance + footCastHalfHeight;
			boxCollider.size = new Vector3(colliderWidth, colliderHeight, colliderDepth);
			boxCollider.center = Vector3.down * (colliderHeight / 2f - pivotHeight);
			castDimensions = new Vector3(colliderWidth / 2f, footCastHalfHeight, colliderDepth / 2f);
			castOffset = Vector3.down * (poses[poseIndex].height - poses[poseIndex].groundDistance - footCastHalfHeight - pivotHeight);
			castDistance = poses[poseIndex].groundDistance + poses[poseIndex].toeDistance;
			castAdjust = -footCastHalfHeight;

			pivot.transform.localRotation = Quaternion.identity;
			firstPersonCameraPivot.transform.localRotation = Quaternion.identity;
			targetRotation = Quaternion.identity;
			targetOffset = Vector3.down * (poses[poseIndex].height - pivotHeight);
		}
	}

	void ApplyVisualDetectionRange()
	{
		comNode.RemoveDetectionModifier(DetectionMode.PassiveVisual, visualDetectionID);
		comNode.RemoveDetectionModifier(DetectionMode.PassiveRadar, radarDetectionID);

		visualDetectionID = comNode.AddDetectionModifier(DetectionMode.PassiveVisual, poses[poseIndex].visualDetectionRadius);
		radarDetectionID = comNode.AddDetectionModifier(DetectionMode.PassiveRadar, poses[poseIndex].visualDetectionRadius);
	}

	public string poseName
	{
		get
		{
			if (poseIndex == -1)
			{
				return "Pose not set.";
			}
			else
			{
				return poses[poseIndex].poseName;
			}
		}
	}

	public delegate void OnPoseChange(Pose newPose);
	public event OnPoseChange onPoseChange;

	Vector3 castDimensions;
	Vector3 castOffset;
	float castDistance;
	float castAdjust;
	Vector3 targetOffset;
	Quaternion targetRotation;
	int auditoryDetectionID = -1;
	int visualDetectionID = -1;
	int radarDetectionID = -1;

	// is the player on the ground?
	bool grounded;

	// Use this for initialization
	void Awake()
	{
		// intialize local properties
		yaw = transform.rotation.eulerAngles.y;
		body = GetComponent<Rigidbody>();
		boxCollider = GetComponent<BoxCollider>();
		comNode = GetComponentInChildren<COMNode>();
		ironSightsNode = GetComponentInChildren<IronSightsNode>();

		forwardLeft = (new Vector3(-1f, 0f, 1f)).normalized;
		forwardRight = (new Vector3(1f, 0f, 1f)).normalized;
		backLeft = (new Vector3(-1f, 0f, -1f)).normalized;
		backRight = (new Vector3(1f, 0f, -1f)).normalized;
	}

	// called by controlling InputSource, also used to determine isPlayer
	// meant to be called whenever controller is changed once body swapping is implemented
	public void SetInputSource(InputSource inputSource)
	{
		this.inputSource = inputSource;
		PlayerInput playerInput = inputSource as PlayerInput;
		if (playerInput != null)
		{
			gameObject.tag = "Player";
			GameObject.DontDestroyOnLoad(this.gameObject);
			GameObject.DontDestroyOnLoad(targets.gameObject);
		}
		else
		{
			gameObject.tag = "Untagged";
			// can't remove DontDestroyOnLoad :/
			// TODO: consider deleting this on scene load if no longer IsPlayer
		}
	}

	// called by InputSource
	// TODO: merge with Awake
	public void Wake()
	{
		// instantiate target
		targets = GameObject.Instantiate(targetPrefab);

		inventory = targets.GetComponent<CharacterInventory>();
		physique = targets.GetComponent<Physique>();
		armor = targets.GetComponent<ArmorManager>();
		weapons = targets.GetComponent<WeaponManager>();

		physique.onAmbulationChanged += OnAmbulationChanged;
		physique.TryToSetAmbulation(Ambulation.Stand);
		physique.metabolismActive = true;

		IMovementControllerReliant[] movementControllerReliant = targets.GetComponentsInChildren<IMovementControllerReliant>();
		foreach (IMovementControllerReliant imcr in movementControllerReliant)
		{
			imcr.SetMovementController(this);
		}

		isAwake = true;
	}


	// Update is called once per frame
	void Update()
	{
		if (isAwake)
		{
			// apply yaw rotation
			transform.rotation = Quaternion.Euler(0f, yaw, 0f);
		}
	}

	// Positions the target body. Called by InputSource because the order of updating
	// various positions and rotations matters.
	public void PositionTargets()
	{
		targets.transform.position = transform.position + transform.rotation * targetOffset;
		targets.transform.rotation = transform.rotation * targetRotation;
	}

	private void FixedUpdate()
	{
		if (isAwake)
		{
			// calculate speed and acceleration
			speed = body.velocity.magnitude;
			acceleration = (velocityLast - body.velocity).magnitude / Time.deltaTime;
			velocityLast = body.velocity;

			// apply WASD movement
			
			Vector3 targetVelocity = transform.rotation * movementDirection * physique.GetAmbulationSpeed();
			Vector3 horizontalVelocity = new Vector3(body.velocity.x, 0f, body.velocity.z);
			body.AddForce((targetVelocity - horizontalVelocity) * movementForceMult);

			GenerateStandingForce();

			// apply jump, if applicable
			if (jump)
			{
				jump = false;
				body.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
			}
		}
	}

	// Instead of having the character skate along with the collider making contact with the ground,
	// a raycast is performed and a standing force is calculated depending on the resulting distance.
	// This is done to prevent the character from bouncing up into the air when encountering bumps at
	// high speed. A raycast is already needed to determine if the controller is grounded, so it isn't
	// much more work.
	void GenerateStandingForce()
	{
		RaycastHit hit;
		Vector3 castPoint = transform.position + castOffset;
		if (Physics.BoxCast(castPoint, castDimensions, Vector3.down, out hit, Quaternion.identity, castDistance, Masks.Raycast.standable))
		// boxcasts were ultimately used because raycasts could potentially fall through cracks, and spherecasts overly complicate
		// calculating the ground distance
		//if (Physics.SphereCast(castPoint, footCastRadius, Vector3.down, out hit, groundDistance + toeDistance, legMask.value))
		//Vector3 castPoint = transform.position + Vector3.up * (groundDistance + toeDistance);
		//if (Physics.Raycast(castPoint, Vector3.down, out hit, groundDistance + toeDistance, legMask.value))
		{
			grounded = true;			// the player is standing on the ground (can jump)
			float standingGs;
			float distance = castPoint.y - hit.point.y + castAdjust; // distance used for force calculation
			if (distance < poses[poseIndex].groundDistance)
			{
				standingGs = maxLegGs + distance * (1f - maxLegGs) / poses[poseIndex].groundDistance;
			}
			else
			{
				// represents force generated when player is reaching ground via extended toes
				// (allows for a bit more damping)
				distance -= poses[poseIndex].groundDistance;
				standingGs = 1f + distance * (toeGs - 1f) / poses[poseIndex].toeDistance;
			}
			standingGs -= legDampingGMult * body.velocity.y;
			body.AddForce(Vector3.down * standingGs * Physics.gravity.y, ForceMode.Acceleration);
		}
		else
		{
			grounded = false;
		}
	}

	// setter used by SceneEntry and Reset button to set the position and rotation of the character
	public void SetLocation(Vector3 position, float yaw)
	{
		transform.position = position + Vector3.up * (poses[poseIndex].height - pivotHeight);
		this.yaw = yaw;
	}

	public Vector3 GetPosition()
	{
		return transform.position - Vector3.up * (poses[poseIndex].height - pivotHeight);
	}

	public float GetYaw()
	{
		return yaw;
	}

	// condition the specified yaw to be between +/-180
	public void SetYaw(float yaw)
	{
		while (yaw > 180f)
		{
			yaw -= 360f;
		}
		while (yaw <= 180f)
		{
			yaw += 360f;
		}
		this.yaw = yaw;
	}

	// efficient but ugly function used to go from an axis reading to a movement direction
	// wouldn't work too well for controllers, but a vector could maybe be
	// calculated directly from the direction vector and normalized.
	public void SetMovement(Vector2 direction)
	{
		// TODO: fix for controllers (ug)
		if (direction.x > Mathf.Epsilon)
		{
			if (direction.y < -Mathf.Epsilon)
			{
				movementDirection = backRight;
			}
			else if (direction.y > Mathf.Epsilon)
			{
				movementDirection = forwardRight;
			}
			else
			{
				movementDirection = Vector3.right;
			}
		}
		else if (direction.x < -Mathf.Epsilon)
		{
			if (direction.y < -Mathf.Epsilon)
			{
				movementDirection = backLeft;
			}
			else if (direction.y > Mathf.Epsilon)
			{
				movementDirection = forwardLeft;
			}
			else
			{
				movementDirection = Vector3.left;
			}
		}
		else
		{
			if (direction.y < -Mathf.Epsilon)
			{
				movementDirection = Vector3.back;
			}
			else
			{
				movementDirection = Vector3.forward;
			}
		}
	}
	Vector3 forwardLeft;
	Vector3 forwardRight;
	Vector3 backLeft;
	Vector3 backRight;

	// called by InputSource to trigger a jump when standing and grounded
	public void TriggerJump()
	{
		if (pose == Pose.Standing && grounded)
		{
			jump = true;
		}
	}

	// getters used by HUD
	public Pose GetPose()
	{
		return pose;
	}

	public float GetSpeed()
	{
		return speed;
	}

	public float GetAcceleration()
	{
		return acceleration;
	}
}
