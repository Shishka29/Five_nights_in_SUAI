using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Menu UI")]
    public Canvas mainMenuCanvas;       // 🎨 основной канвас меню
    public Image menuImage;
    public Sprite[] menuSprites;

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button gachiButton;
    public Button customNightButton;

    [Header("Custom Night")]
    public Canvas customNightCanvas;    // 🎨 канвас кастом найта

    [Header("Intro")]
    public GameObject introPanel;
    public Image introImage;
    public Sprite newGameIntroSprite;
    public float introDuration = 2f;

    [Header("Fade Panel")]
    public GameObject fadePanel;
    public float fadeDuration = 1f;

    [Header("Music")]
    public AudioSource menuMusic;
    public float musicFadeDuration = 2f;

    private void Start()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.LoadProgress();

        // Убедимся, что канвасы в нужном состоянии
        if (mainMenuCanvas != null) mainMenuCanvas.enabled = true;
        if (customNightCanvas != null) customNightCanvas.enabled = false;

        StartCoroutine(InitMenuNextFrame());
    }

    private IEnumerator InitMenuNextFrame()
    {
        yield return null;
        SetupMenu();
    }

    private void SetupMenu()
    {
        if (menuImage == null)
        {
            Debug.LogError("❌ menuImage не назначен!");
            return;
        }

        if (menuSprites == null || menuSprites.Length == 0)
        {
            Debug.LogWarning("⚠️ menuSprites не назначены!");
            return;
        }

        int completedNight = ProgressManager.Instance != null ? ProgressManager.Instance.completedNight : 0;
        int spriteIndex = Mathf.Clamp(completedNight, 0, menuSprites.Length - 1);

        menuImage.sprite = menuSprites[spriteIndex];
        menuImage.gameObject.SetActive(true);

        SafeSetActiveButton(newGameButton, true);
        SafeSetActiveButton(continueButton, completedNight >= 1);
        SafeSetActiveButton(gachiButton, completedNight >= 1);
        SafeSetActiveButton(customNightButton, completedNight >= 1);
    }

    private void SafeSetActiveButton(Button button, bool state)
    {
        if (button != null)
            button.gameObject.SetActive(state);
    }

    // --- Кнопки ---
    public void StartNewGame()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.completedNight = 1;
            ProgressManager.Instance.SaveProgress();
        }

        HideMenuUI();
        StartCoroutine(FadeAndLoad("Night1", showIntro: true));
    }

    public void ContinueNight()
    {
        if (ProgressManager.Instance == null) return;

        int nextNight = ProgressManager.Instance.completedNight + 1;

        if (nextNight <= 6)
        {
            HideMenuUI();
            StartCoroutine(FadeAndLoad($"Night{nextNight}", showIntro: false));
        }
    }

    public void OpenGachi()
    {
        HideMenuUI();
        StartCoroutine(FadeAndLoad("GachiScene", showIntro: false));
    }

    public void OpenCustomNight()
    {
        Debug.Log("⚙️ Открыт режим Custom Night");

        // Отключаем основной канвас и включаем кастом найт
        if (mainMenuCanvas != null)
            mainMenuCanvas.enabled = false;

        if (customNightCanvas != null)
            customNightCanvas.enabled = true;

        // Можно добавить лёгкий fade-переход
        if (fadePanel != null)
        {
            StartCoroutine(SimpleFade(fadePanel, 0.25f));
        }
    }

    public void ReturnFromCustomNight()
    {
        Debug.Log("↩️ Возврат в основное меню");

        if (customNightCanvas != null)
            customNightCanvas.enabled = false;

        if (mainMenuCanvas != null)
            mainMenuCanvas.enabled = true;
    }

    // --- UI Скрытие / Переход ---
    private void HideMenuUI()
    {
        SafeSetActiveButton(newGameButton, false);
        SafeSetActiveButton(continueButton, false);
        SafeSetActiveButton(gachiButton, false);
        SafeSetActiveButton(customNightButton, false);

        if (menuImage != null)
            menuImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeAndLoad(string sceneName, bool showIntro)
    {
        if (menuMusic != null)
            StartCoroutine(FadeOutMusic());

        if (fadePanel != null)
        {
            fadePanel.SetActive(true);
            CanvasGroup cg = fadePanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / fadeDuration;
                    cg.alpha = Mathf.Clamp01(t);
                    yield return null;
                }
            }
        }

        if (showIntro && introPanel != null && introImage != null && newGameIntroSprite != null)
        {
            introImage.sprite = newGameIntroSprite;
            introPanel.SetActive(true);
            yield return new WaitForSeconds(introDuration);
            introPanel.SetActive(false);
        }

        if (!SceneExists(sceneName))
        {
            Debug.LogError($"❌ Сцена '{sceneName}' не найдена в Build Settings!");
            yield break;
        }

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = true;
        while (!asyncOp.isDone)
            yield return null;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = menuMusic.volume;

        float t = 0f;
        while (t < musicFadeDuration)
        {
            t += Time.deltaTime;
            menuMusic.volume = Mathf.Lerp(startVolume, 0f, t / musicFadeDuration);
            yield return null;
        }

        menuMusic.Stop();
        menuMusic.volume = startVolume;
    }

    private IEnumerator SimpleFade(GameObject panel, float duration)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        panel.SetActive(true);
        cg.alpha = 0f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        panel.SetActive(false);
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }
}
