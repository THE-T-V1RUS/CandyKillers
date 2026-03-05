using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

public class Nest_Popup_0 : MonoBehaviour
{
    public Button button_yes, button_no;
    public CanvasGroup darkenBG_CanvasGroup;
    public Transform popupRoot; // assign the root object you want to scale (probably this.transform)
    public AudioVibration audioVibration;
    public GameManager gameManager;
    public Cellphone cellphone;
    public GameObject burritoFridgeInteraction;

    private void Start()
    {
        initialize();
    }

    void initialize()
    {
        gameManager = GameManager.Instance;
        if(gameManager == null)
        {
            initialize();
            return;
        }
    }

    public void YesButton()
    {
        Debug.Log("YES");
        DisableButtons();
        StartCoroutine(FadeAndDisable());
    }

    public void NoButton()
    {
        Debug.Log("NO");
        DisableButtons();
        StartCoroutine(FadeAndDisable());
    }

    void DisableButtons()
    {
        button_yes.image.raycastTarget = false;
        button_no.image.raycastTarget = false;
        cellphone.SetInteractable(false);
    }

    IEnumerator FadeAndDisable()
    {
        audioVibration.StopVibrating();
        float duration = 0.2f;
        float time = 0f;

        Vector3 startScale = popupRoot.localScale;
        float startAlpha = darkenBG_CanvasGroup.alpha;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            popupRoot.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            darkenBG_CanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        popupRoot.localScale = Vector3.zero;
        darkenBG_CanvasGroup.alpha = 0f;

        cellphone.StopInteraction();
        
        StartCoroutine(StartDialogueCoroutine());
    }

    IEnumerator StartDialogueCoroutine()
    {
        yield return new WaitForSeconds(1.2f);

        burritoFridgeInteraction.SetActive(true);

        List<string> dialogue = new List<string>();
        dialogue.Add("As expected...");
        dialogue.Add("Ugh... I didn't sleep well at all last night...");
        dialogue.Add("I guess I should cook something.");
        dialogue.Add("A frozen breakfast burrito should work.");

        gameManager.ChangeGameMode(GameManager.GameMode.FPS);
        gameManager.dialogueText.StartTeletype(dialogue);

        gameObject.SetActive(false);
        yield return null;
    }
}
