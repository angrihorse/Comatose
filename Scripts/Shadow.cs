using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShadowMoment
{
    public Vector2 position;
    public float rotation;
    public Vector2 velocity;

    public ShadowMoment(Vector2 position, float rotation, Vector2 velocity)
    {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
    }
}


public class Shadow : MonoBehaviour
{
    [SerializeField] GameObject shadowPrefab;
    GameObject shadow;
    Stack<ShadowMoment> movementsMoments = new Stack<ShadowMoment>();
    Rigidbody2D rb;
    Rigidbody2D rbShadow;
    Toggle toggle;
    Toggle toggleShadow;

    [SerializeField] float shadowChargeTime;
    int shadowChargeFixedUpdates;
    bool shadowCharged;
    [SerializeField] float frameFreezeLength;

    // Start is called before the first frame update
    void Awake()
    {
        shadow = Instantiate(shadowPrefab, transform.position, transform.rotation);
        rb = GetComponent<Rigidbody2D>();
        rbShadow = shadow.GetComponent<Rigidbody2D>();
        toggle = GetComponent<Toggle>();
        toggleShadow = shadow.GetComponent<Toggle>();
        toggleShadow.Active = false;
        shadowChargeFixedUpdates = (int)(shadowChargeTime / Time.fixedDeltaTime);
    }

    protected virtual void Start() { 
        Chronos.Instance.onRewindStart += RewindStartRoutine;
        Chronos.Instance.onRewindStop += RewindStopRoutine;
    }

    void OnDisable()
    {
        Chronos.Instance.onRewindStart -= RewindStartRoutine;
        Chronos.Instance.onRewindStop -= RewindStopRoutine;
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        if (Chronos.Instance.isRewinding)
        {
            movementsMoments.Push(new ShadowMoment(rb.position, rb.rotation, rb.velocity));
        }
        else if (movementsMoments.Count > 0 && shadowCharged)
        {
            ShadowMoment moment = movementsMoments.Pop();
            rbShadow.position = moment.position;
            rbShadow.rotation = moment.rotation;
            rbShadow.velocity = moment.velocity;
        }
        else
        {
            rbShadow.velocity = Vector2.zero;
        }        

        if (!toggleShadow.Active)
        {
            shadow.transform.position = transform.position;
            shadow.transform.eulerAngles = transform.eulerAngles;
        }
    }

    protected virtual void RewindStartRoutine()
    {
        toggleShadow.Active = false;
        movementsMoments.Clear();
        shadowCharged = false;
    }

    protected virtual void RewindStopRoutine()
    {
        if (movementsMoments.Count > shadowChargeFixedUpdates)
        {
            toggleShadow.Active = true;            
            shadowCharged = true;
            Interface.Instance.FrameFreeze(frameFreezeLength);
        } else
        {
            RewindStartRoutine();
        }     
    }
}
