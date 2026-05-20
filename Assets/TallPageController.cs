using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TallPageController : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField hiddenInput;

    [Header("Text Renderers")]
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI rightText;

    [Header("Containers (Must have RectMask2D)")]
    public RectTransform leftContainer;
    public RectTransform rightContainer;

    [Header("Carets")]
    public RectTransform leftCaret;
    public RectTransform rightCaret;

    private int _caretIndex = 0;
    private Coroutine _blinkCoroutine;

    void Start()
    {
        hiddenInput.onValueChanged.AddListener(OnTextChanged);
        hiddenInput.ActivateInputField();

        // MAGIC TRICK: Shift the right text UP by exactly the height of the left container.
        // This pushes the "top half" of the text out of the right mask, revealing only the bottom half!
        float pageHeight = leftContainer.rect.height;
        rightText.rectTransform.anchoredPosition = new Vector2(
            rightText.rectTransform.anchoredPosition.x,
            pageHeight
        );

        StartBlink();
    }

    void Update()
    {
        if (hiddenInput.caretPosition != _caretIndex)
        {
            _caretIndex = hiddenInput.caretPosition;
            UpdateCaretPosition();
            StartBlink();
        }

        if (!hiddenInput.isFocused)
            hiddenInput.ActivateInputField();
    }

    void OnTextChanged(string newText)
    {
        leftText.text = newText;
        rightText.text = newText;
        UpdateCaretPosition();
    }

    void UpdateCaretPosition()
    {
        leftText.ForceMeshUpdate();
        var info = leftText.textInfo;
        Vector3 localPos = Vector3.zero;

        if (info.characterCount == 0 || _caretIndex >= info.characterCount)
        {
            if (info.characterCount == 0)
            {
                Rect r = leftText.rectTransform.rect;
                localPos = new Vector3(r.xMin + leftText.margin.x, r.yMax - leftText.margin.y, 0);
            }
            else
            {
                localPos = info.characterInfo[info.characterCount - 1].topRight;
            }
        }
        else
        {
            localPos = info.characterInfo[_caretIndex].topLeft;
        }

        // Apply the EXACT same local position to both carets. 
        // Because the rightText is shifted up, rightCaret automatically shifts up too.
        // The RectMask2D on the containers will effortlessly hide the one that goes out of bounds.
        leftCaret.localPosition = localPos;
        rightCaret.localPosition = localPos;

        // Set Caret Height
        float height = leftText.fontSize;
        if (info.lineCount > 0) height = info.lineInfo[0].lineHeight;

        leftCaret.sizeDelta = new Vector2(leftCaret.sizeDelta.x, height);
        rightCaret.sizeDelta = new Vector2(rightCaret.sizeDelta.x, height);
    }

    public void StartBlink()
    {
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        leftCaret.gameObject.SetActive(true);
        rightCaret.gameObject.SetActive(true);
        while (true)
        {
            yield return new WaitForSeconds(0.53f);
            leftCaret.gameObject.SetActive(false);
            rightCaret.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.53f);
            leftCaret.gameObject.SetActive(true);
            rightCaret.gameObject.SetActive(true);
        }
    }

    public void OnPageClicked(int globalCharIndex)
    {
        hiddenInput.ActivateInputField();
        globalCharIndex = Mathf.Clamp(globalCharIndex, 0, hiddenInput.text.Length);

        hiddenInput.caretPosition = globalCharIndex;
        hiddenInput.selectionAnchorPosition = globalCharIndex;
        hiddenInput.selectionFocusPosition = globalCharIndex;

        _caretIndex = globalCharIndex;
        UpdateCaretPosition();
        StartBlink();
    }
}