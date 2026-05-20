using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("References")]
    public APIManager apiManager;
    public GameObject dialogueWindow;
    public GameObject hud;
    public PlayerInput playerInput;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    public void Open(string npcId)
    {
        if (IsOpen) return;
        IsOpen = true;

        dialogueWindow.SetActive(true);
        apiManager.BeginConversation(npcId);
        hud.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerInput != null) playerInput.enabled = false;
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        dialogueWindow.SetActive(false);
        hud.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInput != null) playerInput.enabled = true;
    }
}
