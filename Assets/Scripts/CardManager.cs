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
    private List<CardData> default_cards; 

    [Tooltip("Drag in the meta CardData assets in the desired order")]
    public List<CardData> meta_cards = new List<CardData>();

    [Tooltip("Drag in the meta Yasmin Cards in the desired order")]
    public List<CardData> yasmin_cards = new List<CardData>();
    [Tooltip("Drag in the Game Overs Cards in the desired order")]
    public List<CardData> gameOverCards = new List<CardData>();

    [Tooltip("Drag in the 3-arc Cards in the desired order")]
    public List<CardData> threeArcCards = new List<CardData>();
    private int numberOfThreeArcCards = 1; // Number of 3-arc cards to include in the game

    [Tooltip("Drag in the 2-arc Cards in the desired order")]
    public List<CardData> twoArcCards = new List<CardData>();
    private int numberOfTwoArcCards = 2; // Number of 2-arc cards to include in the game

    [Tooltip("Drag in the Single Cards in the desired order")]
    public List<CardData> singleCards = new List<CardData>();
    private int numberOfSingleCards = 6; // Number of single cards to include in the game


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
    private int[] currentMetrics = new int[4] { 1, 1, 1, 0 }; // Initialize with default values
    public int minMetricStartValue = 30;
    public int maxMetricStartValue = 50;
    [Tooltip("Threshold for winning the game (3 metrics above this value)")]
    public int winThreshold = 101;
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
    private int goal = 9; // Number of cards to reach for the game to be considered complete
    private int lives = 3; // Number of lives


    [Header("UI Interface Elements")]
    public TMP_Text lifeMonitorCount; // Number of lives to display
    public TMP_Text progressMonitorCount; // Current card index to display
    private int currentProgress = 0; // Current progress to display
    public TMP_Text prosperityTag;
    private int prosperityIndex = 5; // The index at which the prosperity card is inserted (1-based)

    [Header("Lab Experiment Mode")]
    [Tooltip("Hard mode: randomly inverts effects and escalates penalties as lives run out. Easy mode: effects always apply as authored on the card.")]
    public bool hardMode = false;


    void Start()
    {
        default_cards = new List<CardData>(cards);

        if (cards.Count > 0)
        {
            // Initialize metrics with random values
            ShuffleCards();
            
            // DisplayCard(cards[currentIndex]);
            //DisplayCard(meta_cards[metaIndex]);
            //UpdateIcons();
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


    private T[] FisherYatesShuffle<T>(T[] items)
    {
        for (int i = items.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
        return items;
    }

    private void ShuffleCards()
    {
        cards = new List<CardData>(FisherYatesShuffle(singleCards.ToArray())).ToList().Take(numberOfSingleCards).ToList(); // Reset to default cards if the list is longer than default
        
        InsertTwoArcCards(); // Insert 2-arc cards at random positions in the shuffled list
        InsertThreeArcCards();  // Insert 3-arc cards at random positions in the shuffled list
        // InsertProsperityCard(); // Insert the prosperity card at the specified index

        cards.InsertRange(0, meta_cards); // Insert meta cards at the beginning

    }

    private void InsertTwoArcCards()
    {
        // Insert 2-arc cards at random positions in the shuffled list
        int start = 0; // starting integer
        int end = twoArcCards.Count/2;  // ending integer
        List<int> twoArcIndices = FisherYatesShuffle(Enumerable.Range(start, end - start).ToArray()).ToList();
        Debug.Log("Shuffled twoArcIndices: " + string.Join(", ", twoArcIndices));

        for (int i = 0; i < numberOfTwoArcCards && twoArcCards.Count > 0; i ++)
        {
            int randomIndex = Random.Range(0, cards.Count); // +1 to allow insertion at the end
            int randomIndex2 = Random.Range(randomIndex + 1, cards.Count); // Ensure the second card is inserted right after the first
            Debug.Log("Inserting 2-arc cards at random index: " + randomIndex + "Random index 2: " + randomIndex2 + " Total cards: " + cards.Count + " Two-arc cards count: " + twoArcCards.Count);
            
            int cardIndice = twoArcIndices[i]*2; // Use shuffled index for 2-arc cards

            cards.Insert(randomIndex, twoArcCards[cardIndice]);
            cards.Insert(randomIndex2, twoArcCards[cardIndice+1]); // Insert the same card again for the second arc
        }
    }

    private void InsertThreeArcCards()
    {
        // Insert 3-arc cards at random positions in the shuffled list
        int start = 0; // starting integer
        int end = threeArcCards.Count/3;  // ending integer
        List<int> threeArcIndices = FisherYatesShuffle(Enumerable.Range(start, end - start).ToArray()).ToList();
        Debug.Log("Shuffled threeArcIndices: " + string.Join(", ", threeArcIndices));

        for (int i = 0; i < numberOfThreeArcCards && threeArcCards.Count > 0; i ++)
        {
            int randomIndex = Random.Range(0, cards.Count); // +1 to allow insertion at the end
            int randomIndex2 = Random.Range(randomIndex + 1, cards.Count); // Ensure the second card is inserted right after the first
            int randomIndex3 = Random.Range(randomIndex2 + 1, cards.Count); // Ensure the third card is inserted right after the second
            Debug.Log("Inserting 3-arc cards at random index: " + randomIndex + "Random index 2: " + randomIndex2 + "Random index 3: " + randomIndex3 + " Total cards: " + cards.Count + " Three-arc cards count: " + threeArcCards.Count);
            
            int cardIndice = threeArcIndices[i]*3; // Use shuffled index for 3-arc cards

            cards.Insert(randomIndex, threeArcCards[cardIndice]);
            cards.Insert(randomIndex2, threeArcCards[cardIndice+1]); // Insert the same card again for the second arc
            cards.Insert(randomIndex3, threeArcCards[cardIndice+2]); // Insert the third card
        }
    }

    // private void InsertProsperityCard()
    // {
    //     if (cards.Count >= prosperityIndex-1)
    //     {
    //         cards.Insert(prosperityIndex, activateProsperity); 
    //     }
    //     else
    //     {
    //         cards.Add(activateProsperity); // If too short, just add it at the end
    //     }
    // }

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
            // #if UNITY_EDITOR
            //     UnityEditor.EditorApplication.isPlaying = false;
            // #else
            //     Application.Quit();
            // #endif
            RestartGame(); // Restart the game instead of quitting
            return;
        }else if (isGameOver && lives > 0)
        {
            ApplyEffects(cardEffects);
            ResumeGame();
            return;
        }

        ApplyEffects(cardEffects);

        if (currentIndex >= (prosperityIndex + meta_cards.Count) && !metricIcons.Contains(prosperityIcon)) // Adjust for meta cards
        {
            cards.Insert(currentIndex+1, activateProsperity); // Insert the prosperity card at the adjusted index
            AddProsperityIcon(); // Add the prosperity icon to the metricIcons array
        }


        (isGameOver, isAWin) = CheckGameEndConditions();

        if (isGameOver)
        {
            EndGame(isAWin);
            return;
        }else
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
        Debug.Log(" 🎃 GetCurrentCard() called - isGameOver: " + isGameOver + ", currentIndex: " + currentIndex);
        if (isGameOver)
        {
            return gameOverCard;

        }else if (currentIndex >= 0 && currentIndex < cards.Count)
        {
            return cards[currentIndex];
        }
        return null; // Return null if no valid card is available
    }

    private void AddProsperityIcon()
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

    private void ApplyEffects(int[] cardEffects)
    {
        int[] effects = (int[])cardEffects.Clone(); // Work on a copy so the source CardData asset is never mutated

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

        //for (int i = 0; i < currentMetrics.Length; i++)
        for (int i = 0; i < metricIcons.Length; i++) // Only apply effects to the metrics that have corresponding icons
        {
            currentMetrics[i] += effects[i]; // Apply the effect
            currentMetrics[i] = Mathf.Clamp(currentMetrics[i], 0, 100); // Clamp the value between 0 and 100
        }
        Debug.Log("Card Name: " + cards[currentIndex].characterName + ", Effects applied: " + string.Join(", ", effects) + ", New Metrics: " + string.Join(", ", currentMetrics));
        Debug.Log("Metrics updated: " + string.Join(", ", effects) + " -> " + string.Join(", ", currentMetrics));

        UpdateIcons(); // Update the icons with the new metric values
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (cards[currentIndex].impactOnProgress) currentProgress++;
        Debug.Log(" 🤡🤡🤡 Progress updated: " + currentProgress + "/" + goal + " (Card: " + cards[currentIndex].characterName + ", impactOnProgress: " + cards[currentIndex].impactOnProgress + ")");
        //int currentProgress = CalculateCurrentProgress(); 

        if (hardMode)
        {
            int invert = Random.Range(1, 100);
            Debug.Log("Random invert value: " + invert);
            if (invert >= 75 && currentProgress > 0) // Randomly invert progress
            {
                int tempCurrentProgress = currentProgress * -1;
                progressMonitorCount.text = tempCurrentProgress.ToString();
                Debug.Log("Inverting progress to: " + currentProgress);
                return;
            }
        }

        // the original progress display
        if (currentProgress >= goal)
        {
            currentProgress = Mathf.Clamp(currentProgress, 0, goal); // Ensure currentProgress does not exceed goal
        }
        progressMonitorCount.text = currentProgress <= 9 ? "0" + currentProgress.ToString() : currentProgress.ToString();
    }

    private void UpdateIcons()
    {
        float baseDelay = 0.1f;
        for (int i = 0; i < metricIcons.Length; i++)
        {
            if (i < currentMetrics.Length && metricIcons[i] != null)
            {
                metricIcons[i].SetMetricValue(currentMetrics[i], baseDelay * i); // Update the icon's fill amount
            }
        }
        Debug.Log("Icon updated: " + string.Join(", ", currentMetrics));
    }

    private (bool, bool) CheckGameEndConditions()
    {
        int metricsAboveThreshold = 0;
        int metricsLoseThreshold = 0;

        int[] checkGameMetrics = (int[])currentMetrics.Clone();

        if (metricIcons.Length < 4) // Check if the prosperity icon is not present
        {
            checkGameMetrics = checkGameMetrics[..^1]; // Exclude the prosperity metric for game end checks
        }
        
        foreach (int metric in checkGameMetrics)
        {
            Debug.Log("Metric : " + metric + ", Lives : " + lives);
            if (metric <= loseThreshold)
            {
                metricsLoseThreshold++;
            }

            if (metric > winThreshold)
            {
                metricsAboveThreshold++;
            }
        }

        if (metricsLoseThreshold > 0)
        {
            currentProgress--;
            lives--;
            lifeMonitorCount.text = lives.ToString();
            Debug.Log("A metric has fallen to or below the lose threshold. Lives remaining: " + lives);
            return (true, false); // Game over, player loses
        }

        if (metricsAboveThreshold >= 3 || currentProgress >= goal)
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
            // Debug.Log("You lost a life! Lives remaining: " + lives);
            return;
        }
        else if (lives == 1 && !playerWon)
        {
            //prompt = "We were not expecting this questions to be this challenging. Here, lets try again? I am restarting the metrics one more time. I hope you can keep them above zero this time.";
            gameOverCard = gameOverCards[1];
            // Debug.Log("You lost a life! Lives remaining: " + lives);
            return;
        }

        else if (lives == 0 && !playerWon)
        {

            //prompt = "I am sorry, Leaf, you are not ready yet. We are going to deactivate you for now. It was nice to meet you Leaf 42b.";
            gameOverCard = gameOverCards[2];
            currentMetrics = new int[4] { 0, 0, 0, 0 }; // Reset metrics to zero
            UpdateIcons(); // Update the icons to reflect the reset metrics
            // Debug.Log("You lost your last life! Game Over.");
            return;
        }

        if (lives > 0 && playerWon)
        {
            gameOverCard = gameOverCards[3];
            currentMetrics = new int[4] { 101, 101, 101, 101 }; // Reset metrics to zero
            UpdateIcons(); // Update the icons to reflect the reset metrics
            Debug.Log("Victory! You have successfully completed the game.");
            return;
        }

    }

    public void QuitGame()
    {
        // Quit the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void ResumeGame()
    {
        // Reset game state
        isGameOver = false;
        InitializeMetrics();
        AdvanceToNextCard();// Continue to next card
        UpdateIcons();

    }

    public void RestartGame()
    {
        // Reset game state
        currentProgress = 0;
        isGameOver = false;
        isAWin = false;
        lives = 3;
        lifeMonitorCount.text = lives.ToString();
        currentIndex = 0;
        currentMetrics = new int[4] { 1, 1, 1, 0}; // Initialize with default values

        UpdateIcons();

        if (metricIcons.Length > 3)
        {
            Debug.Log("Removing prosperity icon from metricIcons array.");
            // Remove the last icon (prosperity icon) if it exists
            IconFiller[] newArray = new IconFiller[metricIcons.Length - 1];
            Array.Copy(metricIcons, newArray, newArray.Length);
            metricIcons = newArray;
        }
        prosperityTag.text = "..."; // Reset prosperity tag text

        
        ShuffleCards();
        UpdateProgress();
        Debug.Log("Game restarted. Lives: " + lives + ", Current Index: " + currentIndex + ", Metrics: " + string.Join(", ", currentMetrics));
    }

}
