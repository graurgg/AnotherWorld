using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PageClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("Which page")]
    public bool isPage2;

    [Header("References")]
    public TextMeshProUGUI pageText;
    public LinkedPageController controller;

    public void OnPointerClick(PointerEventData eventData)
    {
        pageText.ForceMeshUpdate();

        // Convert screen click → TMP local space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pageText.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        int charIndex = TMP_TextUtilities.FindNearestCharacter(
            pageText, localPoint, null, false
        );

        // Page 2 indices are local to that TMP — offset to global
        if (isPage2 && controller.overflowStart >= 0)
            charIndex += controller.overflowStart;

        controller.OnPageClicked(charIndex);
    }
}