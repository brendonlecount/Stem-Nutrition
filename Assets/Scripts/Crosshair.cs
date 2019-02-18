using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creates a crosshair at the center of the field of vision.
// Also keeps track of what that crosshair is currently targeting
public class Crosshair : MonoBehaviour {
	public GameObject crosshairPrefab;		// what the crosshair should be
	public float minDistance;
	public float maxDistance;
	public bool displayCrosshair;			// should the crosshair be displayed?
	public LayerMask mask;					// TODO: use layer manager singleton value
	public float activationDistance;		// max distance at which objects can be activated
	public float activationRadius;			// fuzziness of activation detection

	GameObject crosshair;
	Vector3 targetPosition;
	float targetDistance;
	float initialScale;
	ConditionComponent targetConditionComponent;
	Stem.Activator targetActivator;

	int targetCell;

	// instantiate the prefab, if appropriate and able
	private void Start()
	{
		if (displayCrosshair && crosshairPrefab != null)
		{
			if (crosshair == null)
			{
				crosshair = GameObject.Instantiate(crosshairPrefab);
				initialScale = crosshair.transform.localScale.x;
			}
		}
		else
		{
			displayCrosshair = false;
		}
	}

	// re-instantiate crosshair when level was loaded
	// TODO: flag crosshair as DontDestroyOnLoad to get rid of OnLevelWasLoaded editor warning
	private void OnLevelWasLoaded(int level)
	{
		if (displayCrosshair && crosshairPrefab != null)
		{
			if (crosshair == null)
			{
				crosshair = GameObject.Instantiate(crosshairPrefab);
				initialScale = crosshair.transform.localScale.x;
			}
		}
		else
		{
			displayCrosshair = false;
		}
	}

	// Update the crosshair position, current target, and current activation target
	public void UpdateCrosshair () {
		crosshair.SetActive(true);
		RaycastHit hit;
		// perform raycast to determine crosshair position and current target
		if (Physics.Raycast(transform.position + transform.rotation * Vector3.forward * minDistance, transform.rotation * Vector3.forward, out hit, maxDistance - minDistance, mask.value))
		{
			targetPosition = hit.point;
			targetDistance = hit.distance + minDistance;
			targetConditionComponent = hit.collider.gameObject.GetComponent<ConditionComponent>();
			if (targetConditionComponent != null && targetConditionComponent.hasCells)
			{
				targetCell = hit.triangleIndex / 3;
			}
			else
			{
				targetCell = -1;
			}
			//				DrawCell(hit.collider, hit.triangleIndex);
			//			Debug.Log("Trace Hit");
		}
		else
		{
			targetPosition = transform.position + transform.rotation * Vector3.forward * maxDistance;
			targetDistance = maxDistance;
			targetConditionComponent = null;
			targetCell = -1;
			//			Debug.Log("Trace Miss");
		}
		//		Debug.Log("Target: " + targetPosition.x + ", " + targetPosition.y + ", " + targetPosition.z);

		// perform spherecast to determine current activation target (increase radius if items are too hard to select)
		if (Physics.SphereCast(transform.position + transform.rotation * Vector3.forward * minDistance, activationRadius, transform.rotation * Vector3.forward, out hit, activationDistance - minDistance, mask.value))
		{
			targetActivator = hit.collider.gameObject.GetComponent<Stem.Activator>();
			if (targetActivator == null && hit.collider.transform.parent != null)
			{
				targetActivator = hit.collider.transform.parent.GetComponent<Stem.Activator>();
			}
		}
		else
		{
			targetActivator = null;
		}

		if (displayCrosshair)
		{
			crosshair.transform.position = targetPosition;
			float newScale = targetDistance * initialScale / minDistance;
			crosshair.transform.localScale = Vector3.one * newScale;
		}
	}

	public void HideCrosshair()
	{
		crosshair.SetActive(false);
	}

	// position of targeting crosshair. used to orient weapons.
	public Vector3 GetTargetPosition()
	{
		return targetPosition;
	}

	public Stem.Activator GetTargetActivator()
	{
		return targetActivator;
	}

	public ConditionComponent GetTargetConditionComponent()
	{
		return targetConditionComponent;
	}

	public int GetTargetArmorCell()
	{
		return targetCell;
	}

	// gizmo used to highlight currently targeted cell for debug purposes
	void DrawCell(Collider collider, int triangleIndex)
	{
		MeshCollider meshCollider = collider as MeshCollider;
		if (meshCollider == null || meshCollider.sharedMesh == null)
			return;


		Mesh mesh = meshCollider.sharedMesh;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		Vector3 p0 = vertices[triangles[triangleIndex * 3 + 0]];
		Vector3 p1 = vertices[triangles[triangleIndex * 3 + 1]];
		Vector3 p2 = vertices[triangles[triangleIndex * 3 + 2]];
		Transform hitTransform = collider.transform;
		p0 = hitTransform.TransformPoint(p0);
		p1 = hitTransform.TransformPoint(p1);
		p2 = hitTransform.TransformPoint(p2);
		Debug.DrawLine(p0, p1);
		Debug.DrawLine(p1, p2);
		Debug.DrawLine(p2, p0);
	}

}
