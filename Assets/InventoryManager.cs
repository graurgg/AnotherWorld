using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Settings")]
    public float pickupRange = 2.5f;
    public GameObject worldItemPrefab;

    public ItemData[] slots = new ItemData[3];
    public int selectedSlot = -1;
    public WorldItem hoveredItem { get; private set; }

    public event Action OnInventoryChanged;
    public event Action<ItemData> OnSlotActivated;

    private Camera _cam;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _cam = Camera.main;
    }

    void Update()
    {
        UpdateHover();

        if (Time.timeScale == 0f || IsPlayerTyping()) return;
        if (DialogueController.Instance != null && DialogueController.Instance.IsOpen) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) TrySelectSlot(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) TrySelectSlot(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) TrySelectSlot(2);

        if (Keyboard.current.qKey.wasPressedThisFrame) DropSelected();
        if (Keyboard.current.eKey.wasPressedThisFrame) TryPickupFromWorld();
    }

    void UpdateHover()
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        WorldItem detected = null;
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
            detected = hit.collider.GetComponent<WorldItem>();

        if (detected == hoveredItem) return;

        hoveredItem?.GetComponent<OutlineEffect>()?.Hide();
        hoveredItem = detected;
        hoveredItem?.GetComponent<OutlineEffect>()?.Show();
        OnInventoryChanged?.Invoke();
    }

    void TrySelectSlot(int index)
    {
        if (slots[index] == null) return;
        selectedSlot = index;
        OnSlotActivated?.Invoke(slots[index]);
        OnInventoryChanged?.Invoke();
    }

    public bool TryAdd(ItemData item)
    {
        if (item == null) return false;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                if (selectedSlot == -1) selectedSlot = i;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    void DropSelected()
    {
        if (selectedSlot < 0 || slots[selectedSlot] == null) return;

        ItemData item = slots[selectedSlot];
        slots[selectedSlot] = null;
        selectedSlot = -1;
        OnInventoryChanged?.Invoke();

        SpawnWorldItem(item, GetDropPosition());
    }

    void TryPickupFromWorld()
    {
        if (hoveredItem != null)
            hoveredItem.TryPickup();
    }

    public void ClearHoveredItem(WorldItem item)
    {
        if (!ReferenceEquals(hoveredItem, item)) return;
        hoveredItem = null;
        OnInventoryChanged?.Invoke();
    }

    Vector3 GetDropPosition()
    {
        Vector3 forward = _cam.transform.forward;
        forward.y = 0f;
        if (forward == Vector3.zero) forward = _cam.transform.forward;
        forward.Normalize();

        Vector3 origin = _cam.transform.position + forward * 1.5f;

        if (Physics.Raycast(origin + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
            return hit.point + Vector3.up * 0.25f;

        return origin;
    }

    void SpawnWorldItem(ItemData item, Vector3 position)
    {
        if (worldItemPrefab == null) return;
        var go = Instantiate(worldItemPrefab, position, Quaternion.identity);
        go.GetComponent<WorldItem>().data = item;
    }

    bool IsPlayerTyping()
    {
        if (EventSystem.current == null) return false;
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return false;
        return selected.GetComponent<TMP_InputField>() != null;
    }
}
