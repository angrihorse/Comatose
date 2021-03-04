using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticBoost : MonoBehaviour, IRewindable
{
    private static ChromaticBoost instance;
    public static ChromaticBoost Instance { get { return instance; } }

    [SerializeField] Vector2 minMax;
    [SerializeField] float recoverySpeed = 1;
    [SerializeField] float exponent = 1;

    float trauma;

    Volume volume;
    ChromaticAberration chromatic;

    Stack<float> moments = new Stack<float>();

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
        volume.profile.TryGet(out chromatic);
    }

    // Update is called once per frame
    void Update()
    {
        chromatic.intensity.value = Mathf.Max(minMax.x, minMax.y * Mathf.Pow(trauma, exponent));
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
