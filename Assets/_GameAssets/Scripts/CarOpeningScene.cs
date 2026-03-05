using Hertzole.GoldPlayer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CarOpeningScene : MonoBehaviour
{
    [System.Serializable]
    public class Dialogue
    {
        public List<string> textList = new List<string>();
    }

    public Volume globalVolume; // Assign your global volume in the inspector
    public float duration = 3f; // How long the transition takes
    public CameraShake camShake;
    private ChromaticAberration chromaticAberration;
    private DepthOfField depthOfField;
    private LensDistortion lensDestortion;
    private ColorAdjustments colorAdjustments;
    private float startingChromaticAberration;
    public Color targetColor;
    public GameObject youDiedObj, carSceneObj, gooodEndingObj, askMomTrigger;

    public CanvasGroup blackoutCanvasGroup, eatTutorialCanvasGroup;
    public CoreGameManager gameManager;
    public GoldPlayerController playerController;
    public Transform momLookTarget, startingLookPos;
    public Rig momHeadRig;
    public Collider askMom, candyBucket;
    public CursorController cursorController;

    public AudioClip snd_warning, snd_eatCandy, sndChoke, sndEatBadCandy;
    public AudioSource chooseCandyMusic, carAmbience;

    public List<Dialogue> eatCandyLines_kid, eatCandyLines_mom;

    public TextMeshProUGUI candyTMP;
    public int candyEaten = 0;
    public Transform chosenCandy;
    public CandyInfo candyInfo;

    public ChooseCandy candyController;

    [TextArea(3, 10)]
    public string warningText;

    public float pitchMin_mom, pitchMax_mom, pitchMin_kid, pitchMax_kid;
    public List<AudioClip> mom_audioClips;
    public List<AudioClip> kid_audioClips;

    public List<Transform> GoodEndingCandyTransforms;

    private AudioSource myAudioSource;

    private void OnEnable()
    {
        candyEaten = 0;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        momHeadRig.weight = 0f;
        askMomTrigger.SetActive(true);
        myAudioSource = GetComponent<AudioSource>();
        playerController.Camera.ForceLook(startingLookPos, 0);
        blackoutCanvasGroup.alpha = 1;
        StartCoroutine(BeginCarScene());

        if (globalVolume.profile.TryGet(out chromaticAberration) == false)
            Debug.LogWarning("No Chromatic Aberration found!");

        if (globalVolume.profile.TryGet(out depthOfField) == false)
            Debug.LogWarning("No Depth of Field found!");

        if (globalVolume.profile.TryGet(out lensDestortion) == false)
            Debug.LogWarning("No Lens Distortion found!");

        if (globalVolume.profile.TryGet(out colorAdjustments) == false)
            Debug.LogWarning("No Color Adjustments found!");

        chromaticAberration.intensity.value = 0f;
        depthOfField.focalLength.value = 0f;
        lensDestortion.intensity.value = 0f;
        colorAdjustments.colorFilter.value = Color.white;
    }

    private void Update()
    {
        candyTMP.text = "Candy Eaten: " + candyEaten.ToString() + " / 3";
    }

    IEnumerator BeginCarScene()
    {
        candyBucket.enabled = false;
        askMom.enabled = false;
        blackoutCanvasGroup.alpha = 1;

        yield return new WaitForSeconds(0.2f);

        gameManager.brain.DefaultBlend.Time = 1f;
        askMom.enabled = true;
        cursorController.interactDistance = 2f;
        playerController.Camera.StopForceLooking();

        while (blackoutCanvasGroup.alpha > 0)
        {
            blackoutCanvasGroup.alpha -= 1 * Time.deltaTime * 0.25f;
            yield return null;
        }
    }

    public void AskMom()
    {
        StartCoroutine(askMomCoroutine());
    }

    IEnumerator askMomCoroutine()
    {
        playerController.Camera.ForceLook(momLookTarget, 5);

        gameManager.dialogueController.minPitch = pitchMin_kid;
        gameManager.dialogueController.maxPitch = pitchMax_kid;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(kid_audioClips);
        gameManager.dialogueController.ShowSubtitles("<color=orange>Mom, can I have some candy <i>now</i>?</color>", false);

        yield return new WaitForSeconds(0.25f);

        while(gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        while (momHeadRig.weight < 1)
        {
            momHeadRig.weight += Time.deltaTime * 2;
            yield return null;
        }

        gameManager.dialogueController.minPitch = pitchMin_mom;
        gameManager.dialogueController.maxPitch = pitchMax_mom;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(mom_audioClips);
        List<string> momLines = new List<string>();
        momLines.Add("You can have <b>three</b> pieces but that's it! Okay?");
        gameManager.dialogueController.ShowSubtitles(momLines, false);

        yield return new WaitForSeconds(0.25f);

        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        gameManager.dialogueController.minPitch = pitchMin_kid;
        gameManager.dialogueController.maxPitch = pitchMax_kid;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(kid_audioClips);
        gameManager.dialogueController.ShowSubtitles("<color=orange>Okay! Thanks mom!</color>", false);

        yield return new WaitForSeconds(0.25f);

        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        while (momHeadRig.weight > 0)
        {
            momHeadRig.weight -= Time.deltaTime * 2;
            yield return null;
        }

        myAudioSource.PlayOneShot(snd_warning, 0.5f);
        StartCoroutine(StartChooseCandyMusic());

        gameManager.warningDialogueController.ShowSubtitles(warningText, false);

        yield return new WaitForSeconds(0.25f);

        while (gameManager.warningDialogueController.isShowingSubtitles)
            yield return null;

        playerController.Camera.StopForceLooking();
        candyBucket.enabled = true;
        gameManager.canAccessMemory = true;

        while (eatTutorialCanvasGroup.alpha < 1)
        {
            eatTutorialCanvasGroup.alpha += Time.deltaTime * 0.33f;
            yield return null;
        }
    }

    IEnumerator StartChooseCandyMusic()
    {
        while (chooseCandyMusic.volume < 1)
        {
            chooseCandyMusic.volume += Time.deltaTime * 0.2f;
            yield return null;
        }
    }

    public void EatCandy(Transform chosenCandyTransform)
    {
        gameManager.canAccessMemory = false;
        candyController.CarCam.SetActive(true);
        candyController.BucketCam.SetActive(false);
        chosenCandy = chosenCandyTransform;
        candyInfo = chosenCandy.GetComponent<CandyInfo>();
        chosenCandy.gameObject.SetActive(false);

        foreach(var collider in candyController.candyColliders)
        {
            collider.enabled = false;
        }

        StartCoroutine(EatCandyCoroutine());
    }

    IEnumerator EatCandyCoroutine()
    {
        gameManager.PlaySoundEffect(snd_eatCandy, 0.7f);

        yield return new WaitForSeconds(4);

        candyEaten += 1;

        playerController.Camera.ForceLook(momLookTarget, 5);

        gameManager.dialogueController.minPitch = pitchMin_kid;
        gameManager.dialogueController.minPitch = pitchMax_kid;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(kid_audioClips);
        gameManager.dialogueController.ShowSubtitles(eatCandyLines_kid[candyEaten-1].textList, false);

        yield return new WaitForSeconds(0.1f);

        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        while (momHeadRig.weight < 1)
        {
            momHeadRig.weight += Time.deltaTime * 2;
            yield return null;
        }

        gameManager.dialogueController.minPitch = pitchMin_mom;
        gameManager.dialogueController.maxPitch = pitchMax_mom;
        gameManager.dialogueController.characterSoundEffects = new List<AudioClip>(mom_audioClips);
        gameManager.dialogueController.ShowSubtitles(eatCandyLines_mom[candyEaten - 1].textList, false);

        yield return new WaitForSeconds(0.25f);

        while (gameManager.dialogueController.isShowingSubtitles)
            yield return null;

        if (candyInfo.isPoisoned)
        {
            TriggerPoison();
        }
        else
        {
            playerController.Camera.StopForceLooking();
            playerController.Camera.CanLookAround = true;

            if(candyEaten < 3)
            {
                candyController.bucketCollider.enabled = true;
                gameManager.canAccessMemory = true;
            }

            while (momHeadRig.weight > 0)
            {
                momHeadRig.weight -= Time.deltaTime * 2;
                yield return null;
            }

            if(candyEaten > 2)
            {
                while(chooseCandyMusic.volume > 0 && carAmbience.volume > 0)
                {
                    chooseCandyMusic.volume -= Time.deltaTime * 0.5f;
                    carAmbience.volume -= Time.deltaTime * 0.5f;
                    yield return null;
                }

                while(blackoutCanvasGroup.alpha > 0)
                {
                    blackoutCanvasGroup.alpha -= Time.deltaTime * 0.3f;
                    yield return null;
                }

                //Transfer remaining candy to good ending
                foreach(Transform candyt in gameManager.candyObjs)
                {
                    if (candyt.gameObject.activeInHierarchy)
                    {
                        CandyInfo candyInfo = candyt.GetComponent<CandyInfo>();
                        foreach (Transform trans in GoodEndingCandyTransforms)
                        {
                            var spotInfo = trans.GetComponent<BucketCandySpot>();
                            if(spotInfo.bucketCandySpotID == candyInfo.candyID)
                            {
                                candyt.SetParent(trans, true);
                                candyt.localPosition = Vector3.zero;
                                candyt.localRotation = Quaternion.identity;
                            }
                        }
                    }
                }

                eatTutorialCanvasGroup.alpha = 0;
                gameManager.brain.DefaultBlend.Time = 0f;

                gooodEndingObj.SetActive(true);
                carSceneObj.SetActive(false);
            }
        }
    }

    public void TriggerPoison()
    {
        gameManager.canPause = false;
        print("poison");
        StartCoroutine(PoisonCoroutine());

        //Character Death effects

        //Do all of these things simulatiously in X seconds
        //Slowly increase chromaticAberration to 1
        //Enable depth of field and slowly increase Focal Length to 300
        //Enable Lens Distortion and slowly increase intensity to 0.7
        //Enable Color Adjustment and slowly adjust color filter to target color
    }

    private IEnumerator PoisonCoroutine()
    {
        playerController.Camera.StopForceLooking();

        gameManager.PlaySoundEffect(sndChoke, 0.7f);

        yield return new WaitForSeconds(1);

        gameManager.PlaySoundEffect(sndEatBadCandy, 0.5f);

        startingChromaticAberration = chromaticAberration.intensity.value;

        float timer = 0f;

        // Enable effects
        if (depthOfField != null) depthOfField.active = true;
        if (lensDestortion != null) lensDestortion.active = true;
        if (colorAdjustments != null) colorAdjustments.active = true;

        // Store starting values
        float startCA = chromaticAberration != null ? chromaticAberration.intensity.value : 0f;
        float startDOF = depthOfField != null ? depthOfField.focalLength.value : 0f;
        float startLens = lensDestortion != null ? lensDestortion.intensity.value : 0f;
        Color startColor = colorAdjustments != null ? colorAdjustments.colorFilter.value : Color.white;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(startCA, 1f, t);

            if (depthOfField != null)
                depthOfField.focalLength.value = Mathf.Lerp(startDOF, 300f, t);

            if (lensDestortion != null)
                lensDestortion.intensity.value = Mathf.Lerp(startLens, 0.7f, t);

            if (colorAdjustments != null)
                colorAdjustments.colorFilter.value = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        // Ensure final values
        if (chromaticAberration != null)
            chromaticAberration.intensity.value = 1f;

        if (depthOfField != null)
            depthOfField.focalLength.value = 300f;

        if (lensDestortion != null)
            lensDestortion.intensity.value = 0.7f;

        if (colorAdjustments != null)
            colorAdjustments.colorFilter.value = targetColor;

        camShake.isShaking = true;

        yield return new WaitForSeconds(10 - duration);

        blackoutCanvasGroup.alpha = 1;
        eatTutorialCanvasGroup.alpha = 0;
        chooseCandyMusic.volume = 0f;

        yield return new WaitForSeconds(3);

        if (depthOfField != null) depthOfField.active = false;
        if (lensDestortion != null) lensDestortion.active = false;
        if (colorAdjustments != null) colorAdjustments.active = false;
        chromaticAberration.intensity.value = startingChromaticAberration;
        gameManager.brain.DefaultBlend.Time = 0f;
        camShake.isShaking = false;
        youDiedObj.SetActive(true);
        carSceneObj.SetActive(false);
    }
}
