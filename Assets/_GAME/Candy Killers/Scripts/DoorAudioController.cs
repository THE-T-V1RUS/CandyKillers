using UnityEngine;

public class DoorAudioController : MonoBehaviour
{
    [SerializeField] AudioClip Snd_DoorOpen, Snd_DoorClose;
    [SerializeField] float closedThreshold;
    public bool isClosed = true;
    [SerializeField] bool isLocked = false;
    [SerializeField] float rot;
    AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        rot = Mathf.Abs(transform.localRotation.y) * 100;

        if (isClosed)
        {
            if( rot > closedThreshold)
            {
                audioSource.pitch = Random.Range(0.8f, 1f);
                audioSource.PlayOneShot(Snd_DoorOpen);
                isClosed = false;
            }
        }
        else
        {
            if(rot < closedThreshold)
            {
                audioSource.pitch = Random.Range(0.8f, 1f);
                audioSource.PlayOneShot(Snd_DoorClose);
                isClosed = true;
            }
        }
    }
}
