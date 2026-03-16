using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

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
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
        {
            ToggleJournal();
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
}