using UnityEngine;

public static class NPCDataLoader
{
    public static NPCData Load(string npcId)
    {
        var asset = Resources.Load<TextAsset>("NPCData/" + npcId);
        if (asset == null)
        {
            Debug.LogWarning("[NPCDataLoader] No JSON found for NPC id: " + npcId);
            return null;
        }
        return JsonUtility.FromJson<NPCData>(asset.text);
    }
}
