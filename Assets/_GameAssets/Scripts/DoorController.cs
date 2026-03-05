using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip snd_open, snd_close;

    public void OpenDoorSound()
    {
        audioSource.PlayOneShot(snd_open);
    }

    public void CloseDoorSound()
    {
        audioSource.PlayOneShot(snd_close);
    }

    public void ToggleDoorAnimation(bool value)
    {
        animator.SetBool("Open", value);
    }
}
