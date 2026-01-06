using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tetris-style enemy crumbling effect.
/// When enemy dies, spawns cube chunks that tumble and fade away.
/// 
/// Reference: KB Section IV.C - Tetris Crumbling Enemies
/// </summary>
public class TetrisCrumbleEffect : MonoBehaviour
{
    [Header("Crumble Settings")]
    [SerializeField] private int cubesPerAxis = 3;
    [SerializeField] private float cubeSize = 0.5f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float upwardsBias = 0.5f;
    [SerializeField] private float fadeDelay = 1f;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Visual")]
    [SerializeField] private Material crumbleMaterial;
    [SerializeField] private bool inheritColor = true;
    
    private List<GameObject> _cubes = new List<GameObject>();
    private float _crumbleStartTime;
    private bool _isCrumbling;

    /// <summary>
    /// Triggers the crumble effect, replacing the object with cubes.
    /// </summary>
    public void Crumble()
    {
        if (_isCrumbling) return;
        _isCrumbling = true;
        _crumbleStartTime = Time.time;
        
        // Get base color
        Color baseColor = Color.magenta;
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null && inheritColor)
            baseColor = rend.material.color;
        
        // Hide original mesh
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
            
        // Get bounds
        Bounds bounds = new Bounds(transform.position, Vector3.one * 2f);
        Collider col = GetComponent<Collider>();
        if (col != null)
            bounds = col.bounds;
        
        // Spawn cubes
        Vector3 start = bounds.min;
        float stepSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / cubesPerAxis;
        
        for (int x = 0; x < cubesPerAxis; x++)
        {
            for (int y = 0; y < cubesPerAxis; y++)
            {
                for (int z = 0; z < cubesPerAxis; z++)
                {
                    Vector3 pos = start + new Vector3(
                        (x + 0.5f) * stepSize,
                        (y + 0.5f) * stepSize,
                        (z + 0.5f) * stepSize
                    );
                    
                    SpawnCube(pos, stepSize * 0.9f, baseColor);
                }
            }
        }
        
        Debug.Log($"[TetrisCrumble] Spawned {_cubes.Count} cubes");
        
        // Start cleanup
        StartCoroutine(FadeAndDestroy());
    }

    private void SpawnCube(Vector3 position, float size, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * size;
        cube.transform.rotation = Random.rotation;
        cube.name = "CrumbleCube";
        
        // Material
        Renderer rend = cube.GetComponent<Renderer>();
        if (crumbleMaterial != null)
        {
            rend.material = new Material(crumbleMaterial);
        }
        rend.material.color = color;
        
        // Physics
        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.mass = 0.1f;
        rb.useGravity = false; // We use sphere gravity
        
        // Apply explosive force
        Vector3 explosionDir = (position - transform.position).normalized;
        explosionDir += Vector3.up * upwardsBias;
        rb.AddForce(explosionDir * explosionForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
        
        // Custom gravity component
        CrumbleCubeGravity gravity = cube.AddComponent<CrumbleCubeGravity>();
        
        _cubes.Add(cube);
    }

    private System.Collections.IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(fadeDelay);
        
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);
            
            foreach (var cube in _cubes)
            {
                if (cube != null)
                {
                    Renderer rend = cube.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        Color c = rend.material.color;
                        c.a = alpha;
                        rend.material.color = c;
                    }
                    
                    // Shrink as well
                    cube.transform.localScale *= 0.98f;
                }
            }
            
            yield return null;
        }
        
        // Destroy all cubes
        foreach (var cube in _cubes)
        {
            if (cube != null)
                Destroy(cube);
        }
        
        _cubes.Clear();
        
        // Finally destroy self
        Destroy(gameObject);
    }

    /// <summary>
    /// Static method to apply crumble effect to any GameObject.
    /// </summary>
    public static void ApplyCrumbleEffect(GameObject target)
    {
        TetrisCrumbleEffect effect = target.GetComponent<TetrisCrumbleEffect>();
        if (effect == null)
            effect = target.AddComponent<TetrisCrumbleEffect>();
        effect.Crumble();
    }
}

/// <summary>
/// Simple gravity toward world center for crumble cubes.
/// </summary>
public class CrumbleCubeGravity : MonoBehaviour
{
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;
        
        Vector3 gravityDir = -transform.position.normalized;
        _rb.AddForce(gravityDir * 15f, ForceMode.Acceleration);
    }
}
