using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("Inky JSON file")]
    [SerializeField] private TextAsset inkJSON;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        UIManager.instance.gameObject.SetActive(false);              
        DialogueManager.instance.OnDialogueComplete += EndDialogueTrigger;
        
        hasTriggered = true;
        DialogueManager.instance.EnterDialogueMode(inkJSON, true);
    }

    private void EndDialogueTrigger()
    {
        DialogueManager.instance.OnDialogueComplete -= EndDialogueTrigger;
        UIManager.instance.gameObject.SetActive(true);
    }
}