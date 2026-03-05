using Hertzole.GoldPlayer;
using UnityEngine;

public class Initilization_day1 : MonoBehaviour
{
    public bool playOpeningCutscene;

    public bool startAtStart;
    public bool disableCellphone;
    public bool disableDoorBlocker;
    public bool startWithCookedBurrito;
    public bool disableTitleScreenCutscene;

    //GAMEMANGER
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject openingCutscene;
    [SerializeField] BlinkController blinkController;

    //PLAYER
    [SerializeField] GoldPlayerController playerController;
    [SerializeField] Vector3 playerStartingPosition, playerStartingRotation;

    //CELLPHONE
    [SerializeField] Cellphone cellphone;
    [SerializeField] AudioVibration audioVibration;
    [SerializeField] GameObject cellphoneVibrationAduioSource, cellphone_NestPopup, cellphone_PhotoEditor;
    [SerializeField] CanvasGroup darkenPhoneScreenCanvasGroup;

    //FRIDGE
    [SerializeField] GameObject GetBreakfastBurrito;

    //MICROWAVE
    [SerializeField] MicrowaveController microwave;
    [SerializeField] GameObject OverlayBurrito, OverlayBurrioSteam;

    //GARAGE DOOR
    [SerializeField] GarageDoorBlocker garageDoorBlocker;
    [SerializeField] GameObject titleScreenTrigger;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Opening Cutscene
        openingCutscene.SetActive(playOpeningCutscene);
        if (!playOpeningCutscene)
            blinkController.blinkAmount = 0;
        else
            blinkController.blinkAmount = 1;

        //Set up player
        if (playOpeningCutscene)
            gameManager.ChangeGameMode(GameManager.GameMode.Cutscene);
        else
            gameManager.ChangeGameMode(GameManager.GameMode.FPS);
        playerController.Movement.CanMoveAround = !playOpeningCutscene;
        playerController.Camera.CanLookAround = !playOpeningCutscene;
        if (startAtStart)
        {
            playerController.transform.position = playerStartingPosition;
            playerController.transform.eulerAngles = playerStartingRotation;
        }

        //Set up phone
        cellphone.enabled = true;
        cellphone.SetInteractable(true);
        audioVibration.enabled = true;
        cellphoneVibrationAduioSource.SetActive(true);
        cellphone_NestPopup.SetActive(true);
        cellphone_NestPopup.transform.localScale = Vector3.one;
        cellphone_PhotoEditor.SetActive(false);
        cellphone_PhotoEditor.transform.localScale = Vector3.zero;
        darkenPhoneScreenCanvasGroup.alpha = 1;

        //Fridge
        GetBreakfastBurrito.SetActive(false);

        //Microwave
        microwave.interactionCollider.enabled = false;

        //Garage Door Blocker
        garageDoorBlocker.isBlocking = true;
        garageDoorBlocker.rb.isKinematic = true;

        if (disableCellphone)
            cellphone.gameObject.SetActive(false);
        
        if (disableDoorBlocker)
        {
            garageDoorBlocker.isBlocking = false;
            garageDoorBlocker.rb.isKinematic = false;
        }

        if (startWithCookedBurrito)
        {
            OverlayBurrito.SetActive(true);
            OverlayBurrioSteam.SetActive(true);
        }

        if (disableTitleScreenCutscene)
            titleScreenTrigger.SetActive(false);
    }

    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            playerController.Camera.ForceLook(microwave.transform.position);
        }
    }
    */
}
