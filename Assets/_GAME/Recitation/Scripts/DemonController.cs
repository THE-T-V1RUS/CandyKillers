using UnityEngine;

public class DemonController : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    private bool isMoving = false;
    [SerializeField] private Animator animator;

    public void StartMovingForwardAtSetSpeed()
    {
        isMoving = true;
        animator.SetBool("walk", true);
    }
    
    public void StopMoving()
    {
        isMoving = false;
        animator.SetBool("walk", false);
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
}   