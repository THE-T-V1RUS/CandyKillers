using UnityEngine;

public class SFX_Handler : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    public void PlaySoundEffect(AudioClip clip)
    {
        Recitation_AudioManager.Instance.PlaySFX(clip);
    }

    public void PlayLocalSoundEffect(AudioClip clip)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.pitch = 1f;
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayLocalSoundEffectRandomPitch(AudioClip clip)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.pitch = Random.Range(minPitch, maxPitch);
            sfxAudioSource.PlayOneShot(clip);
        }
    }
}
