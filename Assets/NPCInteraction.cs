using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueWindow;
    public GameObject mainHUD;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    private bool isDialogueOpen = false;

    public PlayerInput playerInput;

    void Start()
    {
        // Ensure the dialogue window is hidden when the game starts
        dialogueWindow.SetActive(false);
    }

    void Update()
    {
        // Only check for interaction if the dialogue isn't already open
        if (!isDialogueOpen)
        {
            CheckForNPC();
        }
        if (isDialogueOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseDialogue();
        }
    }

    void CheckForNPC()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    OpenDialogue();
                }
            }
        }
    }

    public void OpenDialogue()
    {
        isDialogueOpen = true;

        dialogueWindow.SetActive(true);
        if (mainHUD != null) mainHUD.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null) playerInput.enabled = false;
    }

    public void CloseDialogue()
    {
        isDialogueOpen = false;

        dialogueWindow.SetActive(false);
        if (mainHUD != null) mainHUD.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerInput != null) playerInput.enabled = true;
    }
}