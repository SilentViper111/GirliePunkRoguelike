using UnityEngine;
using TMPro;

/// <summary>
/// Floating damage number popup.
/// Shows damage dealt when hitting enemies.
/// 
/// Reference: KB Section IV - Visual Stack
/// </summary>
public class FloatingDamageNumber : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float randomOffset = 0.5f;

    [Header("Scaling")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float criticalScale = 1.5f;

    private TextMeshPro _text;
    private float _spawnTime;
    private Vector3 _floatDirection;

    private void Awake()
    {
        _text = GetComponent<TextMeshPro>();
        if (_text == null)
            _text = gameObject.AddComponent<TextMeshPro>();

        _spawnTime = Time.time;
        
        // Random horizontal offset
        _floatDirection = Vector3.up + new Vector3(
            Random.Range(-randomOffset, randomOffset),
            0f,
            Random.Range(-randomOffset, randomOffset)
        );
    }

    private void Update()
    {
        float elapsed = Time.time - _spawnTime;

        // Float upward
        transform.position += _floatDirection * floatSpeed * Time.deltaTime;

        // Face camera
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);

        // Fade out
        if (elapsed > lifetime - fadeDuration && _text != null)
        {
            float alpha = 1f - (elapsed - (lifetime - fadeDuration)) / fadeDuration;
            Color c = _text.color;
            c.a = alpha;
            _text.color = c;
        }

        // Destroy
        if (elapsed > lifetime)
            Destroy(gameObject);
    }

    /// <summary>
    /// Initializes the damage number.
    /// </summary>
    public void Setup(float damage, bool isCritical = false)
    {
        if (_text == null)
            _text = GetComponent<TextMeshPro>();

        _text.text = Mathf.RoundToInt(damage).ToString();
        
        if (isCritical)
        {
            _text.color = Color.yellow;
            transform.localScale = Vector3.one * criticalScale;
        }
        else
        {
            _text.color = Color.white;
            transform.localScale = Vector3.one * normalScale;
        }

        _text.alignment = TextAlignmentOptions.Center;
        _text.fontSize = 4f;
    }

    /// <summary>
    /// Static method to spawn a damage number.
    /// </summary>
    public static void Spawn(Vector3 position, float damage, bool isCritical = false)
    {
        GameObject go = new GameObject("DamageNumber");
        go.transform.position = position + Vector3.up * 2f;
        
        FloatingDamageNumber num = go.AddComponent<FloatingDamageNumber>();
        num.Setup(damage, isCritical);
    }
}
