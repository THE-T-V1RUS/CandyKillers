using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource sfx_AudioSource;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float roomCompression = -60f;
    [SerializeField] private float maxDoorAngle = 120f;

    [SerializeField] private Transform bedroomDoor;
    [SerializeField] private Transform bathroomDoor;
    [SerializeField] private Transform garageDoor;

    private GameManager gameManager;
    private bool isInitialized;

    private void Start()
    {
        SetCompressorThreshold(0f); // Default to no compression
        Initialize();
    }

    void Initialize()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Invoke(nameof(Initialize), 0.1f); // Retry initialization
            return;
        }

        isInitialized = true;
    }

    public void PlaySFX(AudioClip clip)
    {
        sfx_AudioSource.PlayOneShot(clip);
    }

    private void Update()
    {
        if (!isInitialized) return;

        float compressionValue = 0f;

        switch (gameManager.currentRoom)
        {
            case CurrentRoom.LivingRoom:
                compressionValue = 0f;
                break;

            case CurrentRoom.Bedroom:
                compressionValue = CalculateCompression(bedroomDoor);
                break;

            case CurrentRoom.Bathroom:
                compressionValue = CalculateCompression(bathroomDoor);
                break;

            case CurrentRoom.Garage:
                compressionValue = CalculateCompression(garageDoor);
                break;
        }

        SetCompressorThreshold(compressionValue);
    }

    float CalculateCompression(Transform doorTransform)
    {
        float yAngle = doorTransform.localEulerAngles.y;

        // Normalize to -180..180
        if (yAngle > 180f)
            yAngle -= 360f;

        float openAmount = Mathf.Clamp01(Mathf.Abs(yAngle) / maxDoorAngle);

        float t = 1f - openAmount; // 1 when closed, 0 when fully open

        return Mathf.Lerp(0f, roomCompression, t);
    }

    public void SetCompressorThreshold(float value)
    {
        audioMixer.SetFloat("LivingRoom_CompressorThreshold", value);
    }
}
