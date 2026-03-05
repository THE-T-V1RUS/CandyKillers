using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public enum TeachStep
    {
        None,
        MoveAndLook,
        OpenDoors,
        Interact,
    }

    [SerializeField] GameManager gameManager;
    [SerializeField] CanvasGroup canvas_tutorial_0, canvas_tutorial_1, canvas_tutorial_2;
    [SerializeField] InteractionController interactionController;
    TeachStep currentlyTeaching = TeachStep.None;

    bool hasMovedMouse, hasMovedPlayer, hasLearnedInteract;

    private void Update()
    {
        switch (currentlyTeaching)
        {
            case TeachStep.None:
                if(canvas_tutorial_0.alpha > 0) canvas_tutorial_0.alpha -= Time.deltaTime;
                if(canvas_tutorial_1.alpha > 0) canvas_tutorial_1.alpha -= Time.deltaTime;
                if(canvas_tutorial_2.alpha > 0) canvas_tutorial_2.alpha -= Time.deltaTime;

                if (!hasLearnedInteract && (interactionController.currentCursor == InteractionController.CurrentCursor.Interact))
                {
                    UpdateTeachingMode(TeachStep.Interact);
                    hasLearnedInteract = true;
                    return;
                }

                if(hasLearnedInteract && canvas_tutorial_0.alpha == 0 && canvas_tutorial_1.alpha == 0 && canvas_tutorial_2.alpha == 0)
                {
                    //TUTORIAL COMPLETE
                    this.gameObject.SetActive(false);
                }

                break;

            case TeachStep.MoveAndLook:
                if (canvas_tutorial_0.alpha < 1) canvas_tutorial_0.alpha += Time.deltaTime;
                if (canvas_tutorial_1.alpha > 0) canvas_tutorial_1.alpha -= Time.deltaTime;
                if (canvas_tutorial_2.alpha > 0) canvas_tutorial_2.alpha -= Time.deltaTime;

                // Detect mouse movement
                if ((Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f))
                    hasMovedMouse = true;

                // Detect WASD key presses
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                    hasMovedPlayer = true;

                if (hasMovedPlayer && hasMovedMouse)
                {
                    UpdateTeachingMode(TeachStep.OpenDoors);
                }

                break;

            case TeachStep.OpenDoors:
                if (canvas_tutorial_0.alpha > 0) canvas_tutorial_0.alpha -= Time.deltaTime;
                if (canvas_tutorial_1.alpha < 1) canvas_tutorial_1.alpha += Time.deltaTime;
                if (canvas_tutorial_2.alpha > 0) canvas_tutorial_2.alpha -= Time.deltaTime;

                if (gameManager.currentRoom == CurrentRoom.LivingRoom)
                    UpdateTeachingMode(TeachStep.None);
                break;

            case TeachStep.Interact:
                if (canvas_tutorial_0.alpha > 0) canvas_tutorial_0.alpha -= Time.deltaTime;
                if (canvas_tutorial_1.alpha > 0) canvas_tutorial_1.alpha -= Time.deltaTime;
                if (canvas_tutorial_2.alpha < 1) canvas_tutorial_2.alpha += Time.deltaTime;

                if (gameManager.currentGameMode != GameManager.GameMode.FPS)
                    UpdateTeachingMode(TeachStep.None);
                break;
        }
    }

    public void UpdateTeachingMode(TeachStep teach)
    {
        hasMovedMouse = false;
        hasMovedPlayer = false;
        currentlyTeaching = teach;
    }
}
