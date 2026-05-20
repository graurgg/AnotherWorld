using System.Collections.Generic;
using UnityEngine;

public class NPCAgent : MonoBehaviour
{
    public static readonly List<NPCAgent> All = new List<NPCAgent>();

    public string npcId;
    public string displayName;

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }
}
