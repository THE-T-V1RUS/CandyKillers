using UnityEngine;

public class AudioVibration : MonoBehaviour
{
    public AudioSource audioSource;
    public Transform vibratingObject, vibratingBaseTransform;
    public float vibrationThreshold = 0.1f;
    public float vibrationAmount = 0.05f;
    public float vibrationSpeed = 20f;
    public AudioHighPassFilter highPassFilter;

    private void Start()
    {
        if (vibratingObject == null)
            vibratingObject = transform;
    }

    private void Update()
    {
        float loudness = GetAudioLoudness();

        if (audioSource.isPlaying && loudness > vibrationThreshold)
        {
            // Generate Perlin noise values for both axes
            float x = (Mathf.PerlinNoise(Time.time * vibrationSpeed, 0f) - 0.5f) * 2f;
            float z = (Mathf.PerlinNoise(0f, Time.time * vibrationSpeed) - 0.5f) * 2f;

            Vector3 offset = new Vector3(x, 0f, z) * vibrationAmount;
            vibratingObject.localPosition = vibratingBaseTransform.localPosition + offset;
        }
        else
        {
            // Smoothly return to original position
            vibratingObject.localPosition = Vector3.Lerp(vibratingObject.localPosition, vibratingBaseTransform.localPosition, Time.deltaTime * 10f);
        }
    }

    private float GetAudioLoudness()
    {
        float[] samples = new float[256];
        audioSource.GetOutputData(samples, 0);
        float sum = 0f;
        foreach (float s in samples)
        {
            sum += s * s;
        }
        return Mathf.Sqrt(sum / samples.Length); // RMS
    }

    public void ChangeVibrationAmount(float newAmount)
    {
        vibrationAmount = newAmount;
    }

    public void ChangeHighpassFilter(bool isHolding)
    {
        if (isHolding)
            highPassFilter.cutoffFrequency = 5000f;
        else
            highPassFilter.cutoffFrequency = 10f;
    }

    public void StopVibrating()
    {
        audioSource.Stop();
        this.enabled = false;
    }
}
