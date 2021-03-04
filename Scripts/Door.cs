using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct DoorMoment
{
    public bool entered;
    public float delayed;
    public float rotation;
    public DoorMoment(bool entered, float delayed, float rotation)
    {
        this.entered = entered;
        this.delayed = delayed;
        this.rotation = rotation;
    }
}

public class Door : MonoBehaviour, IRewindable
{
    [SerializeField] float targetRotation;
    [SerializeField] float rotationSpeed;
    [SerializeField] float delayInit;
    [SerializeField] float delayToLoadNewScene;
    float delayed;
    bool entered;

    [SerializeField] string nextScene;
    [SerializeField] Credits credits;

    Stack<DoorMoment> moments = new Stack<DoorMoment>();

    // Update is called once per frame
    void FixedUpdate()
    {
        if (entered)
        {
            delayed += Time.fixedDeltaTime;
        }

        if (delayed > delayInit)
        {
            transform.parent.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(transform.parent.localEulerAngles.z, targetRotation, rotationSpeed * Time.fixedDeltaTime));
         
        } 
        
        if (delayed > delayToLoadNewScene)
        {
            if (nextScene != "End")
            {
                SceneManager.LoadScene(nextScene);
            } else
            {
                credits.PlayCredits();
            }
        }
    }

    void OnTriggerEnter2D()
    {
        entered = true;
    }

    void OnTriggerExit2D()
    {
        entered = false;
        delayed = 0;
    }

    public void Record()
    {
        moments.Push(new DoorMoment(entered, delayed, transform.parent.eulerAngles.z));
    }

    public void Rewind()
    {
        DoorMoment moment = moments.Pop();
        entered = moment.entered;
        delayed = moment.delayed;
        transform.parent.eulerAngles = new Vector3(0, 0, moment.rotation);
    }
}
