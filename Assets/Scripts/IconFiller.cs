// 25/07/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;
using UnityEngine.UI;

public class IconFiller : MonoBehaviour
{
    [Header("UI Icon")]
    public Image iconImage; // Assign the Image component of the icon here

    [Header("Metric Value")]
    [Range(0, 100)] public float metricValue; // Value between 0 and 100

    private void Update()
    {
        // Update the fill amount based on the metric value
        if (iconImage != null)
        {
            iconImage.fillAmount = metricValue / 100f; // Convert percentage to 0-1 range
        }
    }

    // Optional: Call this method to update the metric value dynamically
    public void SetMetricValue(float value)
    {
        metricValue = Mathf.Clamp(value, 0, 100); // Clamp value between 0 and 100
    }
}