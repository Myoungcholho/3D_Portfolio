using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public static NpcManager Instance;

    private Dictionary<NpcList, Npc> npcDictionary = new Dictionary<NpcList, Npc>();

    private void Awake()
    {
        Instance = this;

        foreach (Npc npc in FindObjectsOfType<Npc>())
        {
            npcDictionary[npc.npcID] = npc;
        }
    }

    public Npc GetNpc(NpcList npcID)
    {
        if (npcDictionary.TryGetValue(npcID, out Npc npc))
        {
            return npc;
        }

        return null;
    }
}
