using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Centralized sound manager using singleton pattern.
/// Spawns temporary AudioSource objects at world positions for spatial audio.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [Header("Audio Source Template")]
    [Tooltip("Prefab with AudioSource component (will be created at runtime if not assigned)")]
    [SerializeField] private AudioSource audioSourcePrefab;
    
    [Header("Audio Mixer (Optional)")]
    [Tooltip("Optional audio mixer for volume control")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    
    [Header("Global Settings")]
    [Tooltip("Default volume for sound effects")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 1f;
    
    [Tooltip("Whether sounds should play during Time.timeScale = 0")]
    [SerializeField] private bool playDuringPause = true;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Play a sound effect at the specified world position.
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="position">World position to play the sound at</param>
    /// <param name="volume">Volume (0-1), defaults to global default</param>
    /// <returns>The spawned AudioSource (can be used to stop early if needed)</returns>
    public AudioSource PlaySound(AudioClip clip, Vector3 position, float volume = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: Attempted to play null AudioClip");
            return null;
        }
        
        // Use default volume if not specified
        if (volume < 0f) volume = defaultVolume;
        
        // Create audio source object
        GameObject soundObj = new GameObject($"Sound_{clip.name}");
        soundObj.transform.position = position;
        
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f; // 2D sound (no 3D falloff for this game)
        audioSource.playOnAwake = false;
        
        // Assign mixer group if available
        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
        
        // Play using unscaled time if paused
        if (playDuringPause && Time.timeScale == 0f)
        {
            // For paused playback, we need to handle this differently
            // AudioSource.Play() still works during pause
        }
        
        audioSource.Play();
        
        // Schedule destruction after clip finishes
        Destroy(soundObj, clip.length + 0.1f);
        
        return audioSource;
    }
    
    /// <summary>
    /// Play a sound at a transform's position (convenience overload).
    /// </summary>
    public AudioSource PlaySound(AudioClip clip, Transform sourceTransform, float volume = -1f)
    {
        if (sourceTransform == null)
        {
            return PlaySound(clip, Vector3.zero, volume);
        }
        return PlaySound(clip, sourceTransform.position, volume);
    }
    
    /// <summary>
    /// Play a random sound from an array of clips.
    /// Great for variety (e.g., multiple footstep sounds, impact variations).
    /// </summary>
    /// <param name="clips">Array of audio clips to choose from</param>
    /// <param name="position">World position to play the sound at</param>
    /// <param name="volume">Volume (0-1)</param>
    /// <returns>The spawned AudioSource</returns>
    public AudioSource PlayRandomSound(AudioClip[] clips, Vector3 position, float volume = -1f)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("SoundManager: Attempted to play from null or empty clip array");
            return null;
        }
        
        int randomIndex = Random.Range(0, clips.Length);
        return PlaySound(clips[randomIndex], position, volume);
    }
    
    /// <summary>
    /// Play a random sound at a transform's position.
    /// </summary>
    public AudioSource PlayRandomSound(AudioClip[] clips, Transform sourceTransform, float volume = -1f)
    {
        if (sourceTransform == null)
        {
            return PlayRandomSound(clips, Vector3.zero, volume);
        }
        return PlayRandomSound(clips, sourceTransform.position, volume);
    }
    
    /// <summary>
    /// Play a sound with pitch variation for more natural feel.
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="position">World position</param>
    /// <param name="volume">Volume (0-1)</param>
    /// <param name="pitchMin">Minimum pitch (e.g., 0.9)</param>
    /// <param name="pitchMax">Maximum pitch (e.g., 1.1)</param>
    /// <returns>The spawned AudioSource</returns>
    public AudioSource PlaySoundWithPitchVariation(AudioClip clip, Vector3 position, float volume = -1f, float pitchMin = 0.95f, float pitchMax = 1.05f)
    {
        AudioSource source = PlaySound(clip, position, volume);
        if (source != null)
        {
            source.pitch = Random.Range(pitchMin, pitchMax);
        }
        return source;
    }
    
    /// <summary>
    /// Play a UI sound (non-positional, always at listener).
    /// Good for score sounds, button clicks, etc.
    /// </summary>
    public AudioSource PlayUISound(AudioClip clip, float volume = -1f)
    {
        return PlaySound(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, volume);
    }
}
