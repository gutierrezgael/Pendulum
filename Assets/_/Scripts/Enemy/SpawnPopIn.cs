using UnityEngine;

public class SpawnPopIn : MonoBehaviour
{
    [Tooltip("Seconds the pop-in takes")]
    public float duration = 0.35f;

    [Tooltip("Lights under the object (the enemy's spotlight) fade in too")]
    public bool fadeInLights = true;

    [Tooltip("The enemy doesn't move, shoot or hurt while it is popping in (EnemyChaser and EnemyShooter check this)")]
    public bool blockActions = true;

    Transform[] visuals;
    Vector3[] baseScales;
    Light[] lights;
    float[] lightIntensities;
    float timer;

    public bool IsSpawning => blockActions && timer < duration;

    void Awake()
    {
        // Only child sprites: scaling the root would also scale the colliders
        var list = new System.Collections.Generic.List<Transform>();
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>(true))
            if (sr.transform != transform) list.Add(sr.transform);

        visuals = list.ToArray();
        baseScales = new Vector3[visuals.Length];
        for (int i = 0; i < visuals.Length; i++) baseScales[i] = visuals[i].localScale;

        lights = GetComponentsInChildren<Light>(true);
        lightIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++) lightIntensities[i] = lights[i].intensity;
    }

    // Runs every time the object is switched on (a room wakes an enemy up)
    void OnEnable()
    {
        timer = 0f;
        Apply(0f);
    }

    void Update()
    {
        if (timer >= duration) return;

        timer += Time.deltaTime;
        float k = duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
        k = 1f - (1f - k) * (1f - k); // fast then slow, like the items
        Apply(k);
    }

    void Apply(float k)
    {
        for (int i = 0; i < visuals.Length; i++)
            if (visuals[i] != null) visuals[i].localScale = baseScales[i] * k;

        if (!fadeInLights) return;
        for (int i = 0; i < lights.Length; i++)
            if (lights[i] != null) lights[i].intensity = lightIntensities[i] * k;
    }
}