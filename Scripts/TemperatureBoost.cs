using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TemperatureBoost : MonoBehaviour, IRewindable
{
    private static TemperatureBoost instance;
    public static TemperatureBoost Instance { get { return instance; } }

    [SerializeField] float maxTemperature;
    [SerializeField] float recoverySpeed = 1;
    [SerializeField] float exponent = 1;

    float trauma;
    Stack<float> moments = new Stack<float>();

    Volume volume;
    WhiteBalance whiteBalance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        volume = GetComponent<Volume>();
        volume.profile.TryGet(out whiteBalance);
    }

    // Update is called once per frame
    void Update()
    {
        whiteBalance.temperature.value = maxTemperature * Mathf.Pow(trauma, exponent);
        if (!Chronos.Instance.isRewinding)
            trauma = Mathf.Clamp01(trauma - recoverySpeed * Time.deltaTime);
    }

    public void InduceStress(float stress)
    {
        trauma = Mathf.Clamp01(trauma + stress);
    }

    public void Record()
    {
        moments.Push(trauma);
    }

    public void Rewind()
    {
        trauma = moments.Pop();
    }
}
