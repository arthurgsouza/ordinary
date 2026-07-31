
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using TMPro;
[RequireComponent(typeof(AudioSource))]
public class CardSwiper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [Header("Drag Hint")]
    public TMP_Text dragHintText;     // assign in Inspector
    [SerializeField] private RectTransform dragHintContainer;


    [Header("Swipe Settings")]
    public float swipeThreshold = 150f;   // how far you must drag
    public float offscreenOffsetX = 1000f;  // horizontal distance off‑screen
    public float offscreenOffsetY = -200f;  // vertical drift when swiping away
    public float swipeAnimDuration = 0.3f;   // how long the swipe animation takes
    public float maxRotationDegrees = 15f;    // how much to tilt at full drag

    [Header("Card Assets")]
    [Tooltip("Portrait Settings")]
    public Image portraitImage; // Assign the Portrait's Image component here
    
    [Tooltip("Reference to the TextMeshProUGUI component for the dialogue text")]
    public TMP_Text dialogueText; // Assign the Dialogue TextMeshProUGUI here
    [Tooltip("Reference to the TextMeshProUGUI component for the name tag")]
    public TMP_Text nameTagText; // Assign the NameTag TextMeshProUGUI here

   
    [Header("Card Manager")]
    [SerializeField] private CardManager cardManager;

    [Header("Typewriter Effect")]
    public TypewriterEffect typewriterEffect; // Assign in Inspector

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Played as soon as the player starts dragging the card")]
    [SerializeField] private AudioClip swipeStartSound;
    [Tooltip("Played once the swipe threshold is reached and a decision is committed")]
    [SerializeField] private AudioClip decisionSound;

    [Header("Idle Swipe Hint")]
    [Tooltip("Seconds of no player input before the demo swipe plays")]
    [SerializeField] private float idleHintDelay = 9f;
    [Tooltip("How far the card nudges out during the demo, in the same units as swipeThreshold")]
    [SerializeField] private float hintSwipeDistance = 200f;
    [Tooltip("Duration of each leg (out or back) of the demo nudge")]
    [SerializeField] private float hintSwipeDuration = 0.9f;

    private RectTransform rt;
    private Vector2 startPos;
    private float startRotZ;
    // private int currentSpriteIndex = 0; // Track the current sprite index

    // Tracks the coroutine so it can be cancelled, and whether the player has ever swiped/dragged
    // for real, since the hint should only ever play once, before the first real interaction.
    // Static: the scene wires more than one CardSwiper to the same dragHintText/dragHintContainer,
    // so a real swipe on one instance must stop every instance's idle hint, not just its own.
    private Coroutine idleHintCoroutine;
    private static bool hasInteracted = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Start()
    {
        // remember where we began (position + rotation)
        startPos = rt.anchoredPosition;
        startRotZ = rt.localEulerAngles.z;
        dragHintText.gameObject.SetActive(false);
        dragHintContainer.gameObject.SetActive(false);

        // Initialize the UI with the first card's data
        UpdateCardUI();

        // Teach new players the swipe gesture if they don't touch the card in time.
        if (!hasInteracted)
        {
            idleHintCoroutine = StartCoroutine(IdleSwipeHintRoutine());
        }
    }

    // show as soon as the user starts dragging
    public void OnBeginDrag(PointerEventData eventData)
    {
        CancelIdleHint(); // A real drag started, so the demo must never play (or continue playing)

        dragHintText.gameObject.SetActive(true);
        dragHintContainer.gameObject.SetActive(true);
        dragHintText.text = ""; // Clear the text initially

        PlaySound(swipeStartSound);
    }


    public void OnDrag(PointerEventData eventData)
    {
        // Move in X only
        rt.anchoredPosition += new Vector2(eventData.delta.x, 0);

        // Tilt proportional to drag distance
        float deltaX = rt.anchoredPosition.x - startPos.x;
        float rotZ = Mathf.Clamp(
                            (deltaX / swipeThreshold) * -maxRotationDegrees,
                            -maxRotationDegrees,
                            maxRotationDegrees
                        );
        rt.localEulerAngles = new Vector3(0, 0, rotZ);

        // Update the drag hint text based on drag direction
        if (deltaX > 0) // Dragging to the right
        {
            dragHintText.text = cardManager.GetCurrentCard()?.rightAnswer ?? "No right answer";
        }
        else if (deltaX < 0) // Dragging to the left
        {
            dragHintText.text = cardManager.GetCurrentCard()?.leftAnswer ?? "No left answer";
        }
    }

    
    public void OnEndDrag(PointerEventData eventData)
    {
        dragHintContainer.gameObject.SetActive(false);
        dragHintText.gameObject.SetActive(false);

        float deltaX = rt.anchoredPosition.x - startPos.x;
        if (Mathf.Abs(deltaX) >= swipeThreshold)
        {
            PlaySound(decisionSound);

            if (deltaX > 0)
            {
                StartCoroutine(SwipeAway(new Vector2(offscreenOffsetX, offscreenOffsetY), maxRotationDegrees, OnSwipedRight));
            }
            else
            {
                StartCoroutine(SwipeAway(new Vector2(-offscreenOffsetX, offscreenOffsetY), -maxRotationDegrees, OnSwipedLeft));
            }
        }
        else
        {
            StartCoroutine(SmoothReset()); // Animate back
        }
    }

    IEnumerator SwipeAway(Vector2 offset, float targetRotZ, System.Action onComplete)
    {
        Vector2 initialPos = rt.anchoredPosition;
        float initialRot = rt.localEulerAngles.z;
        Vector2 finalPos = startPos + offset;
        float elapsed = 0f;

        while (elapsed < swipeAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swipeAnimDuration;

            // Lerp position
            rt.anchoredPosition = Vector2.Lerp(initialPos, finalPos, t);
            // Lerp rotation
            float z = Mathf.LerpAngle(initialRot, targetRotZ, t);
            rt.localEulerAngles = new Vector3(0, 0, z);


            yield return null;
        }

        // ensure exact final state
        rt.anchoredPosition = finalPos;
        rt.localEulerAngles = new Vector3(0, 0, targetRotZ);

        onComplete?.Invoke();
    }
    void OnSwipedRight()
    {
        if (cardManager != null)
        {
            cardManager.OnSwipeRight(); // Notify the CardManager about the right swipe
        }
        // Instead of just UpdateCardUI(), use the flip animation
        UpdateCardUI(); // Refresh the UI
        ResetCard();
    }

    void OnSwipedLeft()
    {
        if (cardManager != null)
        {
            cardManager.OnSwipeLeft(); // Notify the CardManager about the left swipe
        }
        // Instead of just UpdateCardUI(), use the flip animation
        UpdateCardUI(); // Refresh the UI
        ResetCard();
    }


    void ResetCard()
    {
        StartCoroutine(SmoothReset());
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }


IEnumerator SmoothReset()
{
    Vector2 fromPos = rt.anchoredPosition;
    float fromRot = rt.localEulerAngles.z;
    float elapsed = 0f;
    float duration = 0.02f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.SmoothStep(0, 1, elapsed / duration);
        rt.anchoredPosition = Vector2.Lerp(fromPos, startPos, t);
        float z = Mathf.LerpAngle(fromRot, startRotZ, t);
        rt.localEulerAngles = new Vector3(0, 0, z);
        yield return null;
    }

    rt.anchoredPosition = startPos;
    rt.localEulerAngles = new Vector3(0, 0, startRotZ);
}
    
    // 21/07/2025 AI-Tag
    // This was created with the help of Assistant, a Unity Artificial Intelligence product.

    public void UpdateCardUI()
    {
        Debug.Log("Updating Card UI...");
        if (cardManager != null)
        {
            CardData currentCard = cardManager.GetCurrentCard();
            if (currentCard != null)
            {
                // Update the portrait image
                if (portraitImage != null)
                {
                    portraitImage.sprite = currentCard.portrait;
                }

                if (typewriterEffect != null && dialogueText != null)
                {
                    typewriterEffect.StartTyping(currentCard.promptText ?? "No prompt available");
                }
                else if (dialogueText != null)
                {
                    dialogueText.text = currentCard.promptText ?? "No prompt available";
                }

                // dialogueText.text = currentCard.promptText; // Assign the Dialogue TextMeshProUGUI her
                nameTagText.text = currentCard.characterName; // Assign the NameTag TextMeshProUGUI here 

                // Update other UI elements (if needed)
                // Example: Update drag hint text or other visuals
            }
        }
    }

    // Stops the idle demo (if it hasn't played yet or is mid-animation) and makes sure
    // it can never start again for the rest of the session.
    private void CancelIdleHint()
    {
        if (hasInteracted) return;
        hasInteracted = true;

        if (idleHintCoroutine != null)
        {
            StopCoroutine(idleHintCoroutine);
            idleHintCoroutine = null;
        }

        // The ghost demo may have been mid-flight; snap the card back before the real drag takes over.
        rt.anchoredPosition = startPos;
        rt.localEulerAngles = new Vector3(0, 0, startRotZ);
        dragHintContainer.gameObject.SetActive(false);
        dragHintText.gameObject.SetActive(false);
    }

    // Waits for the configured idle delay, then demonstrates a right swipe followed by a left swipe.
    private IEnumerator IdleSwipeHintRoutine()
    {
        yield return new WaitForSeconds(idleHintDelay);
        if (hasInteracted) { idleHintCoroutine = null; yield break; }

        yield return GhostSwipe(hintSwipeDistance, isRight: true);
        if (hasInteracted) { idleHintCoroutine = null; yield break; }
        yield return GhostSwipe(-hintSwipeDistance, isRight: false);

        idleHintCoroutine = null;
    }

    // Nudges the card toward offsetX and eases it back to center, purely as a visual demonstration.
    // This never calls cardManager.OnSwipeRight/Left, so it can't consume a card or apply any effects.
    private IEnumerator GhostSwipe(float offsetX, bool isRight)
    {
        if (hasInteracted) yield break;

        CardData currentCard = cardManager?.GetCurrentCard();
        dragHintText.text = isRight ? (currentCard?.rightAnswer ?? "") : (currentCard?.leftAnswer ?? "");
        dragHintContainer.gameObject.SetActive(true);
        dragHintText.gameObject.SetActive(true);

        Vector2 targetPos = startPos + new Vector2(offsetX, 0);
        float targetRotZ = Mathf.Clamp(offsetX / swipeThreshold * -maxRotationDegrees, -maxRotationDegrees, maxRotationDegrees);

        yield return TweenCard(startPos, targetPos, startRotZ, targetRotZ, hintSwipeDuration);
        yield return TweenCard(targetPos, startPos, targetRotZ, startRotZ, hintSwipeDuration);

        dragHintContainer.gameObject.SetActive(false);
        dragHintText.gameObject.SetActive(false);
    }

    // Shared position/rotation lerp for the idle hint, kept separate from SwipeAway/SmoothReset
    // so their existing real-swipe behavior stays untouched.
    private IEnumerator TweenCard(Vector2 fromPos, Vector2 toPos, float fromRot, float toRot, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && !hasInteracted)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            rt.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(fromRot, toRot, t));
            yield return null;
        }

        if (hasInteracted)
        {
            dragHintContainer.gameObject.SetActive(false);
            dragHintText.gameObject.SetActive(false);
            yield break;
        }

        rt.anchoredPosition = toPos;
        rt.localEulerAngles = new Vector3(0, 0, toRot);
    }
}