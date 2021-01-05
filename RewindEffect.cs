using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RewindEffect : MonoBehaviour
{
    [SerializeField] VideoPlayer vhs;
    Volume volume;
    ColorAdjustments colorAdjustments;
    Bloom bloom;
    float originalBloom;
    [SerializeField] ParticleSystem snow;
    [SerializeField] ParticleSystem reversedSnow;
    [SerializeField] float postExposure;
    [SerializeField] float desaturation;
    [ColorUsageAttribute(true, true)] 
    [SerializeField] Color colorFilterHDR;
    Color originalFilterHDR;

    private void Awake()
    {
        snow.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out bloom);
        originalBloom = bloom.intensity.value;
        originalFilterHDR = colorAdjustments.colorFilter.value;
        Chronos.Instance.onRewindStart += OnStart;
        Chronos.Instance.onRewindStop += OnStop;
    }

    private void OnDisable()
    {
        Chronos.Instance.onRewindStart -= OnStart;
        Chronos.Instance.onRewindStop -= OnStop;
    }

    void OnStart()
    {
        vhs.Play();
        colorAdjustments.saturation.value = -desaturation;
        colorAdjustments.colorFilter.value = colorFilterHDR;
        snow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        reversedSnow.Play();
    }

    void OnStop()
    {
        vhs.Stop();
        colorAdjustments.saturation.value = 0;
        colorAdjustments.colorFilter.value = originalFilterHDR;
        snow.Play();
        reversedSnow.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);        
    }
}
