using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class RelationshipItem
{
    public string itemName;
    
    [Tooltip("Amount added NPC relationship")]
    public int relationshipBonus;

    [Tooltip("Gift for which NPC")]
    public string allowedNPC;
}
