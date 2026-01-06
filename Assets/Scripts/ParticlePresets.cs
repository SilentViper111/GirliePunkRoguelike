using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Particle effect presets for runtime creation.
/// Creates particle systems with cyberpunk neon aesthetics.
/// 
/// Reference: KB Section IV - Visual Stack
/// </summary>
public class ParticlePresets : MonoBehaviour
{
    public static ParticlePresets Instance { get; private set; }

    [Header("Colors")]
    [SerializeField] private Color neonPink = new Color(1f, 0f, 0.8f);
    [SerializeField] private Color neonCyan = new Color(0f, 1f, 1f);
    [SerializeField] private Color neonYellow = new Color(1f, 1f, 0f);
    [SerializeField] private Color neonPurple = new Color(0.6f, 0f, 1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Creates a muzzle flash particle system.
    /// </summary>
    public ParticleSystem CreateMuzzleFlash(Transform parent)
    {
        GameObject go = new GameObject("MuzzleFlash");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.forward * 0.5f;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.3f;
        main.startLifetime = 0.1f;
        main.startColor = neonCyan;
        main.maxParticles = 10;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 5)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(neonCyan);

        return ps;
    }

    /// <summary>
    /// Creates impact sparks particle system.
    /// </summary>
    public ParticleSystem CreateImpactSparks()
    {
        GameObject go = new GameObject("ImpactSparks");

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startColor = neonYellow;
        main.maxParticles = 30;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.1f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(neonYellow);

        return ps;
    }

    /// <summary>
    /// Creates explosion particle system.
    /// </summary>
    public ParticleSystem CreateExplosion()
    {
        GameObject go = new GameObject("Explosion");

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
        main.startColor = new ParticleSystem.MinMaxGradient(neonPink, neonPurple);
        main.maxParticles = 100;
        main.playOnAwake = false;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 50)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(neonPink, 0f),
                new GradientColorKey(neonPurple, 0.5f),
                new GradientColorKey(Color.black, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.5f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(neonPink);

        return ps;
    }

    /// <summary>
    /// Creates dash trail particle system.
    /// </summary>
    public ParticleSystem CreateDashTrail(Transform parent)
    {
        GameObject go = new GameObject("DashTrail");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startLifetime = 0.3f;
        main.startColor = neonCyan;
        main.maxParticles = 50;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 100;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(neonCyan);

        return ps;
    }

    private Material CreateParticleMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetColor("_Color", color);
        mat.SetFloat("_InvFade", 2f);
        mat.renderQueue = 3000;
        return mat;
    }
}
