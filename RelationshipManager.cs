using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

[System.Serializable]
public struct Relationship
{
    public string charName;
    
    [Range(0, 100)]
    public int relationshipValue;
}

[System.Serializable]
public struct RelationshipData
{
    public List<Relationship> allRelationships;
}
public class RelationshipManager : MonoBehaviour
{
    public static RelationshipManager instance { get; private set; }
    public List<Relationship> relationships = new List<Relationship>();
    private Dictionary<string, int> relationshipDict = new Dictionary<string, int>();
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        foreach (var r in relationships)
            relationshipDict[r.charName] = r.relationshipValue;
    }

    public void IncreaseRelationship(string charName, int amount)
    {
        if (relationshipDict.TryGetValue(charName, out var relationshipValue))
        {
            relationshipValue = Mathf.Clamp(relationshipValue + amount, 0, 100);    
        }
        
    }
    
    public int ReturnRelationshipValue(string charName)
    {
        if (relationshipDict.TryGetValue(charName, out var relationshipValue))
        {
            return relationshipValue;
        }

        Debug.Log("This character does not exist.");
        return 0;
    }
    
    public void SavePlayer(ref RelationshipData data)
    {
        data.allRelationships = relationships;
    }

    public void LoadPlayer(RelationshipData data)
    {
        relationships = data.allRelationships;
    }

    public void ResetAllRelationships()
    {
        relationships.Clear();
        foreach (var r in relationships)
            relationshipDict[r.charName] = r.relationshipValue;
    }
}
