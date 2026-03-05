using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPLinkOpener : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text textComponent;

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textComponent, eventData.position, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();
            Application.OpenURL(url);
        }
    }
}
