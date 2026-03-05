using Hertzole.GoldPlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Edit_Photos : MonoBehaviour
{
    public Transform appRoot, photoPanelTransform;
    public int photoEditCount = 0;
    public bool GoodEdit = false;
    public GameManager gameManager;
    public EditAction currentEditAction;
    public GameObject SelectionMenu, CropMenu;
    public CropAnalyzer[] cropAnalyzer;
    public EditAction[] editActions;
    public MicrowaveController microwaveController;
    public Cellphone cellphone;
    public GoldPlayerController playerController;
    
    public enum EditAction
    {
        Delete,
        Crop,
        Save
    }

    public void OpenPhotoApp()
    {
        if (!this.enabled) return;
        StartCoroutine(OpenPhotoAppCoroutine());
    }

    public void ClosePhotoApp()
    {
        if (!this.enabled) return;
        StartCoroutine(ClosePhotoAppCoroutine());
    }

    IEnumerator OpenPhotoAppCoroutine()
    {
        appRoot.gameObject.SetActive(true);

        float duration = 0.2f;
        float time = 0f;

        Vector3 startScale = appRoot.localScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            appRoot.localScale = Vector3.Lerp(startScale, Vector3.one * 1.01f, t);

            yield return null;
        }

        appRoot.localScale = Vector3.one * 1.01f;

        if(photoEditCount == 0)
        {
            currentEditAction = EditAction.Crop;
            List<string> dialogue = new List<string>();
            dialogue.Add("She needs to be cropped out of this one...");
            gameManager.dialogueText.StartTeletype(dialogue, GameManager.GameMode.Interact);
        }
    }

    IEnumerator ClosePhotoAppCoroutine()
    {
        appRoot.gameObject.SetActive(true);

        float duration = 0.2f;
        float time = 0f;

        Vector3 startScale = appRoot.localScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            appRoot.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        appRoot.localScale = Vector3.zero;
    }

    public void PressCrop()
    {
        if (GoodEdit)
        {
            AlreadyCropped();
            return;
        }

        switch (currentEditAction)
        {
            case EditAction.Delete:
                Select_Crop_Instead_Of_Delete();
                break;

            case EditAction.Crop:
                SwitchToCropMode();
                break;

            case EditAction.Save:
                Select_Crop_Instead_Of_Save();
                break;
        }
    }

    public void PressDelete()
    {
        switch (currentEditAction)
        {
            case EditAction.Delete:
                DeletePhoto();
                break;

            case EditAction.Crop:
                Select_Delete_Instead_Of_Crop();
                break;

            case EditAction.Save:
                Select_Delete_Instead_Of_Save();
                break;
        }
    }

    public void PressSave()
    {
        if (GoodEdit)
        {
            NextPhoto();
            return;
        }

        switch (currentEditAction)
        {
            case EditAction.Delete:
                Select_Save_Instead_Of_Delete();
                break;

            case EditAction.Crop:
                Select_Save_Instead_Of_Edit();
                break;

            case EditAction.Save:
                    NextPhoto();
                break;
        }
    }

    public void PressSaveCrop()
    {
        CropAnalyzer currentCropAnalyzer = cropAnalyzer[photoEditCount];
        currentCropAnalyzer.CalculatePercentages();
        bool isGuyVisible = false;
        bool isGirlVisible = false;
        if(currentCropAnalyzer.keepPercent >= 1f) isGuyVisible=true;
        if(currentCropAnalyzer.deletePercent > 0.1f) isGirlVisible=true;

        //Just Girl
        if(!isGuyVisible && isGirlVisible)
        {
            BadCrop_Just_Girl_in_Photo();
            return;
        }

        //Girl & Guy Visible
        if(isGuyVisible && isGirlVisible)
        {
            BadCrop_Girl_and_Guy_in_Photo();
            return;
        }

        //Girl & Guy NOT Visible
        if(!isGuyVisible && !isGirlVisible)
        {
            BadCrop_Both_Removed_From_Photo();
            return;
        }

        //Just Guy
        if (isGuyVisible && !isGirlVisible)
        {
            SuccessfulPhotoCrop();
        }
    }

    void SwitchToCropMode()
    {
        CropMenu.SetActive(true);
        SelectionMenu.SetActive(false);
        cropAnalyzer[photoEditCount].cropBox.gameObject.SetActive(true);
    }

    void SwitchToSelectionMode()
    {
        CropMenu.SetActive(false);
        SelectionMenu.SetActive(true);
    }

    void NextPhoto()
    {
        SelectionMenu.SetActive(false);
        StartCoroutine(MovePanelLeftCoroutine());
    }

    IEnumerator MovePanelLeftCoroutine()
    {
        Vector3 startPos = photoPanelTransform.localPosition;
        float subtractionAmount = -895f;

        Vector3 targetPos = startPos + new Vector3(subtractionAmount, 0f, 0f);
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            photoPanelTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        photoPanelTransform.localPosition = targetPos;
        photoEditCount++;

        if(photoEditCount > 3)
        {
            microwaveController.FinishedCooking();
            yield return new WaitForSeconds(1f);
            ClosePhotoApp();
            yield return new WaitForSeconds(0.5f);
            cellphone.StopInteraction();
            playerController.Camera.StopForceLooking();
            List<string> dialogue = new List<string>();
            dialogue.Add("Breakfast is ready...");
            gameManager.dialogueText.StartTeletype(dialogue);
            cellphone.SetInteractable(false);
            microwaveController.interactionCollider.enabled = true;
            this.enabled = false;
        }
        else
        {
            GoodEdit = false;
            currentEditAction = editActions[photoEditCount];
            SelectionMenu.SetActive(true);
        }

    }

    void DeletePhoto()
    {
        StartCoroutine(FadeOutImage(1f));
    }

    IEnumerator FadeOutImage(float duration)
    {
        SelectionMenu.SetActive(false);

        CropAnalyzer currentCropAnalyzer = cropAnalyzer[photoEditCount];
        Image image = currentCropAnalyzer.transform.GetComponent<Image>();

        Color startColor = image.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            image.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        image.color = endColor;

        NextPhoto();
    }

    void SuccessfulPhotoCrop()
    {
        StartCoroutine(SuccessfulPhotoCropCoroutine());
    }

    IEnumerator SuccessfulPhotoCropCoroutine()
    {
        CropMenu.SetActive(false);
        CropAnalyzer currentCropAnalyzer = cropAnalyzer[photoEditCount];

        // TURN OFF CROP ANALYZER SO THE USER CAN NO LONGER DRAG IT AROUND
        currentCropAnalyzer.cropBox.gameObject.SetActive(false);

        // CROP PHOTO
        Transform maskTransform = currentCropAnalyzer.cropBox.GetChild(currentCropAnalyzer.cropBox.childCount - 1).transform;
        Transform photoTransform = currentCropAnalyzer.cropBox.transform.parent;
        Transform photoCenterTransform = photoTransform.parent;
        maskTransform.SetParent(photoCenterTransform);
        photoTransform.SetParent(maskTransform);

        // Calculate movement and scale targets
        RectTransform parentRT = photoCenterTransform.parent.GetComponent<RectTransform>();
        RectTransform maskRT = maskTransform.GetComponent<RectTransform>();

        Vector3 startPos = maskTransform.localPosition;
        Vector3 targetPos = photoCenterTransform.InverseTransformPoint(parentRT.position);
        targetPos.x += (photoEditCount * 2000);

        float startScale = maskTransform.localScale.x; // Assuming uniform scale
        float targetScale = 1f;

        if (parentRT != null && maskRT != null)
        {
            float parentHeight = parentRT.rect.height;
            float maskHeight = maskRT.rect.height;

            if (maskHeight > 0)
                targetScale = parentHeight / maskHeight;
        }

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Interpolate position and scale simultaneously
            maskTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            float scale = Mathf.Lerp(startScale, targetScale, t);
            maskTransform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        // Ensure final values are set
        maskTransform.localPosition = targetPos;
        maskTransform.localScale = new Vector3(targetScale, targetScale, 1f);

        GoodEdit = true;
        SwitchToSelectionMode();
    }

    public void Select_Delete_Instead_Of_Crop()
    {
        List<string> dialogue = new List<string>();

        if (GoodEdit)
        {
            dialogue.Add("Cropping her out was enough. I can keep this one now.");
            gameManager.dialogueText.StartTeletype(dialogue);
            return;
        }

        dialogue.Add("I don't want to delete the entire photo. I just don't want her to be in it anymore.");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void Select_Crop_Instead_Of_Delete()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("There's no salvaging this one. I can just delete it.");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void Select_Crop_Instead_Of_Save()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("I don't need to edit this one. She's not in it...");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void Select_Save_Instead_Of_Delete()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("I can't handle looking at her... I need to delete it.");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void Select_Save_Instead_Of_Edit()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("Oops... almost skipped this one.");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void Select_Delete_Instead_Of_Save()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("I can keep this one. She's not in it...");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void BadCrop_Girl_and_Guy_in_Photo()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("Hmm...She's still in the photo.");
        dialogue.Add("I can do better.");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void BadCrop_Just_Girl_in_Photo()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("What am I doing?");
        dialogue.Add("I need to remove her from the photo...");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void BadCrop_Both_Removed_From_Photo()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("Damn, I cropped too much. I'll fix it.");
        gameManager.dialogueText.StartTeletype(dialogue);
    }

    public void AlreadyCropped()
    {
        List<string> dialogue = new List<string>();
        dialogue.Add("It's like she was never there... I can move on to the next one.");
        gameManager.dialogueText.StartTeletype(dialogue);
    }
}
