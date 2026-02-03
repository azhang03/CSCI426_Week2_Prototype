using UnityEngine;
using System.Collections;

/// <summary>
/// Simple camera shake effect. Attach to the Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    private Vector3 originalPosition;
    private bool isShaking = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Trigger a brief impact shake.
    /// </summary>
    /// <param name="intensity">How far the camera moves (in units)</param>
    /// <param name="duration">How long the shake lasts (in seconds)</param>
    /// <param name="useUnscaledTime">If true, shake works even when Time.timeScale = 0</param>
    public void Shake(float intensity = 0.3f, float duration = 0.15f, bool useUnscaledTime = true)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(intensity, duration, useUnscaledTime));
        }
    }

    /// <summary>
    /// Trigger a death impact shake (preset for strong, brief impact).
    /// </summary>
    public void DeathShake()
    {
        Shake(0.4f, 0.12f, true);
    }

    /// <summary>
    /// Trigger a hit shake (preset for lighter impact).
    /// </summary>
    public void HitShake()
    {
        Shake(0.2f, 0.1f, false);
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration, bool useUnscaledTime)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Random offset within intensity range
            float offsetX = Random.Range(-intensity, intensity);
            float offsetY = Random.Range(-intensity, intensity);

            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        // Return to original position
        transform.localPosition = originalPosition;
        isShaking = false;
    }

    /// <summary>
    /// Call this if the camera moves (e.g., follows player) to update the "original" position.
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (!isShaking)
        {
            originalPosition = transform.localPosition;
        }
    }
}
