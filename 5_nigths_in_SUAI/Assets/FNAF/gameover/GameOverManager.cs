using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    public Image fadeImage;           // Полупрозрачный черный Image для затемнения
    public float fadeDuration = 1.5f; // Длительность затемнения

    [Header("Audio")]
    public AudioSource gameOverAudio; // Звук гейм овер

    [Header("Video")]
    public VideoPlayer gameOverVideo; // Видео "шум" эффекта

    [Header("Settings")]
    public string mainMenuScene = "MainMenu"; // Сцена главного меню
    public float gameOverDelay = 2f;          // Время до начала затемнения

    private void Start()
    {
        // Скрываем затемнение
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }

        // Проигрываем видео и звук
        if (gameOverVideo != null) gameOverVideo.Play();
        if (gameOverAudio != null) gameOverAudio.Play();

        // Запускаем корутину перехода
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // Ждем несколько секунд перед затемнением
        yield return new WaitForSeconds(gameOverDelay);

        // Плавное затемнение
        if (fadeImage != null)
        {
            float timer = 0f;
            Color color = fadeImage.color;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Clamp01(timer / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }
        }

        // Переход в главное меню
        SceneManager.LoadScene(mainMenuScene);
    }
}
