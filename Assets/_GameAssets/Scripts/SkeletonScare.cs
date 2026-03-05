using UnityEngine;

public class SkeletonScare : MonoBehaviour
{
    public Animator scareAnimator;
    public AudioSource scareAudio;
    public float delay = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && delay == 0)
        {
            scareAnimator.SetTrigger("Scare");
            scareAudio.Play();
            delay = 3f;
        }
    }

    private void Update()
    {
        if(delay > 0)
        {
            delay = Mathf.Clamp(delay - Time.deltaTime, 0, 100);
        }
    }
}
