using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AI controller for seeker turrets
// TODO: make enemy non-player-specific - implement factions for COMNodes and track a list of targets
public class TurretController : MonoBehaviour {
	// note: two sets of nodes must be specified for the opaque and transparent versions of the model
	[SerializeField] GameObject projectileNode = null;
	[SerializeField] GameObject yawNode = null;
	[SerializeField] GameObject yawNodeTrans = null;
	[SerializeField] GameObject pitchNode = null;
	[SerializeField] GameObject pitchNodeTrans = null;
	[SerializeField] GameObject seekerPrefab = null;
	[SerializeField] float fireInterval = 3f;
	[SerializeField] float rotationInterval = 0.5f;
	[SerializeField] float rotationRate = 40f;
	[SerializeField] float pitchMin = -10f;
	[SerializeField] float pitchMax = 80f;
	[SerializeField] TargetingParams targetingParams = null;

	Animator[] animators;
	float fireTimer = 0f;
	float rotationTimer = 0f;
	Quaternion targetYawRotation;
	Quaternion targetPitchRotation;

	Organ[] organs;
	List<Organ> deadOrgans;
	bool isDead;
	COMNode player;

	TargetingMatrix currentTargetingMatrix;

	// Use this for initialization
	void Start () {
		animators = GetComponentsInChildren<Animator>();
		targetYawRotation = yawNode.transform.rotation;
		targetPitchRotation = pitchNode.transform.localRotation;

		// register for organ deaths
		organs = GetComponentsInChildren<Organ>();
		foreach (Organ organ in organs)
		{
			organ.onDeathChange += OnOrganKilled;
		}
		deadOrgans = new List<Organ>();

		// initialize targeting matrix to prevent null reference exception
		currentTargetingMatrix = new TargetingMatrix();
	}

	// callback function for organ destruction/heals. disables turret on organ death.
	public void OnOrganKilled(Organ organ, bool isDead)
	{
		if (isDead)
		{
			if (!this.isDead)
			{
				this.isDead = true;
				foreach (Animator animator in animators)
				{
					animator.CrossFade("Entry", 0.5f);
				}
			}
			deadOrgans.Add(organ);
		}
		else
		{
			deadOrgans.Remove(organ);
			if (deadOrgans.Count == 0)
			{
				isDead = false;
			}
		}
	}

	private void LateUpdate()
	{
		if (!isDead)
		{
			if (player == null)
			{
				// poll until player is found
				GameObject go = GameObject.FindWithTag("Player");
				if (go != null)
				{
					player = go.GetComponentInChildren<COMNode>();
				}
			}
			else
			{
				fireTimer -= Time.deltaTime;
				rotationTimer -= Time.deltaTime;

				// calculate yaw and pitch of target
				if (rotationTimer <= 0f)
				{
					rotationTimer = rotationInterval;
					SetTargetingMatrix();
					if (currentTargetingMatrix.canDetect)
					{
						UpdateTargetRotations();
					}
				}

				// rotate towards target yaw and pitch
				RotateTowardsTarget();

				// fire seeker projectile
				if (fireTimer <= 0f)
				{
					fireTimer = fireInterval;
					if (currentTargetingMatrix.canDetect && currentTargetingMatrix.canHitProjectile)
					{
						FireOnTarget();
					}
				}
			}
		}
	}

	// communicate with player's COMNode to determine if they are detected and shootable
	void SetTargetingMatrix()
	{
		currentTargetingMatrix = player.GetTargetingMatrix(projectileNode.transform.position, targetingParams);
	}

	void FireOnTarget()
	{
		GameObject go = GameObject.Instantiate(seekerPrefab, projectileNode.transform.position, projectileNode.transform.rotation);
		Seeker seeker = go.GetComponent<Seeker>();
		if (seeker != null)
		{
			seeker.SetTarget(player.transform);
		}
		else
		{
			GameObject.Destroy(go);
		}
	}

	void UpdateTargetRotations()
	{
		// compute pitch target rotation
		Vector3 headingVector = player.transform.position - pitchNode.transform.position;
		float pitchRotation = Mathf.Clamp(Mathf.Rad2Deg * Mathf.Asin(headingVector.y / headingVector.magnitude), pitchMin, pitchMax);
		targetPitchRotation = Quaternion.Euler(0f, 0f, -pitchRotation);

		// compute yaw target rotation
		headingVector = player.transform.position - yawNode.transform.position;
		headingVector.y = 0f;
		targetYawRotation = Quaternion.FromToRotation(Vector3.left, headingVector);
	}

	void RotateTowardsTarget()
	{
		// apply yaw
		Quaternion rotationTemp = Quaternion.RotateTowards(yawNode.transform.rotation, targetYawRotation, rotationRate * Time.deltaTime);
		yawNode.transform.rotation = rotationTemp;
		yawNodeTrans.transform.rotation = rotationTemp;
		// apply pitch
		rotationTemp = Quaternion.RotateTowards(pitchNode.transform.localRotation, targetPitchRotation, rotationRate * Time.deltaTime);
		pitchNode.transform.localRotation = rotationTemp;
		pitchNodeTrans.transform.localRotation = rotationTemp;
	}
}
