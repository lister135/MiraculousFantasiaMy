using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NPCCollectibleItem : AInteractable
{
    public RelationshipItem itemData;
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found");
        }
    }

    public override void Interact()
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddItem(itemData);
            Debug.Log($"Picked up {itemData.itemName}");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("PlayerInventory not on the player.");
        }
    }
}