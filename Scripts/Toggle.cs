using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public struct ToggleMoment
{
    public bool active;
    public int lives;
    public ToggleMoment(bool active, int lives)
    {
        this.active = active;
        this.lives = lives;
    }
}

[SelectionBase]
public class Toggle : MonoBehaviour, IRewindable
{
    Stack<ToggleMoment> moments = new Stack<ToggleMoment>();
    Rigidbody2D rb;
    Collider2D coll;
    SpriteRenderer sprite;
    ShadowCaster2D shadowCaster2D;
    Material myMaterial;
    [SerializeField] Material flashMaterial;
    [SerializeField] int lives = 1;
    [SerializeField] float flashTime = 0.05f;
    [SerializeField] AudioSource headshotSound;

    bool active = true;
    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            SetActive(value);
        }
    }    

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        shadowCaster2D = GetComponentInChildren<ShadowCaster2D>();
        myMaterial = sprite.material;
    }

    void Start()
    {        
        SetActive(active);
    }

    public bool Damage()
    {
        lives--;
        StartCoroutine(Flash());
        if (lives <= 0)
        {
            SetActive(false, flashTime);
            if (headshotSound != null)
                headshotSound.Play();
            return true;
        } else
        {
            return false;
        }
    }

    public void SetActive(bool value, float delay = 0)
    {       
        if (active == value)
        {
            return;
        }

        active = value;

        // Set physics.    
        if (coll)
            coll.enabled = value;
        if (!value && rb)
            rb.velocity = Vector2.zero;

        // Set children. 
        StartCoroutine(ToggleChildren(value, delay));    
    }

    IEnumerator Flash()
    {
        sprite.material = flashMaterial;
        yield return new WaitForSecondsRealtime(flashTime);
        sprite.material = myMaterial;
    }

    IEnumerator ToggleChildren(bool value, float delay = 0)
    {
        yield return new WaitForSecondsRealtime(delay);
        sprite.enabled = value;
        if (shadowCaster2D)
            shadowCaster2D.castsShadows = value;
        foreach (Transform child in transform)
        {
            if (!child.GetComponent<ShadowCaster2D>())
                child.gameObject.SetActive(value);
        }
    }

    public IEnumerator DeactivateToggle(float delay)
    {
        yield return new WaitForSeconds(delay);
        Active = false;
    }

    public void Record()
    {
        moments.Push(new ToggleMoment(active, lives));
    }

    public void Rewind()
    {
        ToggleMoment moment = moments.Pop();        
        lives = moment.lives;
        SetActive(moment.active);
    }
}
