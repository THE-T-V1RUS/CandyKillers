using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GarageDoorBlocker : MonoBehaviour
{
    public GameManager gameManager;
    public Rigidbody rb;
    public bool isBlocking;

    public void ToggleBlocker(bool shouldBlock)
    {
        isBlocking = shouldBlock;
        rb.isKinematic = isBlocking;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isBlocking) return;

        if (other.CompareTag("Player"))
        {
            List<string> dialogue = new List<string>();
            dialogue.Add("Not yet...");
            gameManager.dialogueText.StartTeletype(dialogue);
        }
    }
}
