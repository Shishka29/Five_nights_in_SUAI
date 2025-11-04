using UnityEngine;
using UnityEngine.UI;

public class NoiceEffect : MonoBehaviour
{
    public RawImage noiseImage;
    public float moveIntensity = 0.2f; // насколько сильно сдвигаетс€
    public float flickerSpeed = 15f;   // скорость мигани€

    private Vector2 randomOffset;

    void Update()
    {
        // √енерируем случайное движение
        randomOffset.x = Mathf.PerlinNoise(Time.time * 1.3f, 0f) * moveIntensity;
        randomOffset.y = Mathf.PerlinNoise(0f, Time.time * 1.7f) * moveIntensity;

        noiseImage.uvRect = new Rect(randomOffset, Vector2.one);

        // Ёффект мигани€
        float alpha = Mathf.Lerp(0.6f, 1f, Mathf.PerlinNoise(Time.time * flickerSpeed, 0));
        noiseImage.color = new Color(1f, 1f, 1f, alpha);
    }
}
