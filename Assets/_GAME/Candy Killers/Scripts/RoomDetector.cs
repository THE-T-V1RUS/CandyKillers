using UnityEngine;

public class RoomDetector : MonoBehaviour
{
    GameManager gameManager;
    bool isInitialized = false;
    bool inRoom = false;

    public CurrentRoom room;
    public BoxCollider boxCollider;
    public DoorAudioController door;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Initialize();
            return;
        }
        
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = true;
        isInitialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRoom = true;
        gameManager.currentRoom = room;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRoom = false;
        gameManager.currentRoom = CurrentRoom.LivingRoom;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRoom = true;
        gameManager.currentRoom = room;
    }
}
