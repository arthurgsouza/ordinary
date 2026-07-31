using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IconFiller : MonoBehaviour
{
    [Header("UI Icon")]
    public Image iconImage; // Assign the Image component of the icon here

    [Header("Metric Value")]
    [Range(0, 100)] public float metricValue; // Value between 0 and 100

    [Header("Animation")]
    public float animationDuration = 0.5f; // Time in seconds for the animation

    private Coroutine currentFillCoroutine;

    private void Update()
    {
        // No longer directly updating fillAmount here
    }

    // Call this method to update the metric value with animation
    public void SetMetricValue(float value, float delay = 0f)
    {
        metricValue = Mathf.Clamp(value, 0, 100); // Clamp value between 0 and 100
        
        // Stop any ongoing animation
        if (currentFillCoroutine != null)
            StopCoroutine(currentFillCoroutine);
        
        // Start the new animation
        currentFillCoroutine = StartCoroutine(AnimateFill(metricValue / 100f, delay));
    }

    private IEnumerator AnimateFill(float targetAmount, float delay)
    {
        yield return new WaitForSeconds(delay);

        float startAmount = iconImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            iconImage.fillAmount = Mathf.Lerp(startAmount, targetAmount, elapsed / animationDuration);
            yield return null;
        }

        iconImage.fillAmount = targetAmount; // Ensure exact final value
    }
}