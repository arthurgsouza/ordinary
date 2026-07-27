using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.01f;
    [SerializeField] private TMP_Text textComponent; // Assign in Inspector
    // private TMP_Text textComponent;
    private Coroutine typingCoroutine;
    private string fullText = "";
    // Input Action
    private InputAction clickAction;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Looped while the text is being typed out, stopped as soon as typing ends")]
    [SerializeField] private AudioClip typingSound;

    private void Awake()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError("TypewriterEffect requires a TMP_Text component!");
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        clickAction = new InputAction(type: InputActionType.Button, binding: "<Pointer>/press");
        clickAction.Enable();
    }

    public void StartTyping(string text)
    {
        if (textComponent == null) return;

        // Stop any ongoing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        fullText = text;
        textComponent.text = ""; // Clear the text initially
        PlayTypingSound();
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
        StopTypingSound();
    }

    public void CompleteText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        textComponent.text = fullText;
        StopTypingSound();
    }

    private void PlayTypingSound()
    {
        if (audioSource == null || typingSound == null) return;
        audioSource.clip = typingSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void StopTypingSound()
    {
        if (audioSource == null) return;
        audioSource.Stop();
    }

    public bool IsTyping()
    {
        return typingCoroutine != null;
    }

    /*
    private void Update()
    {
        if (IsTyping() && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            CompleteText();
        }
    }*/

    private void Update()
    {
        if (IsTyping() && clickAction.WasPressedThisFrame())
            CompleteText();
    }
    
}