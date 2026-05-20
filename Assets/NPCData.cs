using System;

[Serializable]
public class NPCKeyEntry
{
    public int    id;
    public string description;      // what the LLM is told to reveal
    public string reveal_condition; // when/how it should be revealed
}

[Serializable]
public class TurningPointResponse
{
    public string turning_point_id; // matches an id in game_config.json
    public string prompt_addition;  // injected into this NPC's prompt when the point is active
}

[Serializable]
public class NPCData
{
    public string                   npc_id;
    public string                   npc_name;
    public string                   base_prompt;
    public NPCKeyEntry[]            keys;
    public TurningPointResponse[]   turning_point_responses;
}

[Serializable]
public class TurningPointConfig
{
    public string id;
    public string description;
    public int[]  required_keys; // all must be collected to trigger this point
}

[Serializable]
public class GameConfig
{
    public TurningPointConfig[] turning_points;
}
