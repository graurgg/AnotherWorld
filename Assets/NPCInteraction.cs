using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    public static NPCInteraction Instance { get; private set; }

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;

    public NPCAgent HoveredNPC { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        UpdateHover();
        if (DialogueController.Instance == null || DialogueController.Instance.IsOpen) return;
        if (HoveredNPC != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            DialogueController.Instance.Open(HoveredNPC.npcId);
    }

    void UpdateHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        NPCAgent detected = null;
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            if (hit.collider.CompareTag("NPC"))
                detected = hit.collider.GetComponent<NPCAgent>();

        if (detected == HoveredNPC) return;

        HoveredNPC?.GetComponent<OutlineEffect>()?.Hide();
        HoveredNPC = detected;
        HoveredNPC?.GetComponent<OutlineEffect>()?.Show();
    }
}
