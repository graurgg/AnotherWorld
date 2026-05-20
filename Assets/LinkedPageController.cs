using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LinkedPageController : MonoBehaviour
{
    [Header("Pages")]
    public TextMeshProUGUI page1Text;
    public TextMeshProUGUI page2Text;

    [Header("Invisible Input Catcher")]
    public TMP_InputField hiddenInput;

    [Header("Carets")]
    public RectTransform caret1;
    public RectTransform caret2;

    [HideInInspector] public int overflowStart = -1;

    private int _caretIndex = 0;
    private Coroutine _blinkCoroutine;

    // ─────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────
    void Start()
    {
        hiddenInput.onValueChanged.AddListener(OnTextChanged);
        hiddenInput.ActivateInputField();

        SetCaretHeight(caret1, page1Text);
        SetCaretHeight(caret2, page2Text);
        StartBlink();
    }

    void Update()
    {
        if (hiddenInput.caretPosition != _caretIndex)
        {
            _caretIndex = hiddenInput.caretPosition;
            UpdateCaretVisual();
            StartBlink();
        }

        if (!hiddenInput.isFocused)
            hiddenInput.ActivateInputField();
    }

    // ─────────────────────────────────────────
    //  Text Splitting
    // ─────────────────────────────────────────
    void OnTextChanged(string _)
    {
        SplitPages();
    }

    void SplitPages()
    {
        string full = hiddenInput.text;

        page1Text.text = full;
        page1Text.ForceMeshUpdate();

        overflowStart = page1Text.firstOverflowCharacterIndex;

        if (overflowStart == -1)
        {
            page2Text.text = "";
        }
        else
        {
            page1Text.text = full.Substring(0, overflowStart);
            page2Text.text = full.Substring(overflowStart);
        }

        page1Text.ForceMeshUpdate();
        page2Text.ForceMeshUpdate();

        SetCaretHeight(caret1, page1Text);
        SetCaretHeight(caret2, page2Text);
        UpdateCaretVisual();
    }

    // ─────────────────────────────────────────
    //  Caret Visual
    // ─────────────────────────────────────────
    void UpdateCaretVisual()
    {
        bool onPage2 = overflowStart >= 0 && _caretIndex >= overflowStart;

        caret1.gameObject.SetActive(!onPage2);
        caret2.gameObject.SetActive(onPage2);

        if (onPage2)
        {
            int localIndex = _caretIndex - overflowStart;
            // Notice we are setting .position (World Space) directly now!
            caret2.position = GetCaretWorldPos(page2Text, localIndex);
        }
        else
        {
            caret1.position = GetCaretWorldPos(page1Text, _caretIndex);
        }
    }

    Vector3 GetCaretWorldPos(TextMeshProUGUI tmp, int charIndex)
    {
        tmp.ForceMeshUpdate();
        var info = tmp.textInfo;
        Vector3 tmpLocalPos = Vector3.zero;

        if (info.characterCount == 0 || charIndex >= info.characterCount)
        {
            if (info.characterCount == 0)
            {
                // Fallback for completely empty text
                Rect r = tmp.rectTransform.rect;
                tmpLocalPos = new Vector3(r.xMin + tmp.margin.x, r.yMax - tmp.margin.y, 0);
            }
            else
            {
                tmpLocalPos = info.characterInfo[info.characterCount - 1].topRight;
            }
        }
        else
        {
            tmpLocalPos = info.characterInfo[charIndex].topLeft;
        }

        // Convert the TMP local position into absolute World Space and return it directly.
        // No parent math, no screen math.
        return tmp.rectTransform.TransformPoint(tmpLocalPos);
    }

    void SetCaretHeight(RectTransform caret, TextMeshProUGUI tmp)
    {
        // Give it a default height based on font size so it's never 0
        float height = tmp.fontSize;

        tmp.ForceMeshUpdate();
        if (tmp.textInfo != null && tmp.textInfo.lineCount > 0)
        {
            height = tmp.textInfo.lineInfo[0].lineHeight;
        }

        caret.sizeDelta = new Vector2(caret.sizeDelta.x, height);
    }

    // ─────────────────────────────────────────
    //  Caret Blinking
    // ─────────────────────────────────────────
    public void StartBlink()
    {
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        SetActiveCaret(true);
        while (true)
        {
            yield return new WaitForSeconds(0.53f);
            SetActiveCaret(false);
            yield return new WaitForSeconds(0.53f);
            SetActiveCaret(true);
        }
    }

    void SetActiveCaret(bool visible)
    {
        bool onPage2 = overflowStart >= 0 && _caretIndex >= overflowStart;
        RectTransform active = onPage2 ? caret2 : caret1;
        active.gameObject.SetActive(visible);
    }

    // ─────────────────────────────────────────
    //  Called by PageClickHandler
    // ─────────────────────────────────────────
    public void OnPageClicked(int globalCharIndex)
    {
        // 1. Activate FIRST so Unity's internal routines don't overwrite our placement
        hiddenInput.ActivateInputField();

        // 2. Set the caret positions safely
        globalCharIndex = Mathf.Clamp(globalCharIndex, 0, hiddenInput.text.Length);

        hiddenInput.caretPosition = globalCharIndex;
        hiddenInput.selectionAnchorPosition = globalCharIndex;
        hiddenInput.selectionFocusPosition = globalCharIndex;

        _caretIndex = globalCharIndex;
        UpdateCaretVisual();
        StartBlink();
    }
}