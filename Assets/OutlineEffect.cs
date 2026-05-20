using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class OutlineEffect : MonoBehaviour
{
    [Header("Settings")]
    public Color outlineColor = Color.white;
    [Range(0.001f, 0.05f)]
    public float outlineWidth = 0.015f;

    private Renderer   _renderer;
    private Material   _outlineMat;
    private Material[] _originalMats;
    private bool       _shown;

    void Awake()
    {
        _renderer     = GetComponent<Renderer>();
        _originalMats = _renderer.sharedMaterials;

        _outlineMat = new Material(Shader.Find("Custom/Outline"));
        _outlineMat.SetColor("_OutlineColor", outlineColor);
        _outlineMat.SetFloat("_OutlineWidth", outlineWidth);
    }

    public void Show()
    {
        if (_shown) return;
        _shown = true;
        var mats = new Material[_originalMats.Length + 1];
        System.Array.Copy(_originalMats, mats, _originalMats.Length);
        mats[_originalMats.Length] = _outlineMat;
        _renderer.sharedMaterials = mats;
    }

    public void Hide()
    {
        if (!_shown) return;
        _shown = false;
        _renderer.sharedMaterials = _originalMats;
    }

    void OnDestroy()
    {
        if (_shown) _renderer.sharedMaterials = _originalMats;
        if (_outlineMat != null) Destroy(_outlineMat);
    }
}
