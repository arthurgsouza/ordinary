using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Ordinary Leaf/Card Data")]
public class CardData : ScriptableObject
{
    public string characterName;
    [TextArea] public string promptText;
    public string leftAnswer;
    public string rightAnswer;
    public Sprite portrait;

    [Header("Indicator Effects (order: People, Planet, Restoration, Prosperity)")]
    public int[] leftEffects  = new int[4];
    public int[] rightEffects = new int[4];

    [Header("Progress Impact")]
    public bool impactOnProgress = true;
}