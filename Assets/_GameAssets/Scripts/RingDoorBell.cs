using Hertzole.GoldPlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RingDoorBell : MonoBehaviour
{
    [System.Serializable]
    public class Dialogue
    {
        public List<string> textList = new List<string>();
    }

    [Header("References")]
    public CoreGameManager gameManager;
    public GoldPlayerController playerController;
    public DoorController doorController;
    public Animator animator;
    public Rig RightHandRig;
    public Transform standLoacation, playerTransform, lookTarget;
    public Transform adultTransform, handTargetTransform, dropPositionTransform;
    public Transform candyTransform;
    public Bucket bucket;
    public AudioSource DoorBellAudio;
    public AudioClip snd_grabCandy, snd_dropCandy, snd_WalkToDoor, snd_turnOffLight;
    public float StartingDistance;
    public List<AudioClip> sndCharacterClips;
    public List<AudioClip> sndKidClips;
    public CandyInfo candyInfo;
    public Color candyColor;

    public GameObject lightOn_obj, lightOff_obj;

    [Header("Dialogue Settings")]
    public int houseNumber = 0;
    public bool isGood = false;
    public string reactToDoorbellText, goodbyeText;
    public List<Dialogue> goodDialogue;
    public List<Dialogue> badDialogue;
    public float vocalPitch_min, vocalPitch_max;

    [Header("Movement Settings")]
    public float moveDuration = 1.5f;
    public float targetDistance = 0.2f;

    [Header("Hand Animation Settings")]
    public float rigBlendDuration = 1f;
    public float handMoveDuration = 1f;
    public float returnDelay = 0.5f;

    private float starting_xPos;
    private float distance;

    private void Start()
    {
        starting_xPos = adultTransform.localPosition.x;
    }

    private void Update()
    {
        distance = Vector3.Distance(adultTransform.position, playerTransform.position);

        adultTransform.localPosition = new Vector3(starting_xPos, adultTransform.localPosition.y, adultTransform.localPosition.z);
    }

    public void StartInteraction()
    {
        StartCoroutine(GetCandyCoroutine());
    }

    private void SayDialogue()
    {
        if (isGood)
            gameManager.dialogueController.ShowSubtitles(goodDialogue[gameManager.randomHouseLines[houseNumber]].textList, false);
        else
            gameManager.dialogueController.ShowSubtitles(badDialogue[gameManager.randomHouseLines[houseNumber]].textList, false);
    }

    private IEnumerator GetCandyCoroutine()
    {
        // --- Setup ---
        gameManager.TogglePlayerMovement(false);
        gameManager.TogglePlayerRotation(false);
        playerController.Camera.ForceLook(lookTarget, 5f);

        // --- Doorbell ---
        DoorBellAudio.Play();

        // Move player into position
        Vector3 startPos = playerTransform.position;
        Quaternion startRot = playerTransform.rotation;
        Vector3 endPos = standLoacation.position;
        Quaternion endRot = Quaternion.Euler(0f, standLoacation.eulerAngles.y, 0f);

        float moveTimer = 0f;
        while (moveTimer < moveDuration)
        {
            moveTimer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, moveTimer / moveDuration);
            playerTransform.position = Vector3.Lerp(startPos, endPos, t);
            playerTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        playerTransform.position = endPos;
        playerTransform.rotation = endRot;

        // React to doorbell
        gameManager.dialogueController.minPitch = vocalPitch_min;
        gameManager.dialogueController.maxPitch = vocalPitch_max;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(sndCharacterClips);
        gameManager.dialogueController.ShowSubtitles(reactToDoorbellText, false);

        yield return new WaitForSeconds(0.25f);
        yield return new WaitUntil(() => !gameManager.dialogueController.isShowingSubtitles);
        yield return new WaitForSeconds(0.2f);

        adultTransform.gameObject.SetActive(true);

        gameManager.PlaySoundEffect(snd_WalkToDoor, 0.2f);
        yield return new WaitForSeconds(3f);

        // Open door and greet
        doorController.ToggleDoorAnimation(true);
        yield return new WaitForSeconds(0.5f);

        gameManager.dialogueController.minPitch = 1.2f;
        gameManager.dialogueController.maxPitch = 1.3f;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(sndKidClips);
        gameManager.dialogueController.ShowSubtitles("<color=orange>TRICK OR TREAT!</color>", false);

        yield return new WaitForSeconds(0.25f);
        yield return new WaitUntil(() => !gameManager.dialogueController.isShowingSubtitles);
        yield return new WaitForSeconds(0.25f);

        // Response
        gameManager.dialogueController.minPitch = vocalPitch_min;
        gameManager.dialogueController.maxPitch = vocalPitch_max;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(sndCharacterClips);
        SayDialogue();

        yield return new WaitForSeconds(0.25f);
        yield return new WaitUntil(() => !gameManager.dialogueController.isShowingSubtitles);
        yield return new WaitForSeconds(1f);

        StartingDistance = distance;

        // --- Walk to player ---
        animator.SetBool("WalkForward", true);
        yield return new WaitUntil(() => distance <= targetDistance);
        animator.SetBool("WalkForward", false);

        // --- HAND RIG SEQUENCE ---
        // Step 1: Raise hand
        float handTimer = 0f;
        while (handTimer < rigBlendDuration)
        {
            handTimer += Time.deltaTime;
            RightHandRig.weight = Mathf.SmoothStep(0f, 1f, handTimer / rigBlendDuration);
            yield return null;
        }
        RightHandRig.weight = 1f;

        // Play candy grab sound and show candy
        gameManager.PlaySoundEffect(snd_grabCandy, 0.5f);
        candyTransform.gameObject.SetActive(true);

        // Step 2: Move hand to drop position
        handTimer = 0f;
        Vector3 startHandPos = handTargetTransform.position;
        while (handTimer < handMoveDuration)
        {
            handTimer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, handTimer / handMoveDuration);
            handTargetTransform.position = Vector3.Lerp(startHandPos, dropPositionTransform.position, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // --- Candy Drop ---
        Transform candyTransformBeingMoved = null;
        CandyInfo candyInfoBeingMoved = null;
        int targetID = 0;
        foreach(Transform t in candyTransform)
        {
            if(t.childCount > 0)
            {
                candyTransformBeingMoved = t.GetChild(0);
                candyInfoBeingMoved = candyTransformBeingMoved.GetComponent<CandyInfo>();
                targetID = candyInfoBeingMoved.candyID;
            }
        }

        Transform targetCandyPos = bucket.CandySpotTransforms[targetID];

        if (targetCandyPos != null)
        {
            // Reparent candy
            candyTransformBeingMoved.SetParent(targetCandyPos);

            // Smooth local move + rotation
            Vector3 startLocalPos = candyTransformBeingMoved.localPosition;
            Quaternion startLocalRot = candyTransformBeingMoved.localRotation;

            float dropTimer = 0f;
            while (dropTimer < 0.3f)
            {
                dropTimer += Time.deltaTime * 3f;
                float t = Mathf.SmoothStep(0f, 1f, dropTimer / 0.3f);
                candyTransformBeingMoved.localPosition = Vector3.Lerp(startLocalPos, Vector3.zero, t);
                candyTransformBeingMoved.localRotation = Quaternion.Slerp(startLocalRot, Quaternion.identity, t);
                yield return null;
            }

            gameManager.PlaySoundEffect(snd_dropCandy, 0.5f);
            candyTransformBeingMoved.localPosition = Vector3.zero;
            candyTransformBeingMoved.localRotation = Quaternion.identity;
            candyTransformBeingMoved.gameObject.layer = LayerMask.NameToLayer("Hands");
            candyTransformBeingMoved.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Hands");
        }

        gameManager.candyCount += 1;

        // Wait a moment before lowering hand
        yield return new WaitForSeconds(returnDelay);

        // Step 3: Lower hand
        handTimer = 0f;
        while (handTimer < rigBlendDuration)
        {
            handTimer += Time.deltaTime;
            RightHandRig.weight = Mathf.SmoothStep(1f, 0f, handTimer / rigBlendDuration);
            yield return null;
        }
        RightHandRig.weight = 0f;

        yield return new WaitForSeconds(0.5f);

        // --- Walk back to start ---
        animator.SetBool("WalkBackwards", true);
        yield return new WaitUntil(() => distance > StartingDistance + 0.1f);
        animator.SetBool("WalkBackwards", false);

        gameManager.dialogueController.minPitch = 1.2f;
        gameManager.dialogueController.maxPitch = 1.3f;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(sndKidClips);
        gameManager.dialogueController.ShowSubtitles(goodbyeText, false);

        yield return new WaitForSeconds(0.25f);
        yield return new WaitUntil(() => !gameManager.dialogueController.isShowingSubtitles);
        yield return new WaitForSeconds(0.25f);

        doorController.ToggleDoorAnimation(false);

        yield return new WaitForSeconds(1);
        adultTransform.gameObject.SetActive(false);

        gameManager.PlaySoundEffect(snd_turnOffLight, 0.4f);
        lightOn_obj.SetActive(false);
        lightOff_obj.SetActive(true);

        playerController.Camera.StopForceLooking();
        gameManager.playerController.Camera.CanLookAround = true;
        gameManager.playerController.Movement.CanMoveAround = true;

        // End
        if(gameManager.candyCount == 1)
        {
            gameManager.startCutscene.ShowTutorial1();
        }

        if(gameManager.candyCount > 5)
        {
            gameManager.ActivateLeaveTrigger();
        }

        Debug.Log("Interaction complete.");
    }
}
