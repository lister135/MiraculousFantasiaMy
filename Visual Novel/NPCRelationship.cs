using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DEPRECATED

public class NPCRelationship : MonoBehaviour
{
    [SerializeField, Range(0, 100)]
    private int relationshipValue = 0;

    public int RelationshipValue => relationshipValue;

    public void IncreaseRelationship(int amount)
    {
        relationshipValue = Mathf.Clamp(relationshipValue + amount, 0, 100);
        Debug.Log($"{gameObject.name} relationship increased to {relationshipValue}");
    }
}