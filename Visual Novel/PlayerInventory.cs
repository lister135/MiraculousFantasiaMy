using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // A list to store items in the player's inventory

    public List<RelationshipItem> items = new List<RelationshipItem>();

    public void AddItem(RelationshipItem newItem)
    {
        // Add the item to the inventory list
        items.Add(newItem);
        Debug.Log($"Added {newItem.itemName} to inventory.");
    }
    public void GiveItemToNPC(RelationshipItem item, string charName)
    {
        // Check if the item can only be given to a specific NPC
        if (!string.IsNullOrEmpty(item.allowedNPC))
        {
            if (charName != item.allowedNPC)
            {
                Debug.LogWarning($"{item.itemName} are not for {charName}.");
                return;
            }
        }
        // Check if the item is in the player's inventory
        if (items.Contains(item))
        {
            // Increase the relationship value of the NPC by the assigned relationship value on the item
            RelationshipManager.instance.IncreaseRelationship(charName, item.relationshipBonus);
            // Remove the item from the player's inventory
            items.Remove(item);
            Debug.Log($"Gave {item.itemName} to {charName}.");
        }
        else
        {
            Debug.LogWarning("Item not in inventory.");
        }
    }
}
