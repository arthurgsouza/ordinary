using UnityEngine;

// 25/07/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

public class MetricManager : MonoBehaviour
{
    public IconFiller[] iconFillers; // Assign all IconFiller scripts here
    public float[] metrics; // Array of metric values (0-100)

    private void Update()
    {
        for (int i = 0; i < iconFillers.Length; i++)
        {
            if (i < metrics.Length)
            {
                iconFillers[i].SetMetricValue(metrics[i]);
            }
        }
    }
}