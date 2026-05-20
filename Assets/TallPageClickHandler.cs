using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TallPageClickHandler : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI pageText;
    public TallPageController controller;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Convert screen click into local space of whichever text object was clicked
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pageText.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        int charIndex = TMP_TextUtilities.FindNearestCharacter(pageText, localPoint, null, false);

        if (charIndex != -1)
            controller.OnPageClicked(charIndex);
    }
}