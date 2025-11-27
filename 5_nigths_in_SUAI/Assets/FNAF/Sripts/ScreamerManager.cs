using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreamerManager : MonoBehaviour
{
    public static ScreamerManager Instance;

    [Header("Office For Jumpscare")]
    public Room officeRoom;                      // Комната, где должен происходить скример

    [Header("Settings")]
    public GameObject screamerModel;             // Модель скримера (аниматроник в офисе)
    public Animator screamerAnimator;            // Аниматор скримера
    public AudioSource audioSource;              // Аудио проигрыватель
    public AudioClip screamerSound;              // Звук скримера

    [Header("Game Over")]
    public string gameOverScene = "GameOver";    // Имя сцены Game Over

    private bool isTriggered = false;

    void Awake()
    {
        Instance = this;
        screamerModel.SetActive(false);
    }

    public void TriggerScreamer(string animatronicName)
    {
        if (isTriggered) return;
        isTriggered = true;

        // 1. Закрыть планшет
        var tab = tabcontroller.Instance;
        if (tab != null && tab.IsTabletOpen)
            tab.Close();

        // 2. Включить модель скримера
        screamerModel.SetActive(true);

        // 3. Анимация
        screamerAnimator.Play("Jumpscare");

        // 4. Звук
        audioSource.PlayOneShot(screamerSound);

        // 5. Переход в сцену GameOver
        StartCoroutine(GameOverDelay());
    }

    System.Collections.IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(2.5f);   // длительность скримера
        SceneManager.LoadScene(gameOverScene);
    }
}
