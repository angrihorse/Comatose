using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Experimental.Rendering.Universal;

public struct TurretMoment
{
	public Vector2 aimPosition;
	public float delayedFor;
	public TurretMoment(Vector2 aimPosition, float delayedFor)
	{
		this.aimPosition = aimPosition;
		this.delayedFor = delayedFor;
	}
}

public class Target
{
	public Toggle toggle;
	public Vector2 offset;
	public bool hittable;
	public Vector2 point;
	
	public Target(Toggle toggle)
	{
		this.toggle = toggle;
	}
}

public class TurretInput : CustomInput, IRewindable
{
	Stack<TurretMoment> moments = new Stack<TurretMoment>();
	List<Target> targets = new List<Target>();	
	Target currentTarget;

	Toggle toggle;
	Transform firePoint;

	[SerializeField] LayerMask solid;

	[SerializeField] float aimSpeed;	
	[SerializeField] float desiredAccuracy;

	[SerializeField] float shotDelay;
	float delayedFor;

	[SerializeField] int hitBoxPointsCount;
	List<Vector2> hitPointsOffsets = new List<Vector2>();
	[SerializeField] float hitBoxRadius;
	[SerializeField] float bulletRadius;

	[SerializeField] LineRenderer laser;
	Vector2 laserEnd;
	[SerializeField] Color altColor;
	[SerializeField] bool useAltColor;

	[SerializeField] Light2D fireLight;
	[SerializeField] float fireLightMaxIntensity;

	[SerializeField] bool linearAim;
	[SerializeField] bool aimThroughWalls;
	[SerializeField] float range;

	void Awake()
    {
		toggle = GetComponent<Toggle>();
		firePoint = transform.Find("FirePoint");

		laser.transform.parent = transform.parent;
		laser.transform.position = Vector3.zero;
		laser.transform.eulerAngles = Vector3.zero;

		if (useAltColor)
        {
			laser.material.color = altColor;
			fireLight.color = altColor;
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		laser.enabled = toggle.Active;
		targets.Add(new Target(GameObject.FindGameObjectWithTag("Player").GetComponent<Toggle>()));
		targets.Add(new Target(GameObject.FindGameObjectWithTag("Shadow").GetComponent<Toggle>()));

		aimPosition = transform.position + transform.up;
		laser.SetPosition(0, firePoint.position);

		for (int i = 0; i < hitBoxPointsCount; i++)
		{
			float angle = i * 2 * Mathf.PI / hitBoxPointsCount;
			hitPointsOffsets.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * hitBoxRadius);
		}
	}

	void OnDrawGizmos()
	{	
		Handles.DrawWireDisc(aimPosition, Vector3.forward, desiredAccuracy);		
		if (currentTarget != null)
        {
			Handles.DrawLine(aimPosition, currentTarget.toggle.transform.position);
        }
	}

	// Update is called once per frame
	void Update()
    {
		laser.gameObject.SetActive(toggle.Active);
		
		if (aimThroughWalls)
        {
			laserEnd = aimPosition;
		} else
        {
			RaycastHit2D hit = Physics2D.Raycast(firePoint.position, transform.up, Mathf.Infinity, solid);
			if (hit.collider != null)
			{
				laserEnd = hit.point;
			}
			else
			{
				laserEnd = transform.position + 100f * transform.up;
			}
		}			

		laser.SetPosition(1, laserEnd);
        fireLight.intensity = fireLightMaxIntensity * delayedFor / shotDelay;
	}

	void FixedUpdate()
	{
		// Skip condition.
		if (Chronos.Instance.isRewinding || !toggle.Active)
		{
			return;
		}		

		// Find closest hittable point on each target.
		foreach (Target target in targets)
		{
			target.hittable = false;
			if ((target.toggle.transform.position - transform.position).magnitude > range)
				continue;

			Vector2[] hitPoints = new Vector2[hitBoxPointsCount];
			for (int i = 0; i < hitBoxPointsCount; i++)
			{
				hitPoints[i] = (Vector2)target.toggle.transform.position + hitPointsOffsets[i];
			}
			hitPoints = hitPoints.OrderBy(point => (point - (Vector2)firePoint.position).magnitude).ToArray();

			foreach (Vector2 point in hitPoints)
			{
				Vector2 pointOffset = point - (Vector2)firePoint.position;
				RaycastHit2D hit = Physics2D.CircleCast(firePoint.position, bulletRadius, pointOffset, Mathf.Infinity, solid);
				bool pointHittable = hit.collider != null && (hit.collider.gameObject.CompareTag("Player") || hit.collider.gameObject.CompareTag("Shadow"));
							
				if (pointHittable)
                {
					target.hittable = pointHittable;
					target.offset = pointOffset;
					target.point = point;
					break;
				}
				
				if (aimThroughWalls)
                {
					target.hittable = false;
					target.offset = target.toggle.transform.position - transform.position;
					target.point = target.toggle.transform.position;
				}
			}
		}

		// Find closest hittable target, always giving shadow a priority.
		if (currentTarget == null)
		{
			List<Target> orderedTargets = targets.OrderBy(x => x.offset.magnitude).ToList();
			currentTarget = orderedTargets.FirstOrDefault(x => x.toggle.Active && x.hittable);			
		} else if (!currentTarget.toggle.Active || !currentTarget.hittable)
        {
			currentTarget = null;
		}

		if (currentTarget == null)
		{
			if (aimThroughWalls)
            {
				List<Target> orderedTargets = targets.OrderBy(x => x.offset.magnitude).ToList();
				currentTarget = orderedTargets.FirstOrDefault(x => x.toggle.Active);
			} else
            {
				delayedFor = 0;
				shoot = false;
				return;
			}			
		}

		// Aim.
		if (linearAim)
        {
			aimPosition = Vector2.MoveTowards(aimPosition, currentTarget.point, aimSpeed * Time.fixedDeltaTime);
		} else
        {
			aimPosition = Vector2.Lerp(aimPosition, currentTarget.point, aimSpeed * Time.fixedDeltaTime);
		}
		
		// Shoot.
		shoot = false;
		bool atGunpoint = (aimPosition - currentTarget.point).magnitude < desiredAccuracy;
		if (currentTarget.hittable && atGunpoint)
		{
			delayedFor += Time.fixedDeltaTime;
			if (delayedFor > shotDelay)
            {
				delayedFor = 0;
				shoot = true;
			}			
		}
		else
		{
			delayedFor = 0;
		}
	}

	public void Record()
	{
		moments.Push(new TurretMoment(aimPosition, delayedFor));
	}

	public void Rewind()
	{
		TurretMoment moment = moments.Pop();
		aimPosition = moment.aimPosition;
		delayedFor = moment.delayedFor;
		shoot = false;
	}
}
