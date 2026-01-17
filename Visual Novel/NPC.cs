using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

[Serializable]
public struct FlagTriggeredDialogue
{
    public string flagName;
    public TextAsset ink;
}

public class NPC : AInteractable
{
    [Header("Ink Story Asset")] 
    [SerializeField] private List<FlagTriggeredDialogue> flagTriggeredDialogues = new List<FlagTriggeredDialogue>();
    [SerializeField] private TextAsset lowRelInk;
    [SerializeField] private TextAsset midRelInk;
    [SerializeField] private TextAsset highRelInk;
    [SerializeField] private TextAsset giftInk;
    
    [Header("Dialogue Controls")]
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private string charName;

    [Header("References")]
    private Dictionary<string, TextAsset> flagInks = new Dictionary<string, TextAsset>();
    private Animator animator;
    private PlayerInventory playerInv;
    
    private void Awake()
    {
        foreach (FlagTriggeredDialogue d in flagTriggeredDialogues)
            flagInks[d.flagName] = d.ink;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        isInteractable = false;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerInv = player.GetComponent<PlayerInventory>();
    }

    public override void Interact()
    {
        if (!DialogueManager.instance.dialogueIsPlaying)
            BeginInkDialogue();
    }
    // Prepare dialogue environment and chooses the right Ink text based on conditions
    private void BeginInkDialogue()
    {
        UIManager.instance.gameObject.SetActive(false);
        DialogueManager.instance.OnDialogueComplete += EndInkDialogue;

        int rel = RelationshipManager.instance.ReturnRelationshipValue(charName);

        // Determines Ink text based on relationship value with NPC
        TextAsset chosenInk;
        if (rel < 25)
            chosenInk = lowRelInk;
        else if (rel < 51)
            chosenInk = midRelInk;
        else
            chosenInk = highRelInk;

        // Checks for active flags and overrides current Ink text if a flagged dialogue is available
        foreach (Flag f in ProgressChecker.instance.flags)
        {
            if (!ProgressChecker.instance.IsFlagAvailable(f.flagName))
                continue;
            
            if (flagInks.TryGetValue(f.flagName, out var result))
            {
                chosenInk = result;
                break;
            }
        }

        // Checks for gifts in player's inventory and updates ink text if there is a gift
        var gift = playerInv.items
            .Find(i => string.IsNullOrEmpty(i.allowedNPC)
                       || i.allowedNPC == gameObject.name);

        if (gift != null)
        {
            playerInv.GiveItemToNPC(gift, charName);
            chosenInk = giftInk;
            Debug.Log($"{gift.itemName} given to {gameObject.name}!");
        }
        else
        {
               
            Debug.Log($"No gift detected");
        }

        // Enter dialogue mode with the right ink text
        DialogueManager.instance.EnterDialogueMode(chosenInk, false);
    }

    // Cleans up UI after dialogue ends
    private void EndInkDialogue()
    {
        DialogueManager.instance.OnDialogueComplete -= EndInkDialogue;
        
        UIManager.instance.gameObject.SetActive(true);
    }

    // Checks if player is within range to speak with NPC
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (animator) 
                animator.SetBool("isSmiling", true);
        
            if (speechBubble) 
                speechBubble.GetComponent<SpriteRenderer>().enabled = true;
            
            other.gameObject.GetComponent<InteractManager>().addInteractable(this);
        }
    }
    
    //Check if player leaves the range to talk to NPC
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (animator)
                animator.SetBool("isSmiling", false);
        
            if (speechBubble) 
                speechBubble.GetComponent<SpriteRenderer>().enabled = false;
            
            other.gameObject.GetComponent<InteractManager>().removeInteractable(this);
        }
    }
}