using UnityEngine;
using TMPro;
using System.Collections;

public class BookInterfaceManager : MonoBehaviour
{
    [Header("Page References")]
    [Tooltip("Drag your Left Page TMP Input Field here")]
    [SerializeField] private TMP_InputField leftPageInput;

    // This runs every time the book GameObject is turned on (SetActive(true))
    private void OnEnable()
    {
        if (leftPageInput != null)
        {
            StartCoroutine(FocusLeftPageDelay());
        }
    }

    private IEnumerator FocusLeftPageDelay()
    {
        // Wait for exactly one frame. This gives Unity's UI and EventSystem 
        // time to register that the book is now active.
        yield return null;

        // .Select() highlights the object in the Event System
        leftPageInput.Select();

        // .ActivateInputField() specifically tells TextMeshPro to show the blinking 
        // caret and start accepting keyboard input immediately.
        leftPageInput.ActivateInputField();

        // Optional: Move the caret to the end of the text if there's already writing
        leftPageInput.caretPosition = leftPageInput.text.Length;
    }
}