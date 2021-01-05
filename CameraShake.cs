using UnityEngine;

// Reference: https://roystan.net/articles/camera-shake.html
public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    public static CameraShake Instance { get { return instance; } }

    [SerializeField] Vector3 maximumTranslationShake = Vector3.one;
    [SerializeField] Vector3 maximumAngularShake = Vector3.one * 15;
	[SerializeField] float frequency = 25;
	[SerializeField] float traumaExponent = 1;
	[SerializeField] float recoverySpeed = 1;

    float trauma;
    float seed;
    float shake;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            instance = this;
        }
        seed = Random.value;
    }

    void FixedUpdate()
    {
        shake = Mathf.Pow(trauma, traumaExponent);
        TranslationalShake();
        RotationalShake();
        trauma = Mathf.Clamp01(trauma - recoverySpeed * Time.deltaTime);
    }

    void TranslationalShake()
    {
        transform.localPosition = new Vector3(
            maximumTranslationShake.x * (Mathf.PerlinNoise(seed, Time.time * frequency) * 2 - 1),
            maximumTranslationShake.y * (Mathf.PerlinNoise(seed + 1, Time.time * frequency) * 2 - 1),
            maximumTranslationShake.z * (Mathf.PerlinNoise(seed + 2, Time.time * frequency) * 2 - 1)
        ) * shake;
    }

    void RotationalShake()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(
            maximumAngularShake.x * (Mathf.PerlinNoise(seed + 3, Time.time * frequency) * 2 - 1),
            maximumAngularShake.y * (Mathf.PerlinNoise(seed + 4, Time.time * frequency) * 2 - 1),
            maximumAngularShake.z * (Mathf.PerlinNoise(seed + 5, Time.time * frequency) * 2 - 1)
        ) * shake);
    }

    public void InduceStress(float stress)
    {
        trauma = Mathf.Clamp01(trauma + stress);
    }    
}
