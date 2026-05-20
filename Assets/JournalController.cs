using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using UnityEngine.EventSystems;
using TMPro;

public class JournalController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject journalCanvas;

    [Header("Starter Assets References")]
    public StarterAssetsInputs starterInputs;

    public PlayerInput playerInput;

    public GameObject mainHUD;

    private bool isJournalOpen = false;

    void Update()
    {
        if (IsPlayerTyping())
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            return;
        }
        if (Keyboard.current != null)
        {
            // esc while typing in notes section
            if (IsPlayerTyping())
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }

                return;
            }

            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                ToggleJournal();
            }

            else if (isJournalOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ToggleJournal();
            }
        }
    }

    public void ToggleJournal()
    {
        isJournalOpen = !isJournalOpen;
        journalCanvas.SetActive(isJournalOpen);

        if (isJournalOpen)
        {
            Time.timeScale = 0f;

            if (starterInputs != null)
            {
                starterInputs.cursorInputForLook = false;
                starterInputs.cursorLocked = false;
            }

            if (mainHUD != null) mainHUD.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerInput != null) playerInput.enabled = false;
        }
        else
        {
            Time.timeScale = 1f;

            if (starterInputs != null)
            {
                starterInputs.cursorInputForLook = true;
                starterInputs.cursorLocked = true;
            }

            if (mainHUD != null) mainHUD.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerInput != null) playerInput.enabled = true;
        }
    }

    private bool IsPlayerTyping()
    {
        if (EventSystem.current == null) return false;

        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        if (selectedObj == null) return false;

        return selectedObj.GetComponent<TMP_InputField>() != null;
    }
}