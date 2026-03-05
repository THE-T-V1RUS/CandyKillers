using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PossessedItem : MonoBehaviour
{
    [System.Serializable]
    private class SoundPlayer
    {
        public AudioSource audioSource;
        public AudioClip clip;
        public float nextSoundTime;
    }

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioMixerGroup headphonesMixerGroup;
    [SerializeField] float maxDetectionDistance = 20f;
    [SerializeField] float maxVolume = 1f;
    [SerializeField] AnimationCurve distanceFalloff = AnimationCurve.Linear(0, 1, 1, 0);
    [SerializeField] float directionalFocus = 2f; // Higher = need to aim more precisely
    [SerializeField] bool isCleansed = false;
    
    [Header("Audio Playback")]
    [SerializeField] AudioClip[] possessedSounds;
    [SerializeField] float minTimeBetweenSounds = 3f;
    [SerializeField] float maxTimeBetweenSounds = 10f;
    
    [SerializeField] private bool isMatchingCore = false;

    [SerializeField] private ParticleSystem cleanseEffect;
    [SerializeField] private float blendOverlayDuration = 1f;

    private Transform playerCamera;
    private List<SoundPlayer> soundPlayers = new List<SoundPlayer>();
    private PossessedItem coreItem;
    private AudioClip assignedClip;
    private MeshRenderer meshRenderer;
    private Material uniqueMaterial;
    
    private void Awake()
    {
        // Cache camera reference once
        playerCamera = Camera.main.transform;
        
        // Get mesh renderer and create unique material instance
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Create a unique copy of the material for this instance
            uniqueMaterial = new Material(meshRenderer.sharedMaterial);
            meshRenderer.material = uniqueMaterial;
        }
        
        // Disable until sounds are assigned
        enabled = false;
    }
    
    void InitializeSoundPlayers()
    {
        // Clean up old audio sources
        foreach (SoundPlayer player in soundPlayers)
        {
            if (player.audioSource != null)
            {
                Destroy(player.audioSource);
            }
        }
        soundPlayers.Clear();
        
        if (possessedSounds == null || possessedSounds.Length == 0)
        {
            return;
        }
        
        foreach (AudioClip clip in possessedSounds)
        {
            // Create a new audio source for this clip
            AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
            
            // Route audio to headphones channel
            if (headphonesMixerGroup != null)
            {
                newAudioSource.outputAudioMixerGroup = headphonesMixerGroup;
            }
            
            // Configure AudioSource
            newAudioSource.loop = false;
            newAudioSource.playOnAwake = false;
            newAudioSource.spatialBlend = 0f; // 2D sound since we're manually controlling volume
            newAudioSource.volume = 0f;
            newAudioSource.clip = clip;
            
            // Create sound player
            SoundPlayer player = new SoundPlayer();
            player.audioSource = newAudioSource;
            player.clip = clip;
            player.nextSoundTime = Time.time + Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
            
            soundPlayers.Add(player);
        }
    }
    
    private void Update()
    {
        if (isCleansed) return;
        
        UpdateAllVolumes();
        CheckPlayAllSounds();
    }
    
    void UpdateAllVolumes()
    {
        if (playerCamera == null)
        {
            return;
        }
        
        // Only calculate once per frame if any sound is playing
        bool anySoundPlaying = false;
        foreach (SoundPlayer player in soundPlayers)
        {
            if (player.audioSource.isPlaying)
            {
                anySoundPlaying = true;
                break;
            }
        }
        
        if (!anySoundPlaying) return;
        
        // Calculate factors once for all sounds
        float distance = Vector3.Distance(playerCamera.position, transform.position);
        float normalizedDistance = Mathf.Clamp01(distance / maxDetectionDistance);
        float distanceFactor = distanceFalloff.Evaluate(normalizedDistance);
        
        Vector3 directionToObject = (transform.position - playerCamera.position).normalized;
        float dotProduct = Vector3.Dot(playerCamera.forward, directionToObject);
        float directionalFactor = Mathf.Pow(Mathf.Clamp01((dotProduct + 1f) * 0.5f), directionalFocus);
        
        float finalVolume = distanceFactor * directionalFactor * maxVolume;
        
        // Apply to all playing sounds
        foreach (SoundPlayer player in soundPlayers)
        {
            if (player.audioSource.isPlaying)
            {
                player.audioSource.volume = finalVolume;
            }
        }
    }
    
    void CheckPlayAllSounds()
    {
        foreach (SoundPlayer player in soundPlayers)
        {
            if (Time.time >= player.nextSoundTime && !player.audioSource.isPlaying)
            {
                player.audioSource.Play();
                player.nextSoundTime = Time.time + Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
            }
        }
    }
    
    // Visualize detection range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDetectionDistance);
    }

    public void SetSoundClips(AudioClip[] clips)
    {
        possessedSounds = clips;
        if (clips != null && clips.Length > 0)
        {
            assignedClip = clips[0];
        }
        InitializeSoundPlayers();
        enabled = true;
    }
    
    public void SetIsMatchingCore(bool isMatching)
    {
        isMatchingCore = isMatching;
        if(isMatchingCore)
            cleanseEffect.gameObject.SetActive(true);
    }
    
    public bool IsMatchingCore()
    {
        return isMatchingCore;
    }
    
    public void SetCoreItem(PossessedItem core)
    {
        coreItem = core;
    }
    
    public void RemoveSoundClip(AudioClip clipToRemove)
    {
        if (clipToRemove == null) return;
        
        // Find and remove the sound player with this clip
        for (int i = soundPlayers.Count - 1; i >= 0; i--)
        {
            if (soundPlayers[i].clip == clipToRemove)
            {
                // Stop and destroy the audio source
                if (soundPlayers[i].audioSource != null)
                {
                    soundPlayers[i].audioSource.Stop();
                    Destroy(soundPlayers[i].audioSource);
                }
                soundPlayers.RemoveAt(i);
                print("Removed sound clip: " + clipToRemove.name);
            }
        }
    }

    public void Cleanse()
    {
        isCleansed = true;
        StartCoroutine(DelayClenseEffect());
        StartCoroutine(BlendOverlayTransition());
        
        // Stop all audio sources
        foreach (SoundPlayer player in soundPlayers)
        {
            player.audioSource.Stop();
        }
        
        // If this is a matching core item, remove the sound from the core
        if (isMatchingCore && coreItem != null && assignedClip != null)
        {
            coreItem.RemoveSoundClip(assignedClip);
        }
        
        gameObject.tag = "Untagged";
        print("Item cleansed");
    }

    IEnumerator DelayClenseEffect()
    {
        yield return new WaitForSeconds(0.5f);
        cleanseEffect.Play();
    }
    
    IEnumerator BlendOverlayTransition()
    {
        if (uniqueMaterial == null) yield break;
        
        float elapsed = 0f;
        float startValue = uniqueMaterial.GetFloat("_OverlayBlend");
        
        while (elapsed < blendOverlayDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blendOverlayDuration;
            uniqueMaterial.SetFloat("_OverlayBlend", Mathf.Lerp(startValue, 1f, t));
            yield return null;
        }
        
        // Ensure final value is set
        uniqueMaterial.SetFloat("_OverlayBlend", 1f);
    }
    
    public bool HasAnySoundsRemaining()
    {
        return soundPlayers != null && soundPlayers.Count > 0;
    }
    
    public AudioClip GetRandomRemainingSound()
    {
        if (soundPlayers == null || soundPlayers.Count == 0)
        {
            return null;
        }
        
        // Get a random sound player from the remaining ones
        int randomIndex = Random.Range(0, soundPlayers.Count);
        return soundPlayers[randomIndex].clip;
    }
}
