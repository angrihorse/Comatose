using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CrosshairMoment
{
    public Vector2 position;
    public Quaternion rotation;
    public float halfLength;
    public CrosshairMoment(Vector2 position, Quaternion rotation, float halfLength)
    {
        this.position = position;
        this.rotation = rotation;
        this.halfLength = halfLength;
    }
}

public class Crosshair : MonoBehaviour, IRewindable
{
    Stack<CrosshairMoment> moments = new Stack<CrosshairMoment>();
    CustomInput input;
    Transform player;
    [SerializeField] Transform firePoint;
    LineRenderer line;    
    float halfLength;
    [SerializeField] float minWidth;
    public float maxDist;
    float maxHalfLength;
    Gun gun;
    float maxAngle;

    // Start is called before the first frame update
    void Awake()
    {
        input = GameObject.FindWithTag("Player").GetComponent<CustomInput>();
        input.aimPosition = transform.position;
        player = GameObject.FindWithTag("Player").transform;
        firePoint = player.Find("FirePoint");
        gun = player.GetComponent<Gun>();
        maxAngle = gun.maxAngle;
        line = GetComponent<LineRenderer>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        halfLength = minWidth/2;
        SetLength();
    }


    void SetLength()
    {
        line.SetPosition(0, new Vector3(-halfLength, 0, 0));
        line.SetPosition(1, new Vector3(halfLength, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        // Skip condition.
        if (Chronos.Instance.isRewinding)
        {
            return;
        }

        transform.position = input.aimPosition;
        transform.rotation = player.rotation;

        float dist = (transform.position - firePoint.position).magnitude;
        float currentHalfMaxAngle = Mathf.Clamp01(gun.recoil + gun.motionSpread) * maxAngle / 2;
        currentHalfMaxAngle *= Mathf.Deg2Rad;
        maxHalfLength = dist * Mathf.Tan(currentHalfMaxAngle);
        halfLength = Mathf.Max(minWidth / 2, maxHalfLength);
        SetLength();
    }

    public void Record()
    {
        moments.Push(new CrosshairMoment(transform.position, transform.rotation, halfLength));
    }

    public void Rewind()
    {
        CrosshairMoment moment = moments.Pop();
        transform.position = moment.position;
        input.aimPosition = moment.position;
        transform.rotation = moment.rotation;
        halfLength = moment.halfLength;
        SetLength();
    }
}
