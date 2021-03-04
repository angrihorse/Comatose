using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalCameraShake : MonoBehaviour
{
    private static DirectionalCameraShake instance;
    public static DirectionalCameraShake Instance { get { return instance; } }

    [SerializeField] float maxTranslationalShake;
    [SerializeField] float maxAngularShake;
    [SerializeField] float frequency;
    [SerializeField] float traumaExponent;
    [SerializeField] float recoverySpeed;    

    Vector2 dir;
    float shake;
    float seed;
    
    float trauma;

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
        seed = Random.value;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        shake = Mathf.Pow(trauma, traumaExponent);
        TranslationalShake();
        RotationalShake();
        trauma = Mathf.Clamp01(trauma - recoverySpeed * Time.fixedDeltaTime);
    }

    void TranslationalShake()
    {
        transform.localPosition = new Vector3(
            dir.x * maxTranslationalShake * Mathf.PerlinNoise(seed, Time.time * frequency),
            dir.y * maxTranslationalShake * Mathf.PerlinNoise(seed + 1, Time.time * frequency), 0) * shake;
    }

    void RotationalShake()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, maxAngularShake * (Mathf.PerlinNoise(seed + 5, Time.time * frequency) * 2 - 1)) * shake);
    }

    public void InduceDirectionalStress(float stress, Vector2 offset)
    {
        trauma = Mathf.Clamp01(trauma + stress);
        dir = -offset.normalized;
    }
}
