using UnityEngine;
using UnityEngine.Audio;

public class Recitation_AudioManager : MonoBehaviour
{
    public static Recitation_AudioManager Instance { get; private set; }

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioSource sfxAudioSource;
    [SerializeField] AudioSource ambienceAudioSource;
    [SerializeField] AudioSource interruptibleAudioSource; // Separate source for sounds that need to be stoppable
    [SerializeField] float transitionDuration = 1f; // Duration in seconds

    public AudioClip amb_normal, amb_chase;


    private float targetHeadphonesVolume = -80f;
    private float targetNormalVolume = 0f;
    private float currentHeadphonesVolume;
    private float currentNormalVolume;
    private bool isTransitioning = false;
    
    public enum AudioMode
    {
        Normal,
        Headphones,
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        audioMixer.GetFloat("HeadphonesVolume", out currentHeadphonesVolume);
        audioMixer.GetFloat("NormalVolume", out currentNormalVolume);
        
        // Set default volumes
        audioMixer.SetFloat("HeadphonesVolume", targetHeadphonesVolume);
        audioMixer.SetFloat("NormalVolume", targetNormalVolume);
        currentHeadphonesVolume = targetHeadphonesVolume;
        currentNormalVolume = targetNormalVolume;
    }

    private void Update()
    {
        if (!isTransitioning) return;
        
        bool headphonesNeedsUpdate = !Mathf.Approximately(currentHeadphonesVolume, targetHeadphonesVolume);
        bool normalNeedsUpdate = !Mathf.Approximately(currentNormalVolume, targetNormalVolume);
        
        // Calculate individual speeds based on their own distances
        float headphonesDistance = Mathf.Abs(targetHeadphonesVolume - currentHeadphonesVolume);
        float normalDistance = Mathf.Abs(targetNormalVolume - currentNormalVolume);
        float headphonesSpeed = headphonesDistance / transitionDuration;
        float normalSpeed = normalDistance / transitionDuration;
        
        // Smoothly interpolate towards target volumes
        if (headphonesNeedsUpdate)
        {
            currentHeadphonesVolume = Mathf.MoveTowards(currentHeadphonesVolume, targetHeadphonesVolume, headphonesSpeed * Time.deltaTime);
            audioMixer.SetFloat("HeadphonesVolume", currentHeadphonesVolume);
        }
        
        if (normalNeedsUpdate)
        {
            currentNormalVolume = Mathf.MoveTowards(currentNormalVolume, targetNormalVolume, normalSpeed * Time.deltaTime);
            audioMixer.SetFloat("NormalVolume", currentNormalVolume);
        }
        
        // Stop updating when both transitions are complete
        if (!headphonesNeedsUpdate && !normalNeedsUpdate)
        {
            isTransitioning = false;
        }
    }

    public void SetAudioMode(AudioMode mode)
    {
        print("Setting audio mode to: " + mode);
        switch(mode)
        {
            default:
            case AudioMode.Normal:
                targetHeadphonesVolume = -80f;
                targetNormalVolume = 0f;
                break;
            case AudioMode.Headphones:
                targetHeadphonesVolume = 0f;
                targetNormalVolume = -80f;
                break;
        }
        
        isTransitioning = true; // Start transitioning
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlayInterruptibleSFX(AudioClip clip)
    {
        if (interruptibleAudioSource == null)
        {
            Debug.LogWarning("Interruptible audio source not assigned!");
            return;
        }
        
        interruptibleAudioSource.Stop();
        interruptibleAudioSource.clip = clip;
        interruptibleAudioSource.Play();
    }

    public void StopInterruptibleSFX()
    {
        if (interruptibleAudioSource != null)
        {
            interruptibleAudioSource.Stop();
        }
    }

    public bool IsInterruptibleSFXPlaying()
    {
        return interruptibleAudioSource != null && interruptibleAudioSource.isPlaying;
    }

    public void ChangeAmbience(AudioClip newAmbience)
    {
        if (ambienceAudioSource.clip == newAmbience) return; // Already playing this ambience

        ambienceAudioSource.clip = newAmbience;
        ambienceAudioSource.loop = true;
        ambienceAudioSource.Play();
    }   
}
