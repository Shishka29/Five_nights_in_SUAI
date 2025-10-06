using UnityEngine;
using UnityEngine.SceneManagement; // нужно, если хочешь перезапускать сцену

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Over UI")]
    public GameObject gameOverScreen; // сюда можешь прикрепить Canvas с экраном проигрыша

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void TriggerGameOver(string killerName)
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($" Игра окончена! Аниматроник {killerName} достиг офиса!");

        // Останавливаем время
        Time.timeScale = 0f;

        // Включаем экран проигрыша, если он есть
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);
        else
            Debug.LogWarning("GameOverScreen не назначен!");

        // Можно добавить перезапуск сцены через пару секунд:
        // StartCoroutine(RestartAfterDelay(5f));
    }

    // Пример: автоматический перезапуск
    private System.Collections.IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
