using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraMovement : MonoBehaviour, IRewindable
{
	Stack<Vector3> moments = new Stack<Vector3>();
	[HideInInspector] public Transform player;
	[HideInInspector] public Transform crosshair;
	public float cameraSmoothTime = 0;
	public float cameraMaxDistance = 0;
	Camera cam;
	Vector2 cameraReferenceVelocity;
	Vector2 cameraTarget;	
	// TODO: [SerializeField] RippleEffect rippleEffect;

	void Awake()
    {
		cam = Camera.main;
	}	

	// Update is called once per frame
	void LateUpdate()
    {
		// Skip condition.
		if (Chronos.Instance.isRewinding || Interface.Instance.paused)
		{
			return;
		}

		// Assign target.
		cameraTarget = (Vector2)player.transform.position;

		// Convert viewport mouse position to [-1, 1] range.
		Vector2 viewportMousePos = 2f * cam.ScreenToViewportPoint(cam.WorldToScreenPoint(crosshair.position)) - Vector3.one;

		// Move camera target toward mouse position.
		cameraTarget += viewportMousePos * cameraMaxDistance;

		// Smoothly move the camera.
		Vector2 nextPosition = Vector2.SmoothDamp(cam.transform.position, cameraTarget, ref cameraReferenceVelocity, cameraSmoothTime);
		transform.localPosition = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
	}

	public void Record()
	{
		moments.Push(transform.localPosition);
	}

	public void Rewind()
	{
		transform.localPosition = moments.Pop();
	}
}
