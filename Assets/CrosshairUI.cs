using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    public Image dot;

    [Range(0f, 1f)] public float baseAlpha  = 0.12f;
    [Range(0f, 1f)] public float nearAlpha  = 0.90f;
    public float screenFadeRadius = 130f; // pixels from screen center
    public float maxWorldRange    = 6f;   // ignore items beyond this distance
    public float lerpSpeed        = 10f;

    private Camera _cam;
    private float  _currentAlpha;

    void Awake()
    {
        _cam = Camera.main;
        _currentAlpha = baseAlpha;
    }

    void Update()
    {
        float targetAlpha = baseAlpha;

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float minScreenDist = float.MaxValue;

        foreach (WorldItem wi in WorldItem.All)
        {
            if (wi == null) continue;
            if (Vector3.Distance(_cam.transform.position, wi.transform.position) > maxWorldRange) continue;

            Vector3 sp = _cam.WorldToScreenPoint(wi.transform.position);
            if (sp.z < 0f) continue;

            float d = Vector2.Distance(new Vector2(sp.x, sp.y), center);
            if (d < minScreenDist) minScreenDist = d;
        }

        foreach (NPCAgent npc in NPCAgent.All)
        {
            if (npc == null) continue;
            if (Vector3.Distance(_cam.transform.position, npc.transform.position) > maxWorldRange) continue;

            Vector3 sp = _cam.WorldToScreenPoint(npc.transform.position);
            if (sp.z < 0f) continue;

            float d = Vector2.Distance(new Vector2(sp.x, sp.y), center);
            if (d < minScreenDist) minScreenDist = d;
        }

        if (minScreenDist < screenFadeRadius)
        {
            float t = 1f - (minScreenDist / screenFadeRadius);
            targetAlpha = Mathf.Lerp(baseAlpha, nearAlpha, t);
        }

        _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, Time.deltaTime * lerpSpeed);
        dot.color = new Color(1f, 1f, 1f, _currentAlpha);
    }
}
