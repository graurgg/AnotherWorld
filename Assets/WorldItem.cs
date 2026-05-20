using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public static readonly List<WorldItem> All = new List<WorldItem>();

    public ItemData data;

    public float bobSpeed  = 1.4f;
    public float bobHeight = 0.09f;

    private float _baseY;
    private float _bobOffset;

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Start()
    {
        _baseY     = transform.position.y;
        _bobOffset = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        var p = transform.position;
        p.y = _baseY + Mathf.Sin(Time.time * bobSpeed + _bobOffset) * bobHeight;
        transform.position = p;
    }

    public void TryPickup()
    {
        if (InventoryManager.Instance == null) return;
        if (InventoryManager.Instance.TryAdd(data))
        {
            InventoryManager.Instance.ClearHoveredItem(this);
            Destroy(gameObject);
        }
    }
}
