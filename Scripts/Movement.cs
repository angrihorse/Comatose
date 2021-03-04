using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MovementMoment
{
    public Vector2 position;
    public float rotation;
    public Vector2 velocity;
    public float slowdownFraction;
    public float slowDownDuration;

    public MovementMoment(Vector2 position, float rotation, Vector2 velocity, float slowdownFraction, float slowDownDuration)
    {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.slowdownFraction = slowdownFraction;
        this.slowDownDuration = slowDownDuration;
    }
}

public class Movement : MonoBehaviour, IRewindable
{
    Stack<MovementMoment> moments = new Stack<MovementMoment>();
    CustomInput input;
    Rigidbody2D rb;
    Toggle toggle;
    Transform firepoint;

    [SerializeField] float maxMoveSpeed;
    [SerializeField] float timeToReachMaxSpeed;
    [SerializeField] float timeToFullyStop;
    [SerializeField] float deadZone;
    [SerializeField] float slowdownPower;

    float slowdownFraction = 1;
    float slowDownDuration;

    float acceleration, decceleration;

    [SerializeField] float minStrafeFraction;
    float strafeFraction = 1;    
    Vector2 strafeCoeff;

    // Start is called before the first frame update
    void Awake()
    {
        input = GetComponent<CustomInput>();
        rb = GetComponent<Rigidbody2D>();
        toggle = GetComponent<Toggle>();
        firepoint = transform.Find("FirePoint");

        acceleration = maxMoveSpeed / timeToReachMaxSpeed;
        decceleration = maxMoveSpeed / timeToFullyStop;


        strafeCoeff.x = (1f + minStrafeFraction) / 2f;
        strafeCoeff.y = (1f - minStrafeFraction) / 2f;        
    }

    void FixedUpdate()
    {
        // Skip condition.
        if (Chronos.Instance.isRewinding || !toggle.Active)
        {
            return;
        }

        // Translational.
        if (input.movementInput.magnitude < Mathf.Epsilon)
        {
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, decceleration * Time.fixedDeltaTime);
        } else
        {
            strafeFraction = strafeCoeff.x + Vector2.Dot(rb.velocity.normalized, transform.up) * strafeCoeff.y;
            Vector2 targetVelocity = input.movementInput.normalized * maxMoveSpeed * Mathf.Pow(slowdownFraction, slowdownPower) * strafeFraction;
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }

        // Translational slowdown.
        slowdownFraction = Mathf.Clamp01(slowdownFraction + Time.fixedDeltaTime / slowDownDuration);            

        // Rotational.    
        Vector2 mouseOffset = input.aimPosition - (Vector2)firepoint.position;
        if ((input.aimPosition - (Vector2)transform.position).magnitude >= deadZone)
        {
            rb.rotation = Mathf.Atan2(mouseOffset.y, mouseOffset.x) * Mathf.Rad2Deg - 90f;
        }
    }

    public void Record()
    {
        moments.Push(new MovementMoment(rb.position, rb.rotation, rb.velocity, slowdownFraction, slowDownDuration));
    }

    public void Rewind()
    {
        MovementMoment moment = moments.Pop();
        rb.position = moment.position;
        rb.rotation = moment.rotation;
        rb.velocity = moment.velocity;
        slowdownFraction = moment.slowdownFraction;
        slowDownDuration = moment.slowDownDuration;
    }

    public void Slowdown(float slowdownFraction, float slowDownDuration)
    {
        this.slowdownFraction = slowdownFraction;
        this.slowDownDuration = slowDownDuration;
    }

    public float MaxSpeedFraction { get => rb.velocity.magnitude / maxMoveSpeed; }
}