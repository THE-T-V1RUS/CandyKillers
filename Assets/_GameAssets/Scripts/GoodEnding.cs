using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GoodEnding : MonoBehaviour
{
    [System.Serializable]
    public class Dialogue
    {
        public List<string> textList = new List<string>();
    }

    public CoreGameManager gameManager;
    public List<Dialogue> momLines, kidLines;
    public float pitchMin_mom, pitchMax_mom, pitchMin_kid, pitchMax_kid;
    public List<AudioClip> mom_audioClips;
    public List<AudioClip> kid_audioClips;
    public Transform momTransform, momLookAtTransform, momLookAtTransform2, candyInHandTransform, firstHandLocation, secondHandLocation, eatLocation, handTarget;
    public List<Transform> candyTransforms;
    public AudioClip snd_MomPickUpCandy, snd_momEatCandy, snd_MomDie, snd_GetInBed, snd_Glass, snd_Eerie;
    public AudioSource momHeadAudioSource;
    public GameObject EndingCredits, ceditsCamera;
    public GameObject PoisonBottle;
    public TextMeshProUGUI endingTypeTMP;

    public Quaternion startingMomRotation;

    public bool isGoodEnding = false;

    public Animator momAnimator, camAnimator;

    public float rotateSpeed = 0.3f;
    public float pickupSpeed = 0.3f;

    public Rig handRig, waistRig;

    public CanvasGroup blackoutCanvas, cursorCanvas;

    public GameObject carSceneObj, neighborhoodObj, ambienceObj;

    private void Start()
    {
        StartEnding();
    }

    public void StartEnding()
    {
        cursorCanvas.alpha = 0f;
        startingMomRotation = momTransform.rotation;
        StartCoroutine(EndingCoroutine());
    }

    IEnumerator EndingCoroutine()
    {
        gameManager.canPause = false;

        if (gameManager.poisonBottlesFound > 2 && gameManager.retryCount == 0)
        {
            PoisonBottle.SetActive(true);
            isGoodEnding = true;
        }
        gameManager.PlaySoundEffect(snd_GetInBed);
        blackoutCanvas.alpha = 1;
        ambienceObj.SetActive(false);
        neighborhoodObj.SetActive(false);
        carSceneObj.SetActive(false);

        yield return new WaitForSeconds(1);

        blackoutCanvas.alpha = 1;

        // Fade in from black
        while (blackoutCanvas.alpha > 0)
        {
            blackoutCanvas.alpha -= Time.deltaTime * 0.5f;
            yield return null;
        }

        // Set mom dialogue parameters
        gameManager.dialogueController.minPitch = pitchMin_mom;
        gameManager.dialogueController.maxPitch = pitchMax_mom;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(mom_audioClips);
        gameManager.dialogueController.ShowSubtitles(momLines[0].textList, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        handTarget.position = firstHandLocation.position;
        handTarget.rotation = firstHandLocation.rotation;

        // --- Smoothly rotate mom to look at target, Y-axis only ---
        float rotateDuration = rotateSpeed;
        float rotateTimer = 0f;
        Quaternion startRot = momTransform.rotation;

        // Get direction to target but ignore vertical difference
        Vector3 direction = momLookAtTransform.position - momTransform.position;
        direction.y = 0f; // Lock to horizontal plane

        // Avoid NaN if direction is zero
        if (direction.sqrMagnitude < 0.001f)
            yield break;

        Quaternion targetRot = Quaternion.LookRotation(direction.normalized);

        while (rotateTimer < rotateDuration)
        {
            rotateTimer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, rotateTimer / rotateDuration);
            momTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // --- Smoothly increase rig weights ---
        float rigDuration = pickupSpeed;
        float rigTimer = 0f;

        while (rigTimer < rigDuration)
        {
            rigTimer += Time.deltaTime;
            float t = rigTimer / rigDuration;
            handRig.weight = Mathf.Lerp(0f, 1f, t);
            waistRig.weight = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        if (isGoodEnding)
        {
            StartCoroutine(TheGoodEnding());
        }
        else
        {
            StartCoroutine(TheBadEnding());
        }

        // You can continue your ending sequence here...
    }

    IEnumerator TheGoodEnding()
    {
        endingTypeTMP.text = "<size=60>Good ending</size>";
        Debug.Log("Pick up");
        gameManager.PlaySoundEffect(snd_Glass, 0.2f);

        PoisonBottle.transform.SetParent(candyInHandTransform, true);
        PoisonBottle.transform.localPosition = new Vector3(-0.000896f, -9e-06f, -0.000124f);
        PoisonBottle.transform.localRotation = Quaternion.Euler(new Vector3(180f, -174.688f, -74.807f));

        // --- Smoothly decrease rig weights ---
        float rigDuration = pickupSpeed;
        var rigTimer = 0f;
        while (rigTimer < rigDuration)
        {
            rigTimer += Time.deltaTime;
            float t = rigTimer / rigDuration;
            handRig.weight = Mathf.Lerp(1f, 0f, t);
            waistRig.weight = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        handTarget.position = secondHandLocation.position;
        handTarget.rotation = secondHandLocation.rotation;

        // --- Smoothly increase rig weights ---
        rigDuration = pickupSpeed;
        rigTimer = 0f;

        while (rigTimer < rigDuration)
        {
            rigTimer += Time.deltaTime;
            float t = rigTimer / rigDuration;
            handRig.weight = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        gameManager.dialogueController.ShowSubtitles(momLines[3].textList, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        // --- Smoothly decrease rig weights ---
        rigDuration = pickupSpeed;
        rigTimer = 0f;
        while (rigTimer < rigDuration)
        {
            rigTimer += Time.deltaTime;
            float t = rigTimer / rigDuration;
            handRig.weight = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        gameManager.dialogueController.minPitch = pitchMin_kid;
        gameManager.dialogueController.maxPitch = pitchMax_kid;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(kid_audioClips);
        gameManager.dialogueController.ShowSubtitles(kidLines[2].textList, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        Quaternion targetRot = startingMomRotation;
        float rotateDuration = rotateSpeed;
        float rotateTimer = 0f;
        Quaternion startRot = momTransform.rotation;

        while (rotateTimer < rotateDuration)
        {
            rotateTimer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, rotateTimer / rotateDuration);
            momTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        gameManager.PlaySoundEffect(snd_Eerie, 0.4f);

        gameManager.dialogueController.minPitch = pitchMin_mom;
        gameManager.dialogueController.maxPitch = pitchMax_mom;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(mom_audioClips);
        gameManager.dialogueController.ShowSubtitles(momLines[4].textList, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        cursorCanvas.alpha = 0f;
        gameManager.brain.DefaultBlend.Time = 1;

        ceditsCamera.SetActive(true);

        yield return new WaitForSeconds(1);

        EndingCredits.SetActive(true);
    }

    IEnumerator TheBadEnding()
    {
        endingTypeTMP.text = "<size=60>Bad ending</size><color=orange>\nPoison Bottles Found: " + gameManager.poisonBottlesFound.ToString() + " / 3\nRetries used: " + gameManager.retryCount.ToString() + "<color=red>\n\n3 poison bottles and NO retries for good ending";
        Debug.Log("Pick up");
        gameManager.PlaySoundEffect(snd_MomPickUpCandy, 0.2f);

        Transform chosenCandy = null;
        foreach (Transform candyTransform in candyTransforms)
        {
            if (candyTransform.childCount > 0)
            {
                chosenCandy = candyTransform.GetChild(0).transform;
            }
        }
        chosenCandy.SetParent(candyInHandTransform, true);
        chosenCandy.localPosition = Vector3.zero;
        chosenCandy.localRotation = Quaternion.identity;

        // --- Smoothly decrease rig weights ---
        float rigDuration = pickupSpeed;
        var rigTimer = 0f;
        while (rigTimer < rigDuration)
        {
            rigTimer += Time.deltaTime;
            float t = rigTimer / rigDuration;
            handRig.weight = Mathf.Lerp(1f, 0f, t);
            waistRig.weight = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        handTarget.position = secondHandLocation.position;
        handTarget.rotation = secondHandLocation.rotation;

        // --- Smoothly increase rig weights ---
        rigDuration = pickupSpeed;
        rigTimer = 0f;

        while (rigTimer < rigDuration)
        {
            rigTimer += Time.deltaTime;
            float t = rigTimer / rigDuration;
            handRig.weight = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        // --- Smoothly move handTarget to eatLocation ---
        float moveDuration = 0.4f;
        float moveTimer = 0f;

        Vector3 startPos = handTarget.position;
        var startRot = handTarget.rotation;

        while (moveTimer < moveDuration)
        {
            moveTimer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, moveTimer / moveDuration);
            handTarget.position = Vector3.Lerp(startPos, eatLocation.position, t);
            handTarget.rotation = Quaternion.Slerp(startRot, eatLocation.rotation, t);
            yield return null;
        }

        // --- Eat candy ---
        momHeadAudioSource.PlayOneShot(snd_momEatCandy, 0.1f);
        candyInHandTransform.gameObject.SetActive(false);

        // --- Smoothly move handTarget to secondHandLocation ---
        moveTimer = 0f;
        startPos = handTarget.position;
        startRot = handTarget.rotation;

        while (moveTimer < moveDuration)
        {
            moveTimer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, moveTimer / moveDuration);
            handTarget.position = Vector3.Lerp(startPos, secondHandLocation.position, t);
            handTarget.rotation = Quaternion.Slerp(startRot, secondHandLocation.rotation, t);
            yield return null;
        }

        // --- Smoothly decrease rig weights ---
        rigTimer = 0f;
        while (rigTimer < rigDuration)
        {
            rigTimer += Time.deltaTime;
            float t = rigTimer / rigDuration;
            handRig.weight = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        gameManager.dialogueController.ShowSubtitles(momLines[1].textList, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        gameManager.dialogueController.minPitch = pitchMin_kid;
        gameManager.dialogueController.maxPitch = pitchMax_kid;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(kid_audioClips);
        gameManager.dialogueController.ShowSubtitles(kidLines[0].textList, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        gameManager.dialogueController.minPitch = pitchMin_mom;
        gameManager.dialogueController.maxPitch = pitchMax_mom;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(mom_audioClips);
        gameManager.dialogueController.ShowSubtitles(momLines[2].textList, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        momAnimator.SetBool("Die", true);
        momHeadAudioSource.PlayOneShot(snd_MomDie, 0.1f);

        yield return new WaitForSeconds(4.5f);

        camAnimator.SetBool("Check", true);

        gameManager.dialogueController.minPitch = pitchMin_kid;
        gameManager.dialogueController.maxPitch = pitchMax_kid;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(kid_audioClips);
        gameManager.dialogueController.ShowSubtitles(kidLines[1].textList, false);

        yield return new WaitForSeconds(0.25f);
        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        cursorCanvas.alpha = 0f;
        gameManager.brain.DefaultBlend.Time = 1;

        ceditsCamera.SetActive(true);

        yield return new WaitForSeconds(1);

        EndingCredits.SetActive(true);
    }
}
