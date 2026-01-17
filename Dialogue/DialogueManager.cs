using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
public struct SpriteMapping
{
    public string tagKey;
    public Texture2D sprite;
}

[Serializable]
public struct FontMapping
{
    public string charName;
    public TMP_FontAsset font;
}

[Serializable]
public struct TimelineMapping
{
    public string timelineName;
    public PlayableDirector timeline;
}
// Handles dialogue system, including UI updates, story assets and taking in inky inputs
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance { get; private set; }

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject spriteImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private GameObject dialogueContinue;
    [SerializeField] private float textDisplaySpeed = 0.05f;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;
    
    [Header("Mappings")]
    [SerializeField] private SpriteMapping[] spriteMappings;
    [SerializeField] private FontMapping[] fontMappings;
    [SerializeField] private TimelineMapping[] timelineMappings;
    private Dictionary<string, Texture2D> _spriteDict = new Dictionary<string, Texture2D>();
    private Dictionary<string, TMP_FontAsset> _fontDict = new Dictionary<string, TMP_FontAsset>();
    private Dictionary<string, PlayableDirector> _timelineDict = new Dictionary<string, PlayableDirector>();

    private bool DialogueCanAdvance = true;

    private Story currentStory;

    private bool isTextPlaying = false;
    private bool skipDialogue = false;
    
    private GameObject player;
    private APlayerMovement movement;

    private bool isTimelinePlaying; 

    [SerializeField] private InputActionReference continueDialogue, continueDialogueAlt;
    public bool dialogueIsPlaying { get; private set; }
    public event Action OnDialogueComplete;

    private void Start()
    {
        isTimelinePlaying = false;
        
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        dialogueIsPlaying = false;
        spriteImage.SetActive(false);
        dialoguePanel.SetActive(false);

        dialogueText.text = "";
        spriteImage.GetComponent<RawImage>().texture = null;
        
        player = GameObject.FindGameObjectWithTag("Player");
        if (!player)
            Debug.Log("No player class found");
        
        movement = player.GetComponent<APlayerMovement>();
        
        choicesText = new TextMeshProUGUI[choices.Length];
        for (int i = 0; i < choices.Length; i++)
            choicesText[i] = choices[i].GetComponentInChildren<TextMeshProUGUI>();

        foreach (var map in spriteMappings)
        {
            _spriteDict[map.tagKey] = map.sprite;
        }

        
        foreach (var map in fontMappings)
        {
            _fontDict[map.charName] = map.font;
        }
        
        foreach (var map in timelineMappings)
        {
            _timelineDict[map.timelineName] = map.timeline;
        }
    }

    // Enable input actions for continuing dialogue
    private void OnEnable()
    {
        continueDialogue.action.Enable();
        continueDialogueAlt.action.Enable();
        continueDialogue.action.performed += OnContinueInput;
        continueDialogueAlt.action.performed += OnContinueInput;
    }

    // Disable input actions and remove event listeners
    private void OnDisable()
    {
        continueDialogue.action.Disable();
        continueDialogueAlt.action.Disable();
        continueDialogue.action.performed -= OnContinueInput;
        continueDialogueAlt.action.performed -= OnContinueInput;
    }

    // Advance dialogue if dialogue is playing and can be advanced
    private void OnContinueInput(InputAction.CallbackContext context)
    {
        if (!dialogueIsPlaying) return;

        if (DialogueCanAdvance)
        {
            ContinueStory();
        }
    }

    // Enter dialogue mode and load Ink text
    public void EnterDialogueMode(TextAsset inkJSON, bool skipFirstLine)
    {
        currentStory = new Story(inkJSON.text);
        
        movement.isMovementFrozen = true;
        SpellManager.instance.canPerformSpells = false;
        InteractManager.instance.canInteract = false;
        dialogueIsPlaying = true;
        
        EnableDialogueUI();
        
        if (skipFirstLine || string.IsNullOrWhiteSpace(dialogueText.text)
            && spriteImage.GetComponent<RawImage>().texture == null)
        {
            ContinueStory();
        }
    }
    // Show dialogue UI and disable other UI
    public void EnableDialogueUI()
    {
        spriteImage.SetActive(true);
        dialoguePanel.SetActive(true);
        StateManager.instance.canReset = false;
        UIManager.instance.gameObject.SetActive(false);
    }

    // Exit dialogue mode, enable and reset other UI
    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);
        dialogueIsPlaying = false;
        spriteImage.SetActive(false);
        movement.isMovementFrozen = false;
        StateManager.instance.canReset = true;
        dialoguePanel.SetActive(false);
        UIManager.instance.gameObject.SetActive(true);
        SpellManager.instance.canPerformSpells = true;
        InteractManager.instance.canInteract = true;
        dialogueText.text = "";
        OnDialogueComplete?.Invoke();
    }

    // Advances dialogue and handle text display, NPC sprites, and tags.
    private void ContinueStory()
    {
        if (isTextPlaying || isTimelinePlaying) 
            return;
        
        if (currentStory.canContinue)
        {
            dialogueContinue.gameObject.SetActive(false);
            string nextLine = SkipEmptyLines();

            StartCoroutine(DisplayTextOneByOne(nextLine));

            string NPCName = " ";

            foreach (string tag in currentStory.currentTags)
            {
                if (tag.StartsWith("sprite:"))
                {
                    string key = tag.Substring("sprite:".Length);
                    if (_spriteDict.TryGetValue(key, out var tex))
                    {
                        spriteImage.GetComponent<RawImage>().texture = tex;
                    }
                    else 
                    {
                        Debug.LogWarning($"No sprite for tag '{key}'");
                    }
                }
                
                if (tag.StartsWith("speaker:"))
                {
                    NPCName = tag.Substring("speaker:".Length);
                }
                
                if (tag.StartsWith("timeline:"))
                {
                    string key = tag.Substring("timeline:".Length);
                    if (_timelineDict.TryGetValue(key, out var timeline))
                    {
                        timeline.Play();
                        StartCoroutine(TimelinePlayingMode());
                    }
                    else 
                    {
                        Debug.LogWarning($"No timeline found for tag '{key}'");
                    }
                }

                if (tag.StartsWith("flag:"))
                {
                    string key = tag.Substring("flag:".Length);
                    ProgressChecker.instance.ToggleFlag(key);
                    StateManager.instance.CheckFlags();
                }
            }
            
            NPCNameText.text = NPCName;
            if (_fontDict.TryGetValue(NPCName.ToLower(), out var font))
                dialogueText.font = font;   
            
            DisplayChoices();
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }
    // Skips over empty lines if there is any in the text
    private string SkipEmptyLines()
    {
        while (currentStory.canContinue)
        {
            string line = currentStory.Continue();

            if (!string.IsNullOrWhiteSpace(line))
                return line.Trim();
        }

        return null;
    }
    // Manages state if a timeline cutscene is playing
    private IEnumerator TimelinePlayingMode()
    {
        float startTime = Time.time;
        isTimelinePlaying = true;

        yield return new WaitForSeconds(2f);
        isTimelinePlaying = false;

    }
    // Displays text character one by one with an option to skip
    private IEnumerator DisplayTextOneByOne(string fullText)
    {
        isTextPlaying = true;
        dialogueText.text = "";
        char[] characters = fullText.ToCharArray();

        /*StartCoroutine(CheckForSkip(() => skip = true));*/

        continueDialogue.action.performed += SkipDialogueInput;
        continueDialogueAlt.action.performed += SkipDialogueInput;
        
        foreach (char c in characters)
        {
            if (skipDialogue)
            {
                dialogueText.text = fullText;
                break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(textDisplaySpeed);
        }
        
        continueDialogue.action.performed -= SkipDialogueInput;
        continueDialogueAlt.action.performed -= SkipDialogueInput;
        skipDialogue = false;
        
        isTextPlaying = false;
        dialogueContinue.gameObject.SetActive(true);
        DisplayChoices();
       
    }
    // Enables skipping the current dialogue
    private void SkipDialogueInput(InputAction.CallbackContext context)
    {
        skipDialogue = true;
    }

    /*private IEnumerator CheckForSkip(Action skipAction)
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                skipAction();
                yield break;
            }
            yield return null;
        }
    }*/

    // Displays dialogue branching choices and sets up choice buttons
    private void DisplayChoices()
    {
        var currentChoices = currentStory.currentChoices;

        DialogueCanAdvance = (currentChoices.Count == 0);

        for (int i = 0; i < choices.Length; i++)
        {
            var btn = choices[i].GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            if (i < currentChoices.Count)
            {
                int choiceIndex = i;
                btn.onClick.AddListener(() => MakeChoice(choiceIndex));
                choicesText[i].text = currentChoices[i].text;
                btn.gameObject.SetActive(true);
            }
            else btn.gameObject.SetActive(false);
        }
    }

    /*private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }*/

    // Handle the player making dialogue choices
    public void MakeChoice(int choiceIndex)
    {
        DialogueCanAdvance = true;
        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
    }
}