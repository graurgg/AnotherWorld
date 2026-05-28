using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private readonly HashSet<int>    _collectedKeys   = new HashSet<int>();
    private readonly HashSet<string> _triggeredPoints = new HashSet<string>();
    private GameConfig               _config;

    public event Action<int>    OnKeyCollected;
    public event Action<string> OnTurningPointTriggered;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadConfig();
    }

    void LoadConfig()
    {
        var asset = Resources.Load<TextAsset>("NPCData/game_config");
        if (asset != null)
        {
            _config = JsonUtility.FromJson<GameConfig>(asset.text);
        }
        else
        {
            Debug.LogWarning("[GameState] Resources/NPCData/game_config.json not found.");
            _config = new GameConfig { turning_points = new TurningPointConfig[0] };
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void CollectKey(int keyId)
    {
        if (!_collectedKeys.Add(keyId)) return;
        Debug.Log("[GameState] Key collected: " + keyId);
        OnKeyCollected?.Invoke(keyId);
        EvaluateTurningPoints();
    }

    public void TriggerPoint(string pointId)
    {
        if (!_triggeredPoints.Add(pointId)) return;
        Debug.Log("[GameState] Turning point triggered: " + pointId);
        OnTurningPointTriggered?.Invoke(pointId);
    }

    public bool HasKey(int keyId)          => _collectedKeys.Contains(keyId);
    public bool HasTurningPoint(string id) => _triggeredPoints.Contains(id);

    public IReadOnlyCollection<int>    CollectedKeys      => _collectedKeys;
    public IReadOnlyCollection<string> TriggeredPoints    => _triggeredPoints;

    // ── Internal ──────────────────────────────────────────────────────────

    void EvaluateTurningPoints()
    {
        if (_config?.turning_points == null) return;

        foreach (var tp in _config.turning_points)
        {
            if (_triggeredPoints.Contains(tp.id))         continue;
            if (tp.required_keys == null || tp.required_keys.Length == 0) continue;
            if (!tp.required_keys.All(k => _collectedKeys.Contains(k)))   continue;

            _triggeredPoints.Add(tp.id);
            Debug.Log("[GameState] Turning point triggered: " + tp.id);
            OnTurningPointTriggered?.Invoke(tp.id);
        }
    }
}
