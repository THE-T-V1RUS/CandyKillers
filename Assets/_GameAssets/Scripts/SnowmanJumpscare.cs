using UnityEngine;

public class SnowmanJumpscare : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject snowmanEyes;
    [SerializeField] private Transform SnowmanTransform;
    [SerializeField] private Transform PlayerTransform;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float activatedRotateSpeed = 5f;
    [SerializeField] private float deactivatedRotateSpeed = 1.5f;
    [SerializeField] private float activatedDuration = 5f;

    private Quaternion startingRotation;
    private float activatedTimer;
    private bool isActivated = false;

    private void Start()
    {
        if (snowmanEyes != null)
            snowmanEyes.SetActive(false);

        startingRotation = SnowmanTransform.rotation;
    }

    private void Update()
    {
        if (isActivated)
        {
            // Smoothly rotate to face the player on Y axis only
            Vector3 direction = PlayerTransform.position - SnowmanTransform.position;
            direction.y = 0f; // lock vertical rotation
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            SnowmanTransform.rotation = Quaternion.Slerp(
                SnowmanTransform.rotation,
                lookRotation,
                Time.deltaTime * activatedRotateSpeed
            );

            // Countdown timer
            activatedTimer -= Time.deltaTime;
            if (activatedTimer <= 0)
            {
                activatedTimer = 0;
                isActivated = false;
                snowmanEyes.SetActive(false);
                animator.SetBool("JumpScare", false);

                if (snowmanEyes != null)
                    snowmanEyes.SetActive(false);
            }
        }
        else
        {
            // Smoothly rotate back to start rotation
            SnowmanTransform.rotation = Quaternion.Slerp(
                SnowmanTransform.rotation,
                startingRotation,
                Time.deltaTime * deactivatedRotateSpeed
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            if (snowmanEyes != null)
                snowmanEyes.SetActive(true);

            if (audioSource != null)
                audioSource.Play();

            activatedTimer = activatedDuration;

            animator.SetBool("JumpScare", true);
        }
    }
}
