using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Interface : MonoBehaviour
{
    private static Interface instance;
    public static Interface Instance { get { return instance; } }

    public bool paused { get; private set; }

    public delegate void OnPause();
    public event OnPause onPause;

    public delegate void OnPlay();
    public event OnPlay onPlay;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
            if (paused)
            {
                UnPause();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (paused)
            {
                UnPause();
            } else
            {
                Pause();
            }            
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
    }

    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (paused)
            Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0;
        paused = true;
        if (onPause != null)
        {
            onPause();
        }
    }

    public void UnPause()
    {
        Time.timeScale = 1;
        paused = false;
        if (onPlay != null)
        {
            onPlay();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void FrameFreeze(float howLong)
    {
        StartCoroutine(PauseUnpause(howLong));
    }

    IEnumerator PauseUnpause(float howLong)
    {
        Pause();
        yield return new WaitForSecondsRealtime(howLong);
        UnPause();
    }
}
