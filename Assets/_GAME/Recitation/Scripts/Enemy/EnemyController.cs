using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Hertzole.GoldPlayer;

public class EnemyController : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Harm,
        Death,
        // Add more states here as needed
    }
    [SerializeField] private AudioSource audioSource_demon, audioSource_footsteps, audioSource_doom;
    [SerializeField] SkinnedMeshRenderer[] enemyRenderers;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolNodes;
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private float waitTimeAtNode = 2f;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float nodeReachDistance = 0.5f;
    [SerializeField] private float animationFadeDuration = 0.1f;
    
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float attackDistance = 0.1f; // Distance to trigger harm state
    
    [Header("Vision Settings")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private float visionAngle = 60f;
    [SerializeField] private float visionHeight = 3f; // Height of the vision cone
    [SerializeField] private LayerMask visionObstacleLayers;
    [SerializeField] private Transform eyePosition; // Optional: where vision starts from
    [SerializeField] private float suspicionThreshold = 1f; // Time needed to observe before chasing
    [SerializeField] private float suspicionIncreaseRate = 1f; // How fast suspicion builds
    [SerializeField] private float suspicionDecreaseRate = 2f; // How fast suspicion fades
    [SerializeField] private float suspicionTimer = 0f; // Current suspicion level (visible in inspector)
    [SerializeField] private float bottomDetectionHeightOffset = 0.5f; // Offset for bottom detection point

    [Header("Material Settings")]
    [ColorUsage(false, true)] [SerializeField] private Color spawnColor = Color.black; // Initial color when enemy spawns
    [ColorUsage(false, true)] [SerializeField] private Color activeColor = new Color(19f/255f, 0f, 0f); // Color after fade-in completes
    [ColorUsage(false, true)] [SerializeField] private Color deathColor = Color.black; // Color to fade to on death

    [Header("Attack Settings")]
    [SerializeField] GameObject attackFX;
    [SerializeField] BookController bookController;
    [SerializeField] AudioClip snd_Attack, snd_Poof;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip snd_Rage, snd_Spotted;

    private EnemyState currentState;
    private NavMeshAgent navAgent;
    private Animator animator;
    
    // Patrol state variables
    private Transform currentTargetNode;
    private bool isWaitingAtNode = false;
    private bool isRotatingToNodeDirection = false;
    private string currentAnimation = "";
    
    // Vision variables
    private Transform detectedPlayer;
    private GoldPlayerController detectedPlayerController;
    
    // Chase state variables
    private bool isRaging = false;

    public bool IsInDeathState()
    {
        return currentState == EnemyState.Death;
    }

    public void SetRageSound(AudioClip newRageSound)
    {
        if (newRageSound != null)
        {
            snd_Rage = newRageSound;
        }
    }

    private void OnEnable()
    {
        // Go to random spawn point
        if (enemySpawnPoints != null && enemySpawnPoints.Length > 0)
        {
            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        // Reset state
        currentState = EnemyState.Patrol;
        suspicionTimer = 0f;
        isWaitingAtNode = false;
        isRotatingToNodeDirection = false;
        isRaging = false;
        detectedPlayer = null;
        detectedPlayerController = null;
        
        // Stop all coroutines (in case re-enabling)
        StopAllCoroutines();
        
        // Reset NavMeshAgent if it exists
        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.ResetPath();
            navAgent.speed = movementSpeed;
            navAgent.updateRotation = true;
        }
        
        // Start all audio sources at volume 0
        if (audioSource_demon != null) audioSource_demon.volume = 0f;
        if (audioSource_footsteps != null) audioSource_footsteps.volume = 0f;
        if (audioSource_doom != null) audioSource_doom.volume = 0f;

        foreach (Renderer renderer in enemyRenderers)
            renderer.enabled = true;
        
        // Set all materials to spawn color
        SetMaterialsColor(spawnColor);
        
        // Fade in audio and color over 2 seconds
        StartCoroutine(FadeInAudio(2f));
        StartCoroutine(FadeInColor(2f));
        
        // Resume patrol if components are initialized
        if (navAgent != null && animator != null)
        {
            StartPatrolState();
        }
    }

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // If no eye position set, use this transform
        if (eyePosition == null)
        {
            eyePosition = transform;
        }
        
        // Configure NavMeshAgent
        navAgent.speed = movementSpeed;
        navAgent.updateRotation = true;
        navAgent.stoppingDistance = nodeReachDistance;
        
        // Start in Patrol state
        currentState = EnemyState.Patrol;
        StartPatrolState();
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrolState();
                break;
            case EnemyState.Chase:
                UpdateChaseState();
                break;
            case EnemyState.Harm:
                UpdateHarmState();
                break;
            case EnemyState.Death:
                // Do nothing - enemy is dead
                break;
        }
    }

    #region Patrol State
    
    private void StartPatrolState()
    {
        if (patrolNodes == null || patrolNodes.Length == 0)
        {
            Debug.LogWarning("No patrol nodes assigned!");
            return;
        }
        
        ChooseRandomNode();
    }

    private void UpdatePatrolState()
    {
        // Check for player in vision and get number of visible points
        int visiblePoints = CountVisiblePlayerPoints();
        
        if (visiblePoints > 0)
        {
            // Check if player is crouching
            bool playerIsCrouching = false;
            if (detectedPlayerController != null)
            {
                playerIsCrouching = detectedPlayerController.Movement.IsCrouching;
            }
            
            if (playerIsCrouching)
            {
                // Use suspicion timer for crouching player
                suspicionTimer += suspicionIncreaseRate * Time.deltaTime;
                suspicionTimer = Mathf.Min(suspicionTimer, suspicionThreshold);
                
                // If we've observed long enough, chase
                if (suspicionTimer >= suspicionThreshold)
                {
                    TransitionToChaseState();
                    return;
                }
            }
            else
            {
                // Instant spot for standing player
                TransitionToChaseState();
                return;
            }
        }
        else
        {
            // Decrease suspicion when we don't see the player
            suspicionTimer -= suspicionDecreaseRate * Time.deltaTime;
            suspicionTimer = Mathf.Max(suspicionTimer, 0f);
        }
        
        if (currentTargetNode == null || isWaitingAtNode) return;

        // Check if we've reached the node
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
            {
                // We've reached the node
                OnReachedNode();
            }
        }

        // Update animation based on movement
        bool isMoving = navAgent.velocity.magnitude > 0.1f;
        UpdateAnimation(isMoving);
    }

    private void ChooseRandomNode()
    {
        if (patrolNodes.Length == 0) return;

        // Choose a random node (optionally different from current)
        Transform newNode;
        if (patrolNodes.Length == 1)
        {
            newNode = patrolNodes[0];
        }
        else
        {
            do
            {
                newNode = patrolNodes[Random.Range(0, patrolNodes.Length)];
            }
            while (newNode == currentTargetNode && patrolNodes.Length > 1);
        }

        currentTargetNode = newNode;
        navAgent.SetDestination(currentTargetNode.position);
        isWaitingAtNode = false;
        isRotatingToNodeDirection = false;
    }

    private void OnReachedNode()
    {
        StartCoroutine(WaitAtNode());
    }

    private IEnumerator WaitAtNode()
    {
        isWaitingAtNode = true;
        UpdateAnimation(false); // Play idle animation
        
        // Rotate to face the node's forward direction
        isRotatingToNodeDirection = true;
        Quaternion targetRotation = currentTargetNode.rotation;
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.rotation = targetRotation;
        isRotatingToNodeDirection = false;
        
        // Wait at the node
        yield return new WaitForSeconds(waitTimeAtNode);
        
        // Choose next node
        ChooseRandomNode();
    }

    #endregion

    #region Chase State
    
    private void TransitionToChaseState()
    {
        currentState = EnemyState.Chase;
        suspicionTimer = 0f; // Reset suspicion timer
        StopAllCoroutines(); // Stop any patrol coroutines
        navAgent.ResetPath(); // Stop moving
        navAgent.speed = chaseSpeed; // Set chase speed
        
        // Start the rage sequence
        StartCoroutine(RageSequence());
    }
    
    private IEnumerator RageSequence()
    {
        audioSource.PlayOneShot(snd_Rage);
        Recitation_AudioManager.Instance.PlaySFX(snd_Spotted);

        isRaging = true;
        
        // Temporarily disable NavMesh rotation control
        navAgent.updateRotation = false;
        
        // Play Rage animation
        currentAnimation = "Rage";
        animator.CrossFade("Rage", animationFadeDuration);
        
        // Keep facing the player for 2 seconds during rage
        float rageTimer = 0f;
        float rageDuration = 2f;
        
        while (rageTimer < rageDuration)
        {
            // Continuously rotate to face the player
            if (detectedPlayer != null)
            {
                Vector3 directionToPlayer = (detectedPlayer.position - transform.position).normalized;
                directionToPlayer.y = 0; // Keep rotation on horizontal plane only
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 2f * Time.deltaTime);
            }
            
            rageTimer += Time.deltaTime;
            yield return null;
        }
        
        isRaging = false;
        
        // Re-enable NavMesh rotation control for chasing
        navAgent.updateRotation = true;
        
        // Switch to Run animation
        Recitation_AudioManager.Instance.ChangeAmbience(Recitation_AudioManager.Instance.amb_chase);
        currentAnimation = "Run";
        animator.CrossFade("Run", animationFadeDuration);
    }
    
    private void UpdateChaseState()
    {
        // Don't move during rage animation
        if (isRaging)
        {
            return;
        }
        
        // Chase the player
        if (detectedPlayer != null)
        {
            // Check distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, detectedPlayer.position);
            
            // If close enough, transition to harm state
            if (distanceToPlayer <= attackDistance)
            {
                TransitionToHarmState();
                return;
            }
            
            navAgent.SetDestination(detectedPlayer.position);
        }
    }
    
    #endregion

    #region Harm State
    
    private void TransitionToHarmState()
    {
        StopAllCoroutines();
        StartCoroutine(TransitionToHarmStateCoroutine());
    }

    private IEnumerator TransitionToHarmStateCoroutine()
    {
        currentState = EnemyState.Harm;
        EquipmentController playerEquipmentController = detectedPlayerController.GetComponent<EquipmentController>();
        playerEquipmentController.InterruptRecording();
        navAgent.ResetPath();
        navAgent.enabled = false;
        playerEquipmentController.blockPlayerInput = true;
        detectedPlayerController.Movement.CanCrouch = false;
        detectedPlayerController.Movement.CanMoveAround = false;
        detectedPlayerController.Camera.CanLookAround = false;
        detectedPlayerController.Camera.ForceLook(attackFX.transform.position);
        Recitation_AudioManager.Instance.PlaySFX(snd_Attack);
        Recitation_AudioManager.Instance.ChangeAmbience(Recitation_AudioManager.Instance.amb_normal);

        // Rotate to face the player
        Vector3 directionToPlayer = (detectedPlayer.position - transform.position).normalized;
                directionToPlayer.y = 0; // Keep rotation on horizontal plane only
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = targetRotation;

        // play rage animation
        currentAnimation = "Rage";
        animator.CrossFade("Rage", animationFadeDuration);

        //play rage sound
        audioSource.PlayOneShot(snd_Rage);

        yield return new WaitForSeconds(1f);
        
        // Enable attack FX
        if (attackFX != null)
        {
            attackFX.SetActive(true);
        }
        audioSource.PlayOneShot(snd_Poof);

        // Instantly change to death color
        SetMaterialsColor(deathColor);
        foreach (Renderer renderer in enemyRenderers)
        {
            renderer.enabled = false;
        }
        
        yield return new WaitForSeconds(1f);

        //move book to attack FX position
        Animator bookAnimator = playerEquipmentController.GetBookAnimator();
        bookAnimator.SetBool("BurnPage", true);
        
        playerEquipmentController.GenerateNextPrayer();

        StartCoroutine(FadeOutAudio(2f));

        yield return new WaitForSeconds(5f);

        //slerp to new position and rotation over 1 second also change slider value to 1
        bookAnimator.SetBool("BurnPage", false);
        detectedPlayerController.Movement.CanCrouch = true;
        detectedPlayerController.Movement.CanMoveAround = true;
        detectedPlayerController.Camera.CanLookAround = true;
        detectedPlayerController.Camera.StopForceLooking();
        playerEquipmentController.blockPlayerInput = false;
        this.gameObject.SetActive(false);
    }
    
    private void UpdateHarmState()
    {
        // Do nothing - harm state is active
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Only process collision if we're in Chase state
        if (currentState == EnemyState.Chase && collision.gameObject.CompareTag("Player"))
        {
            TransitionToHarmState();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Only process trigger if we're in Chase state
        if (currentState == EnemyState.Chase && other.CompareTag("Player"))
        {
            TransitionToHarmState();
        }
    }
    
    #endregion

    #region Death State
    
    public void Die()
    {
        if (currentState == EnemyState.Death) return;
        
        // Stop all coroutines (including rage sequence if running)
        StopAllCoroutines();
        
        Recitation_AudioManager.Instance.PlaySFX(snd_Spotted);
        Recitation_AudioManager.Instance.ChangeAmbience(Recitation_AudioManager.Instance.amb_normal);

        // Transition to death state
        currentState = EnemyState.Death;
        isRaging = false; // Ensure raging flag is cleared
        
        // Start fading out audio
        StartCoroutine(FadeOutAudioAndDie());
    }
    
    private IEnumerator FadeOutAudioAndDie()
    {
        // Stop movement
        navAgent.ResetPath();
        navAgent.enabled = false;
        
        // Play death animation
        currentAnimation = "Death";
        animator.CrossFade("Death", 0.25f);
        
        // Wait for death animation to complete
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(0.25f); // Wait for crossfade
        
        // Get the actual death animation length
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);
        
        // Fade out audio and color over 2 seconds in parallel
        Coroutine audioFade = StartCoroutine(FadeOutAudio(2f));
        Coroutine colorFade = StartCoroutine(FadeOutColor(2f));
        
        yield return audioFade;
        yield return colorFade;
        
        // Disable the game object
        gameObject.SetActive(false);
    }
    
    #endregion

    #region Audio Fade
    
    private IEnumerator FadeInAudio(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (audioSource_demon != null) audioSource_demon.volume = t;
            if (audioSource_footsteps != null) audioSource_footsteps.volume = t;
            if (audioSource_doom != null) audioSource_doom.volume = t;
            
            yield return null;
        }
        
        // Ensure final volume is set
        if (audioSource_demon != null) audioSource_demon.volume = 1f;
        if (audioSource_footsteps != null) audioSource_footsteps.volume = 1f;
        if (audioSource_doom != null) audioSource_doom.volume = 1f;
    }
    
    private IEnumerator FadeOutAudio(float duration)
    {
        float elapsed = 0f;
        float startVolume_demon = audioSource_demon != null ? audioSource_demon.volume : 0f;
        float startVolume_footsteps = audioSource_footsteps != null ? audioSource_footsteps.volume : 0f;
        float startVolume_doom = audioSource_doom != null ? audioSource_doom.volume : 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (audioSource_demon != null) audioSource_demon.volume = Mathf.Lerp(startVolume_demon, 0f, t);
            if (audioSource_footsteps != null) audioSource_footsteps.volume = Mathf.Lerp(startVolume_footsteps, 0f, t);
            if (audioSource_doom != null) audioSource_doom.volume = Mathf.Lerp(startVolume_doom, 0f, t);
            
            yield return null;
        }
        
        // Ensure final volume is set
        if (audioSource_demon != null) audioSource_demon.volume = 0f;
        if (audioSource_footsteps != null) audioSource_footsteps.volume = 0f;
        if (audioSource_doom != null) audioSource_doom.volume = 0f;
    }
    
    #endregion

    #region Material Color Fade
    
    private void SetMaterialsColor(Color color)
    {
        if (enemyRenderers != null)
        {
            foreach (SkinnedMeshRenderer renderer in enemyRenderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetColor("_Color", color);
                }
            }
        }
    }
    
    private IEnumerator FadeInColor(float duration)
    {
        float elapsed = 0f;
        Color startColor = spawnColor;
        Color targetColor = activeColor;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Color currentColor = Color.Lerp(startColor, targetColor, t);
            
            SetMaterialsColor(currentColor);
            
            yield return null;
        }
        
        // Ensure final color is set
        SetMaterialsColor(targetColor);
    }
    
    private IEnumerator FadeOutColor(float duration)
    {
        float elapsed = 0f;
        Color startColor = activeColor;
        
        // Get the actual current color from the first renderer if available
        if (enemyRenderers != null && enemyRenderers.Length > 0 && enemyRenderers[0] != null)
        {
            startColor = enemyRenderers[0].material.GetColor("_Color");
        }
        
        Color targetColor = deathColor;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Color currentColor = Color.Lerp(startColor, targetColor, t);
            
            SetMaterialsColor(currentColor);
            
            yield return null;
        }
        
        // Ensure final color is set
        SetMaterialsColor(targetColor);
    }
    
    #endregion

    #region Vision Detection
    
    private int CountVisiblePlayerPoints()
    {
        int visibleCount = 0;
        
        // Find all colliders with Player tag in range
        Collider[] collidersInRange = Physics.OverlapSphere(eyePosition.position, visionRange);
        
        foreach (Collider collider in collidersInRange)
        {
            if (collider.CompareTag("Player"))
            {
                Transform player = collider.transform;
                
                // Get player height from their collider
                float playerHeight = GetPlayerHeight(collider);
                
                // Check multiple points on the player based on their actual height
                Vector3[] checkPoints = new Vector3[]
                {
                    player.position + Vector3.up * (playerHeight * 0.6f),  // Middle
                    player.position + Vector3.up * (playerHeight * 0.3f + bottomDetectionHeightOffset)   // Lower with offset
                };
                
                foreach (Vector3 checkPoint in checkPoints)
                {
                    Vector3 directionToPoint = (checkPoint - eyePosition.position).normalized;
                    
                    // Lock vision to Y-axis rotation only
                    Vector3 flatForward = eyePosition.forward;
                    flatForward.y = 0;
                    flatForward.Normalize();
                    
                    Vector3 flatDirection = directionToPoint;
                    flatDirection.y = 0;
                    flatDirection.Normalize();
                    
                    // Check if this point is within vision angle
                    float angleToPoint = Vector3.Angle(flatForward, flatDirection);
                    if (angleToPoint <= visionAngle / 2f)
                    {
                        // Check if there's a clear line of sight to this point
                        float distanceToPoint = Vector3.Distance(eyePosition.position, checkPoint);
                        if (!Physics.Raycast(eyePosition.position, directionToPoint, distanceToPoint, visionObstacleLayers))
                        {
                            visibleCount++;
                            detectedPlayer = player;
                            
                            // Cache the player controller reference
                            if (detectedPlayerController == null)
                            {
                                detectedPlayerController = player.GetComponent<GoldPlayerController>();
                            }
                        }
                    }
                }
            }
        }
        
        return visibleCount;
    }
    
    private float GetPlayerHeight(Collider playerCollider)
    {
        // Try to get height from CapsuleCollider
        CapsuleCollider capsule = playerCollider as CapsuleCollider;
        if (capsule != null)
        {
            return capsule.height;
        }
        
        // Try to get height from CharacterController
        CharacterController characterController = playerCollider.GetComponent<CharacterController>();
        if (characterController != null)
        {
            return characterController.height;
        }
        
        // Fallback to bounds height
        return playerCollider.bounds.size.y;
    }
    
    #endregion

    #region Animation Control

    private void UpdateAnimation(bool isMoving)
    {
        string targetAnimation = isMoving ? "Walk" : "Idle";
        
        // Only transition if we're not already playing this animation
        if (currentAnimation != targetAnimation)
        {
            animator.CrossFade(targetAnimation, animationFadeDuration);
            currentAnimation = targetAnimation;
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Draw connections to patrol nodes
        if (patrolNodes != null && patrolNodes.Length > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform node in patrolNodes)
            {
                if (node != null)
                {
                    Gizmos.DrawLine(transform.position, node.position);
                    Gizmos.DrawWireSphere(node.position, 0.3f);
                }
            }
            
            // Highlight current target
            if (currentTargetNode != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentTargetNode.position, 0.5f);
            }
        }
        
        // Draw vision cone - always use this transform's position and rotation
        Vector3 visionPosition = transform.position;
        Vector3 visionForward = transform.forward;
        
        // If eyePosition is assigned, use it instead
        if (eyePosition != null && eyePosition != transform)
        {
            visionPosition = eyePosition.position;
            visionForward = eyePosition.forward;
            
            // Draw a magenta sphere to show where eye position actually is
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(visionPosition, 0.3f);
            Gizmos.DrawLine(transform.position, visionPosition);
        }
        
        // Lock vision cone to Y-axis rotation only
        visionForward.y = 0;
        visionForward.Normalize();
        
        // Draw vision range sphere
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawWireSphere(visionPosition, visionRange);
        
        // Draw vision cone boundary lines (more visible)
        int segments = 20;
        Gizmos.color = Color.yellow;
        
        // Calculate bottom (feet) and top (eye) positions
        Vector3 bottomPosition = transform.position;
        Vector3 topPosition = eyePosition.position;
        Vector3 middlePosition = (bottomPosition + topPosition) / 2f;
        
        // Draw arcs at different heights (top, middle, bottom)
        Vector3[] arcCenters = { topPosition, middlePosition, bottomPosition };
        
        foreach (Vector3 arcCenter in arcCenters)
        {
            // Draw the two edge lines of the cone at this height
            Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * visionForward * visionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * visionForward * visionRange;
            
            Gizmos.DrawLine(arcCenter, arcCenter + leftBoundary);
            Gizmos.DrawLine(arcCenter, arcCenter + rightBoundary);
            
            // Draw arc at the end of the cone at this height
            Vector3 previousPoint = arcCenter + leftBoundary;
            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(-visionAngle / 2f, visionAngle / 2f, i / (float)segments);
                Vector3 direction = Quaternion.Euler(0, angle, 0) * visionForward * visionRange;
                Vector3 point = arcCenter + direction;
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }
        
        // Draw vertical lines connecting the top and bottom arcs at key angles
        int verticalSegments = 5;
        for (int i = 0; i <= verticalSegments; i++)
        {
            float angle = Mathf.Lerp(-visionAngle / 2f, visionAngle / 2f, i / (float)verticalSegments);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * visionForward * visionRange;
            
            Vector3 topPoint = topPosition + direction;
            Vector3 bottomPoint = bottomPosition + direction;
            
            Gizmos.DrawLine(topPoint, bottomPoint);
        }
        
        // Check for objects in range and visualize them
        Collider[] collidersInRange = Physics.OverlapSphere(visionPosition, visionRange);
        
        foreach (Collider collider in collidersInRange)
        {
            if (collider.CompareTag("Player"))
            {
                Transform player = collider.transform;
                
                // Get player height from their collider
                float playerHeight = GetPlayerHeight(collider);
                
                // Check multiple points on the player like in CheckForPlayer()
                Vector3[] checkPoints = new Vector3[]
                {
                    player.position + Vector3.up * (playerHeight * 0.6f),  // Middle
                    player.position + Vector3.up * (playerHeight * 0.3f + bottomDetectionHeightOffset)   // Lower with offset
                };
                
                bool anyPointVisible = false;
                
                foreach (Vector3 checkPoint in checkPoints)
                {
                    Vector3 directionToPoint = (checkPoint - visionPosition).normalized;
                    
                    // Lock to Y-axis rotation for angle check
                    Vector3 flatDirection = directionToPoint;
                    flatDirection.y = 0;
                    flatDirection.Normalize();
                    
                    float angleToPoint = Vector3.Angle(visionForward, flatDirection);
                    
                    if (angleToPoint <= visionAngle / 2f)
                    {
                        float distanceToPoint = Vector3.Distance(visionPosition, checkPoint);
                        bool canSeeThisPoint = !Physics.Raycast(visionPosition, directionToPoint, distanceToPoint, visionObstacleLayers);
                        
                        if (canSeeThisPoint)
                        {
                            anyPointVisible = true;
                            // Draw green line to visible points
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(visionPosition, checkPoint);
                            Gizmos.DrawWireSphere(checkPoint, 0.2f);
                        }
                        else
                        {
                            // Draw red line to occluded points
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(visionPosition, checkPoint);
                            Gizmos.DrawWireSphere(checkPoint, 0.2f);
                        }
                    }
                }
                
                // Draw overall player detection status
                Gizmos.color = anyPointVisible ? Color.green : Color.red;
                Gizmos.DrawWireSphere(player.position, 0.5f);
            }
        }
        
        // Draw line to detected player during play
        if (Application.isPlaying && detectedPlayer != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, detectedPlayer.position);
            Gizmos.DrawWireSphere(detectedPlayer.position, 0.7f);
        }
    }
}
