using UnityEngine;

public class CandyInfo : MonoBehaviour
{
    public int candyID = 0;
    public bool isPoisoned = false;
    public GameObject candyCollider;
    public Interactable candyInteractable;

    private void Start()
    {
        if(candyCollider != null)
            candyInteractable = candyCollider.GetComponent<Interactable>();
    }
}
