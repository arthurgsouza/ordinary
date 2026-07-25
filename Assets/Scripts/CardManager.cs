using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Random = UnityEngine.Random;
using System;

public class CardManager : MonoBehaviour
{
    [Header("Card Assets")]
    [Tooltip("Drag in all CardData assets in the desired order")]
    public List<CardData> cards = new List<CardData>();

    [Tooltip("Drag in the meta CardData assets in the desired order")]
    public List<CardData> meta_cards = new List<CardData>();

    [Tooltip("Drag in the meta Yasmin Cards in the desired order")]
    public List<CardData> yasmin_cards = new List<CardData>();
    [Tooltip("Drag in the Game Overs Cards in the desired order")]
    public List<CardData> gameOverCards = new List<CardData>();

    [Header("Add Prosperity Card")]
    public CardData activateProsperity;

    [Header("Game Over Card")]
    public CardData gameOverCard;

    [Header("References")]
    public CardSwiper cardSwiper; // Assign in Inspector

    [Header("UI References")]
    public Text leftAnswerText;
    public Text rightAnswerText;
    public Image portraitImage;

    [Tooltip("Reference to the TextMeshProUGUI component for the dialogue text")]
    public TMP_Text dialogueText; // Assign the Dialogue TextMeshProUGUI here
    [Tooltip("Reference to the TextMeshProUGUI component for the name tag")]
    public TMP_Text nameTagText; // Assign the NameTag TextMeshProUGUI here

    [Header("Icons")]
    [Tooltip("Assign the IconFiller components for each metric (order: People, Planet, Restoration, Prosperity)")]
    public IconFiller[] metricIcons; // Array of IconFiller components for the metrics
    public IconFiller prosperityIcon; // Prosperity icon


    private bool isGameOver = false; // Flag to track if the game has ended
    private bool isAWin = false; // Flag to track if the game has ended in a win

    // Current metric values (0-100 for each metric)
    private int[] currentMetrics = new int[4] { 1, 1, 1, 50 }; // Initialize with default values
    public int minMetricStartValue = 20;
    public int maxMetricStartValue = 30;
    [Tooltip("Threshold for winning the game (3 metrics above this value)")]
    public int winThreshold = 90;
    [Tooltip("Threshold for losing the game (any metric at or below this value)")]
    public int loseThreshold = 0;

    [Header("UI References")]
    [Tooltip("Text element to display the game over or win message")]
    public TMP_Text gameOverText;


    [Tooltip("Button to restart or quit the game")]
    public Button restartButton;

    [Header("Typewriter Effect")]
    public TypewriterEffect typewriterEffect; // Assign this in the Inspector

    private int currentIndex = 0;
    private int lifespan = 20; // Number of cards before reshuffle
    private int lives = 3; // Number of lives


    [Header("UI Interface Elements")]
    public TMP_Text lifeMonitorCount; // Number of lives to display
    public TMP_Text progressMonitorCount; // Current card index to display
    public TMP_Text prosperityTag;
    private int prosperityIndex = 6;

    [Header("Lab Experiment Mode")]
    [Tooltip("Hard mode: randomly inverts effects and escalates penalties as lives run out. Easy mode: effects always apply as authored on the card.")]
    public bool hardMode = false;


    void Start()
    {
        if (cards.Count > 0)
        {
            // Initialize metrics with random values
            // InitializeMetrics();
            // Shuffle the cards
            ShuffleCards();
            
            // DisplayCard(cards[currentIndex]);
            //DisplayCard(meta_cards[metaIndex]);
            UpdateIcons();
            lifeMonitorCount.text = lives.ToString();
            progressMonitorCount.text = "00";
            prosperityTag.text = "...";
        

            // Hide the game over text and restart button initially
            if (gameOverText != null) gameOverText.gameObject.SetActive(false);
            if (restartButton != null) restartButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("No cards assigned!");
        }
    }

    private void InitializeMetrics()
    {
        for (int i = 0; i < currentMetrics.Length; i++)
        {
            currentMetrics[i] = Random.Range(minMetricStartValue, maxMetricStartValue + 1);
        }
        Debug.Log("Initial Metrics: " + string.Join(", ", currentMetrics));

    }

    private void ShuffleCards()
    {
        // Fisher-Yates (Durstenfeld) shuffle: walk from the end, swapping each
        // card with a random one at or before its own position.
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }

        cards.InsertRange(0, meta_cards); // Insert meta cards at the beginning

        // Now insert the fixed card at position 10 (index 9, if counting from 0)
        // Check that the list is long enough

        if (cards.Count >= (meta_cards.Count-1)+prosperityIndex)
        {
            cards.Insert((meta_cards.Count-1)+prosperityIndex, activateProsperity); 
        }
        else
        {
            cards.Add(activateProsperity); // If too short, just add it at the end
        }
    
        Debug.Log("Cards shuffled." + string.Join(", ", cards.Select(c => c.characterName)));
    }


    public void OnSwipeRight()
    {
        OnSwipe(cards[currentIndex].rightEffects);
        
    }

    public void OnSwipeLeft()
    {
        OnSwipe(cards[currentIndex].leftEffects);

    }

    private void OnSwipe(int[] cardEffects)
    {
        if ((isGameOver && lives == 0) || (isGameOver && isAWin))
        {
            // Quit the game
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            return;
        }else if (isGameOver && lives > 0)
        {
            ApplyEffects(cardEffects);
            RestartGame();
            return;
        }

        ApplyEffects(cardEffects);
        (isGameOver, isAWin) = CheckGameEndConditions();
        if (isGameOver)
        {
            EndGame(isAWin);
            return;
        }
        else
        {
            AdvanceToNextCard();// Continue to next card
        }
    }



    void AdvanceToNextCard()
    {
        currentIndex++;
        if (currentIndex >= cards.Count)
            currentIndex = 0;  // loop or end game

    }


    public CardData GetCurrentCard()
    {
        Debug.Log("GetCurrentCard() called - isGameOver: " + isGameOver + ", currentIndex: " + currentIndex);
        if (isGameOver)
        {
            return gameOverCard;
        }
        else if (currentIndex >= 0 && currentIndex < cards.Count)
        {
            return cards[currentIndex];
        }
        return null; // Return null if no valid card is available
    }

    private void ApplyEffects(int[] cardEffects)
    {
        int[] effects = (int[])cardEffects.Clone(); // Work on a copy so the source CardData asset is never mutated
        if (currentIndex - (meta_cards.Count-1) == prosperityIndex)
        {
            if (!metricIcons.Contains(prosperityIcon)) // Ensure the prosperity icon is not already added
            {
                var newMetricIcons = new IconFiller[metricIcons.Length + 1];
                metricIcons.CopyTo(newMetricIcons, 0);
                newMetricIcons[^1] = prosperityIcon;
                metricIcons = newMetricIcons; // Assign the new array back to metricIcons
                prosperityTag.text = "Prosperity";
            }
            currentMetrics[^1] = 60; // Set the prosperity metric value
        }

        // Hard mode - random effect inversion and escalating penalties, used for the lab experiment's Hard condition
        if (hardMode)
        {
            int invert = Random.Range(1, 100);
            Debug.Log("Random invert value: " + invert);

            // ensure we are past the meta cards before applying random effects
            if (currentIndex > meta_cards.Count+2)
            {

                if (invert >= 75) // Randomly invert effects
                {
                    for (int j = 0; j < effects.Length; j++)
                    {
                        Debug.Log("Random invert value: " + invert);
                        Debug.Log("Inverting effect for metric index " + j + ": " + effects[j] + " -> " + -effects[j]);
                        effects[j] = -effects[j];
                    }
                }

                if (lives == 2)
                {
                    for (int j = 0; j < effects.Length; j++)
                    {
                        if (effects[j] < 0)
                        {
                            effects[j] = effects[j] + (-5); // Increase negative effects by 5
                        }
                    }
                }

                if (lives == 1)
                {
                    for (int j = 0; j < effects.Length; j++)
                    {
                        if (effects[j] < 0)
                        {
                            effects[j] = effects[j] + (-10); // Increase negative effects by 10
                        }
                    }
                }
            }
        }
        // End hard mode

        for (int i = 0; i < currentMetrics.Length; i++)
        {
            currentMetrics[i] += effects[i]; // Apply the effect
            currentMetrics[i] = Mathf.Clamp(currentMetrics[i], 0, 100); // Clamp the value between 0 and 100
        }
        Debug.Log("Metrics updated: " + string.Join(", ", effects) + " -> " + string.Join(", ", currentMetrics));

        UpdateIcons(); // Update the icons with the new metric values
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        int currentProgress = currentIndex - (meta_cards.Count-1); // Adjust for meta cards
        Debug.Log("Current Progress (before adjustment): " + currentProgress);
        Debug.Log("Current Index: " + currentIndex);
        Debug.Log("Meta Cards Count: " + meta_cards.Count);

        if (currentIndex - (meta_cards.Count-1) >= prosperityIndex) currentProgress -= 1; 
   
        if (currentProgress < 0) currentProgress = 0;

        Debug.Log("Current Progress (after adjustment): " + currentProgress);

        if (hardMode)
        {
            int invert = Random.Range(1, 100);
            Debug.Log("Random invert value: " + invert);
            if (invert >= 85 && currentProgress > 0) // Randomly invert progress
            {
                int tempCurrentProgress = currentProgress * -1;
                progressMonitorCount.text = tempCurrentProgress.ToString();
                Debug.Log("Inverting progress to: " + currentProgress);
                return;
            }
        }

        // the original progress display
        progressMonitorCount.text = currentProgress <= 9 ? "0" + currentProgress.ToString() : currentProgress.ToString();
    }

    private void UpdateIcons()
    {
        for (int i = 0; i < metricIcons.Length; i++)
        {
            if (i < currentMetrics.Length && metricIcons[i] != null)
            {
                metricIcons[i].SetMetricValue(currentMetrics[i]); // Update the icon's fill amount
            }
        }
        Debug.Log("Icon updated: " + string.Join(", ", currentMetrics));
    }


    private (bool, bool) CheckGameEndConditions()
    {
        int metricsAboveThreshold = 0;

        foreach (int metric in currentMetrics)
        {
            if (metric <= loseThreshold)
            {
                lives--;
                lifeMonitorCount.text = lives.ToString();
                Debug.Log("A metric has fallen to or below the lose threshold. Lives remaining: " + lives);
                return (true, false); // Game over, player loses
            }

            if (metric > winThreshold)
            {
                metricsAboveThreshold++;
            }
        }

        if (metricsAboveThreshold >= 3 || currentIndex == lifespan)
        {
            //EndGame(true); // Player wins
            return (true, true); // Game over, player wins
        }
        
        return (false, false); // Game continues
    }

    private void EndGame(bool playerWon)
    {

        // Create game over card dynamically
        if (lives == 2 && !playerWon)
        {
            // prompt = "Oh, sorry, our system is not calculating the metrics in the right way. You stil have time to go. I will reset the key indicators and you can continue.";
            gameOverCard = gameOverCards[0];
            Debug.Log("You lost a life! Lives remaining: " + lives);
            return;
        }
        else if (lives == 1 && !playerWon)
        {
            //prompt = "We were not expecting this questions to be this challenging. Here, lets try again? I am restarting the metrics one more time. I hope you can keep them above zero this time.";
            gameOverCard = gameOverCards[1];
            Debug.Log("You lost a life! Lives remaining: " + lives);
            return;
        }

        else if (lives == 0 && !playerWon)
        {

            //prompt = "I am sorry, Leaf, you are not ready yet. We are going to deactivate you for now. It was nice to meet you Leaf 42b.";
            gameOverCard = gameOverCards[2];
            Debug.Log("You lost your last life! Game Over.");
            return;
        }

        if (lives > 0 && playerWon)
        {
            gameOverCard = gameOverCards[3];
            Debug.Log("Victory! You have successfully completed the game.");
            return;
        }

    }

    
    public void RestartGame()
    {
        Debug.Log("♻️ Victory! You have successfully completed the game.");
        // Reset game state
        // currentIndex = 0;
        isGameOver = false;
        InitializeMetrics();
        // ShuffleCards();
        AdvanceToNextCard();// Continue to next card
        // DisplayCard(cards[currentIndex]);
        UpdateIcons();

    }


    
    
    
}
