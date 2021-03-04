using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public struct GunMoment
{
    public float timeSinceLastShot;
    public int ammoCount;
    public float recoil;
    public GunMoment(float timeSinceLastShot, int ammoCount, float recoil)
    {
        this.timeSinceLastShot = timeSinceLastShot;
        this.ammoCount = ammoCount;
        this.recoil = recoil;
    }
}

public abstract class Gun : MonoBehaviour, IRewindable
{
    Stack<GunMoment> moments = new Stack<GunMoment>();
    protected Timeline timeline;
    protected CustomInput input;
    protected Rigidbody2D rb;
    protected Toggle toggle;
    protected Transform firePoint;

    [SerializeField] protected Rigidbody2D bulletPrefab;
    [SerializeField] protected float bulletSpeed;

    [SerializeField] protected int ammoTotal;    
    protected int ammoCount;    

    [SerializeField] protected float minShootingPeriod;
    protected float timeSinceLastShot;

    [SerializeField] protected float maxSpreadAngle;
    public float maxAngle { get => maxSpreadAngle; }
    [SerializeField] float spreadIncreaseInMotion;    
    public float motionSpread { get; protected set; }
    [SerializeField] protected float recoilPerShot;
    public float recoil { get; protected set; }    
    [SerializeField] protected float recoilRecoverySpeed;

    Movement movement;
    [SerializeField] float shootingSlowdown;
    [SerializeField] float slowdownDuration;    

    [HideInInspector] public Text ammoCountText;

    public delegate void OnBulletFire();
    public event OnBulletFire onFire;

    [SerializeField] AudioSource nice;
    bool clickPlayed = true;
    

    protected virtual void Start()
    {
        timeline = GetComponent<Timeline>();
        input = GetComponent<CustomInput>();
        rb = GetComponent<Rigidbody2D>();
        toggle = GetComponent<Toggle>();
        firePoint = transform.Find("FirePoint");

        ammoCount = ammoTotal;
        if (ammoCountText)
            SetAmmoCountText();

        movement = GetComponent<Movement>();        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Skip condition.
        if (Chronos.Instance.isRewinding || !toggle.Active)
        {
            return;
        }

        timeSinceLastShot += Time.fixedDeltaTime;
        motionSpread = spreadIncreaseInMotion * movement.MaxSpeedFraction;
        recoil = Mathf.Clamp01(recoil - recoilRecoverySpeed * Time.fixedDeltaTime);
        if (recoil < 0.2f && !clickPlayed)
        {
            if (nice != null)
                nice.Play();
            clickPlayed = true;
        }

        // Fire condition.
        if (input.shoot && ammoCount > 0 && timeSinceLastShot > minShootingPeriod)
        {
            Fire();
            clickPlayed = false;
        }        
    }

    public void Fire()
    {
        PreFire();
        MidFire();
        PostFire();
    }

    public abstract void MidFire();

    void PreFire()
    {        
        timeSinceLastShot = 0;
        ammoCount--;        

        if (onFire != null)
        {
            onFire();
        }

        if (gameObject.CompareTag("Player"))
            ChromaticBoost.Instance.InduceStress(1 - recoil);
    }

    void PostFire()
    {
        recoil = Mathf.Clamp01(recoil + recoilPerShot);
        movement.Slowdown(shootingSlowdown, slowdownDuration);

        if (gameObject.CompareTag("Player"))
        {
            Vector2 shootingDir = input.aimPosition - (Vector2)transform.position;
            DirectionalCameraShake.Instance.InduceDirectionalStress(1f, shootingDir);            
        }

        if (ammoCountText)
            SetAmmoCountText();        
    }

    void SetAmmoCountText()
    {
        if (ammoCount > 99999)
        {
            ammoCountText.text = "∞";
        }
        else
        {
            ammoCountText.text = ammoCount.ToString();
        }
    }

    protected float GetShootingAngle(float fraction)
    {
        float angle = rb.rotation + 90f;
        angle += (2 * Random.value - 1) * Mathf.Clamp01(fraction) * maxSpreadAngle / 2;
        angle *= Mathf.Deg2Rad;
        return angle;
    }

    public void Record()
    {
        moments.Push(new GunMoment(timeSinceLastShot, ammoCount, recoil));
    }

    public void Rewind()
    {
        GunMoment moment = moments.Pop();
        timeSinceLastShot = moment.timeSinceLastShot;
        recoil = moment.recoil;
        ammoCount = moment.ammoCount;
        if (ammoCountText)
            SetAmmoCountText();
    }
}
