using UnityEngine;

public class FridgeDoor : MonoBehaviour
{
    [SerializeField] private float openAngle = -120f;
    [SerializeField] private float closedAngle = 0f;
    [SerializeField] private float openCloseDuration = 1f;
    [SerializeField] private Light[] fridgeLights;
    [SerializeField] private AudioSource fridgeAudioSource, fridgeAmbienceAudioSource;
    [SerializeField] private AudioClip snd_FridgeOpen, snd_FridgeClose;
    [SerializeField] private float closeSoundThreshold = 5f; // degrees before fully closed

    private bool isOpen = false;
    private float currentLerpTime = 0f;
    private bool isAnimating = false;

    private float startAngle;
    private float endAngle;
    private bool hasPlayedCloseSound = false;

    private void Update()
    {
        if (isAnimating)
        {
            currentLerpTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentLerpTime / openCloseDuration);

            float angleY = Mathf.Lerp(startAngle, endAngle, t);
            transform.localRotation = Quaternion.Euler(0f, angleY, 0f);

            SetLights(angleY < -50f);

            // Play close sound slightly before door is fully closed
            if (!isOpen && !hasPlayedCloseSound && Mathf.Abs(angleY) <= closeSoundThreshold)
            {
                if (fridgeAudioSource && snd_FridgeClose)
                    fridgeAudioSource.PlayOneShot(snd_FridgeClose);

                hasPlayedCloseSound = true;
            }

            if (t >= 1f)
            {
                isAnimating = false;
                SetLights(endAngle < -100f);
            }
        }

        if (isOpen)
        {
            if (!fridgeAmbienceAudioSource.isPlaying)
                fridgeAmbienceAudioSource.Play();
            else
                if (fridgeAmbienceAudioSource.volume < 0.3) 
                    fridgeAmbienceAudioSource.volume += 1f * Time.deltaTime;
        }
        else
        {
            if (fridgeAmbienceAudioSource.isPlaying)
            {
                fridgeAmbienceAudioSource.volume -= 1f * Time.deltaTime;
                if (fridgeAmbienceAudioSource.volume <= 0f)
                    fridgeAmbienceAudioSource.Stop();
            }
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        startAngle = transform.localEulerAngles.y;
        if (startAngle > 180f)
            startAngle -= 360f;

        endAngle = isOpen ? openAngle : closedAngle;

        currentLerpTime = 0f;
        isAnimating = true;
        hasPlayedCloseSound = false;

        // Play open sound immediately
        if (isOpen && fridgeAudioSource && snd_FridgeOpen)
            fridgeAudioSource.PlayOneShot(snd_FridgeOpen);
    }

    private void SetLights(bool state)
    {
        foreach (Light light in fridgeLights)
        {
            if (light != null)
                light.enabled = state;
        }
    }
}
