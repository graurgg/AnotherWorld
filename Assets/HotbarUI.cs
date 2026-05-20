using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [System.Serializable]
    public struct SlotUI
    {
        public Image background;
        public Image itemIcon;
    }

    [Header("Slots")]
    public SlotUI[] slots = new SlotUI[3];

    [Header("Drop hint")]
    public GameObject dropHint;

    [Header("Hover tooltip")]
    public GameObject tooltip;
    public TextMeshProUGUI tooltipName;
    public Image tooltipIcon;

    [Header("Slot name popup")]
    public TextMeshProUGUI slotNamePopup;
    public float popupHoldTime = 1.4f;
    public float popupFadeTime = 0.4f;

    static readonly Color ColorIdle     = new Color(0.15f, 0.15f, 0.15f, 0.55f);
    static readonly Color ColorSelected = new Color(0.45f, 0.45f, 0.45f, 0.80f);

    private Coroutine _popupCoroutine;
    private NPCAgent  _lastHoveredNPC;

    void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += Refresh;
            InventoryManager.Instance.OnSlotActivated   += OnSlotActivated;
        }
        Refresh();
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
            InventoryManager.Instance.OnSlotActivated   -= OnSlotActivated;
        }
    }

    void Update()
    {
        var currentNPC = NPCInteraction.Instance?.HoveredNPC;
        if (!ReferenceEquals(currentNPC, _lastHoveredNPC))
        {
            _lastHoveredNPC = currentNPC;
            Refresh();
        }
    }

    void OnSlotActivated(ItemData item) => TriggerPopup(item.itemName);

    void Refresh()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            bool selected = inv.selectedSlot == i;
            bool hasItem  = inv.slots[i] != null;

            slots[i].background.color = selected ? ColorSelected : ColorIdle;

            if (slots[i].itemIcon != null)
            {
                slots[i].itemIcon.enabled = hasItem;
                if (hasItem) slots[i].itemIcon.sprite = inv.slots[i].sprite;
            }
        }

        if (dropHint != null)
            dropHint.SetActive(inv.selectedSlot >= 0);

        if (tooltip != null)
        {
            bool showItem = inv.hoveredItem != null && inv.hoveredItem.data != null;
            var  npc      = NPCInteraction.Instance?.HoveredNPC;
            bool showNPC  = !showItem && npc != null;

            tooltip.SetActive(showItem || showNPC);

            if (showItem)
            {
                if (tooltipName != null) tooltipName.text = inv.hoveredItem.data.itemName;
                if (tooltipIcon != null)
                {
                    tooltipIcon.gameObject.SetActive(true);
                    tooltipIcon.sprite  = inv.hoveredItem.data.sprite;
                    tooltipIcon.enabled = inv.hoveredItem.data.sprite != null;
                }
            }
            else if (showNPC)
            {
                string name = string.IsNullOrWhiteSpace(npc.displayName) ? npc.npcId : npc.displayName;
                if (tooltipName != null) tooltipName.text = name;
                if (tooltipIcon != null) tooltipIcon.gameObject.SetActive(false);
            }
        }

    }

    void TriggerPopup(string itemName)
    {
        if (slotNamePopup == null) return;
        if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
        _popupCoroutine = StartCoroutine(PopupRoutine(itemName));
    }

    void CancelPopup()
    {
        if (slotNamePopup == null) return;
        if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
        slotNamePopup.gameObject.SetActive(false);
    }

    IEnumerator PopupRoutine(string itemName)
    {
        slotNamePopup.text = itemName;
        slotNamePopup.gameObject.SetActive(true);

        var c = slotNamePopup.color;
        c.a = 1f;
        slotNamePopup.color = c;

        yield return new WaitForSeconds(popupHoldTime);

        float elapsed = 0f;
        while (elapsed < popupFadeTime)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / popupFadeTime);
            slotNamePopup.color = c;
            yield return null;
        }

        slotNamePopup.gameObject.SetActive(false);
    }
}
