using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.01f;
    [SerializeField] private TMP_Text textComponent; // Assign in Inspector
    // private TMP_Text textComponent;
    private Coroutine typingCoroutine;
    private string fullText = "";
    // Input Action
    private InputAction clickAction;

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
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void CompleteText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        textComponent.text = fullText;
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