using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CustomNightMenu : MonoBehaviour
{
    [Header("AI Levels (0–20)")]
    public TMP_Text[] levelTexts; // текстовые поля TMP
    private int[] levels;

    [Header("Настройки")]
    public int step = 1; // шаг изменения уровня
    public int maxLevel = 20;

    [Header("Звуки 🎵")]
    public AudioSource audioSource;     // общий источник звука (на Canvas или объекте меню)
    public AudioClip backgroundLoop;    // фоновый звук меню кастом найта
    public AudioClip buttonClickSound;  // звук при нажатии кнопки

    private void Start()
    {
        // Инициализация уровней
        levels = new int[levelTexts.Length];
        UpdateAllTexts();

        // 🎧 Запуск фонового звука
        if (audioSource != null && backgroundLoop != null)
        {
            audioSource.clip = backgroundLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void IncreaseLevel(int index)
    {
        if (index < 0 || index >= levels.Length) return;

        // 🔊 Звук клика
        PlayButtonSound();

        levels[index] = Mathf.Clamp(levels[index] + step, 0, maxLevel);
        UpdateText(index);
    }

    public void DecreaseLevel(int index)
    {
        if (index < 0 || index >= levels.Length) return;

        // 🔊 Звук клика
        PlayButtonSound();

        levels[index] = Mathf.Clamp(levels[index] - step, 0, maxLevel);
        UpdateText(index);
    }

    void UpdateText(int index)
    {
        if (levelTexts[index] != null)
            levelTexts[index].text = levels[index].ToString();
    }

    void UpdateAllTexts()
    {
        for (int i = 0; i < levelTexts.Length; i++)
            UpdateText(i);
    }

    // --- 🔊 Звуковая функция ---
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }
}
