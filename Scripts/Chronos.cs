using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chronos : MonoBehaviour
{
    private static Chronos instance;
    public static Chronos Instance { get { return instance; } }

    public bool isRewinding { get; private set; }
    bool wasRewinding;

    Toggle playerToggle;
    public bool playerActive { get; private set; }

    public int count { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }        
    }

    void Start()
    {
        playerToggle = GameObject.FindWithTag("Player").GetComponent<Toggle>();
        playerActive = playerToggle.Active;
    }

    public delegate void OnRewindStart();
    public event OnRewindStart onRewindStart;

    public delegate void OnRewindStop();
    public event OnRewindStop onRewindStop;

    public delegate void OnPlayerDeath();
    public event OnPlayerDeath onPlayerDeath;

    // Update is called once per frame
    void Update()
    {
        isRewinding = Input.GetAxisRaw("Rewind") > 0;

        if (isRewinding)
        {
            count--;
            if (count == 0)
            {
                isRewinding = false;
            }
        } else
        {
            count++;
        }

        if (isRewinding && !wasRewinding && onRewindStart != null)
        {
            if (onRewindStart != null)
            {
                onRewindStart();
            }
            
            if (!playerActive)
                Interface.Instance.UnPause();                
        }            
        if (!isRewinding && wasRewinding && onRewindStop != null)
        {
            if (onRewindStop != null)
            {
                onRewindStop();
            }
        }            

        wasRewinding = isRewinding;

        // Pause if player dies.
        if (playerActive && !playerToggle.Active)
        {            
            Interface.Instance.Pause();
            if (onPlayerDeath != null)
                onPlayerDeath();
        }
            
        playerActive = playerToggle.Active;
    }
}
