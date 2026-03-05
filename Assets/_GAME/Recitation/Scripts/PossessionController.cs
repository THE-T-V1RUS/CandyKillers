using UnityEngine;

public class PossessionController : MonoBehaviour
{
    [SerializeField] PossessedItem[] possessedItems;
    PossessedItem corePossessedItem;
    [SerializeField] AudioClip[] possessionAudioClips;
    [SerializeField] int numberOfPossessions = 1;
    
    [Header("Enemy Timer Settings")]
    [SerializeField] GameObject enemy;
    [SerializeField] float timerDuration = 60f;
    [SerializeField] private float currentTimer;
    [SerializeField] EquipmentController equipmentController;
    
    private bool timerActive = false;
    private bool allItemsCleansed = false;

    private void Start()
    {
        corePossessedItem = GetComponent<PossessedItem>();
        corePossessedItem.enabled = true;

        // Create a shuffled copy of the audio clips to ensure no repeating sounds
        AudioClip[] shuffledClips = new AudioClip[possessionAudioClips.Length];
        System.Array.Copy(possessionAudioClips, shuffledClips, possessionAudioClips.Length);
        
        // Fisher-Yates shuffle
        for (int i = shuffledClips.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            AudioClip temp = shuffledClips[i];
            shuffledClips[i] = shuffledClips[j];
            shuffledClips[j] = temp;
        }
        
        // Assign numberOfPossessions clips to core possessed item
        int coreClipCount = Mathf.Min(numberOfPossessions, shuffledClips.Length);
        AudioClip[] coreClips = new AudioClip[coreClipCount];
        System.Array.Copy(shuffledClips, 0, coreClips, 0, coreClipCount);
        corePossessedItem.SetSoundClips(coreClips);
        
        // Pick random possessed items to distribute the core sounds
        // Create a shuffled list of indices
        int[] itemIndices = new int[possessedItems.Length];
        for (int i = 0; i < itemIndices.Length; i++)
        {
            itemIndices[i] = i;
        }
        
        // Fisher-Yates shuffle of indices
        for (int i = itemIndices.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = itemIndices[i];
            itemIndices[i] = itemIndices[j];
            itemIndices[j] = temp;
        }
        
        // Assign clips to possessed items
        // Start after the core clips to avoid duplicates
        int clipIndex = coreClipCount;
        int coreClipIndex = 0;
        
        for (int i = 0; i < possessedItems.Length; i++)
        {
            int itemIndex = itemIndices[i];
            PossessedItem item = possessedItems[itemIndex];
            
            // First coreClipCount items get one sound each from core clips
            if (coreClipIndex < coreClipCount)
            {
                item.SetSoundClips(new AudioClip[] { coreClips[coreClipIndex] });
                item.SetIsMatchingCore(true);
                item.SetCoreItem(corePossessedItem);
                coreClipIndex++;
            }
            else if (clipIndex < shuffledClips.Length)
            {
                // Remaining items get unique clips
                item.SetSoundClips(new AudioClip[] { shuffledClips[clipIndex] });
                item.SetIsMatchingCore(false);
                clipIndex++;
            }
        }
        
        // Start the timer
        StartTimer();
    }
    
    private void Update()
    {
        if (timerActive && !allItemsCleansed)
        {
            // Only decrease timer when mic equipment is equipped (currentEquipment == 1)
            if (equipmentController != null && equipmentController.GetCurrentEquipment() == 1)
            {
                currentTimer -= Time.deltaTime;
                
                if (currentTimer <= 0f)
                {
                    currentTimer = 0f;
                    timerActive = false;
                    EnableEnemy();
                }
            }
        }
        
        // Check if enemy is inactive and should restart timer
        if (!timerActive && !allItemsCleansed && enemy != null && !enemy.activeSelf)
        {
            StartTimer();
        }
        
        // Check if all items are cleansed
        CheckAllItemsCleansed();
    }
    
    private void StartTimer()
    {
        currentTimer = timerDuration;
        timerActive = true;
    }
    
    private void EnableEnemy()
    {
        if (enemy != null && !allItemsCleansed)
        {
            // Get one of the remaining sounds from the core possessed item
            if (corePossessedItem != null && corePossessedItem.HasAnySoundsRemaining())
            {
                AudioClip remainingSound = corePossessedItem.GetRandomRemainingSound();
                
                if (remainingSound != null)
                {
                    EnemyController enemyController = enemy.GetComponent<EnemyController>();
                    if (enemyController != null)
                    {
                        enemyController.SetRageSound(remainingSound);
                    }
                }
            }
            
            enemy.SetActive(true);
        }
    }
    
    private void CheckAllItemsCleansed()
    {
        if (allItemsCleansed) return;
        
        // Check if core item has no sounds remaining (all matching items cleansed)
        bool coreHasNoSounds = corePossessedItem != null && !corePossessedItem.HasAnySoundsRemaining();
        
        if (coreHasNoSounds)
        {
            allItemsCleansed = true;
            timerActive = false;
            
            // Change core possessed item layer to Interactable
            if (corePossessedItem != null)
            {
                corePossessedItem.gameObject.layer = LayerMask.NameToLayer("Interactable");
            }
            
            // Kill the enemy if active
            if (enemy != null && enemy.activeSelf)
            {
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.Die();
                }
            }
        }
    }
    
    public float GetCurrentTimer()
    {
        return currentTimer;
    }
    
    public bool IsTimerActive()
    {
        return timerActive;
    }
}
