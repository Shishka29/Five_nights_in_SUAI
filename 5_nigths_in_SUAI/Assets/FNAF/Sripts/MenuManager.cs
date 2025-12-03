using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Menu UI")]
    public Canvas mainMenuCanvas;
    public Image menuImage;

    [Header("Menu Sprites")]
    public Sprite[] menuProgressSprites;   // спрайты меню по пройденным ночам

    [Header("Night Intro Sprites")]
    public Sprite[] nightIntroSprites;     // спрайты перед каждой ночью

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button gachiButton;
    public Button customNightButton;

    [Header("Custom Night")]
    public Canvas customNightCanvas;

    [Header("Intro Panel")]
    public GameObject introPanel;
    public Image introImage;
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

        // Канвасы в нужном состоянии
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
        UpdateMenuSprite();

        int completedNight = ProgressManager.Instance != null ? ProgressManager.Instance.completedNight : 0;
        SafeSetActiveButton(newGameButton, true);
        SafeSetActiveButton(continueButton, completedNight >= 1);
        SafeSetActiveButton(gachiButton, completedNight >= 1);
        SafeSetActiveButton(customNightButton, completedNight >= 1);
    }

    private void UpdateMenuSprite()
    {
        if (menuImage == null || menuProgressSprites == null || menuProgressSprites.Length == 0) return;

        int completedNight = ProgressManager.Instance != null ? ProgressManager.Instance.completedNight : 0;
        int index = Mathf.Clamp(completedNight, 0, menuProgressSprites.Length - 1);
        menuImage.sprite = menuProgressSprites[index];
        menuImage.gameObject.SetActive(true);
    }

    private void SafeSetActiveButton(Button button, bool state)
    {
        if (button != null)
            button.gameObject.SetActive(state);
    }

    private void ShowNightIntroSprite(int nightNumber)
    {
        if (introPanel == null || introImage == null || nightIntroSprites == null || nightIntroSprites.Length == 0) return;

        int index = Mathf.Clamp(nightNumber - 1, 0, nightIntroSprites.Length - 1);
        introImage.sprite = nightIntroSprites[index];
        introPanel.SetActive(true);
    }

    // --- Кнопки ---
    public void StartNewGame()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.ResetNights(); // сброс всех ночей

        UpdateMenuSprite(); // обновляем меню на "0 ночей"
        HideMenuUI();

        // запускаем корутину интро → спрайт ночи → загрузка сцены
        StartCoroutine(StartNewGameSequence());
    }

    private IEnumerator StartNewGameSequence()
    {
        // 1️⃣ Показ интро
        if (introPanel != null)
        {
            introPanel.SetActive(true); // включаем интро
            yield return new WaitForSeconds(introDuration); // ждём несколько секунд
            introPanel.SetActive(false); // выключаем интро
        }

        // 2️⃣ Показ спрайта первой ночи
        ShowNightIntroSprite(1); // включаем спрайт первой ночи на Image
        yield return new WaitForSeconds(1.5f); // держим спрайт перед началом ночи

        // 3️⃣ Плавный переход и загрузка сцены
        StartCoroutine(FadeAndLoad("Night1"));
    }




    public void ContinueNight()
    {
        if (ProgressManager.Instance == null) return;

        int nextNight = ProgressManager.Instance.completedNight + 1;
        if (nextNight <= 5)
        {
            HideMenuUI();
            ShowNightIntroSprite(nextNight); // показываем спрайт текущей ночи
            StartCoroutine(FadeAndLoad($"Night{nextNight}"));
        }
    }

    public void OpenGachi()
    {
        HideMenuUI();
        StartCoroutine(FadeAndLoad("Prize"));
    }

    public void OpenCustomNight()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.enabled = false;
        if (customNightCanvas != null) customNightCanvas.enabled = true;

        if (fadePanel != null)
            StartCoroutine(SimpleFade(fadePanel, 0.25f));
    }

    public void ReturnFromCustomNight()
    {
        if (customNightCanvas != null) customNightCanvas.enabled = false;
        if (mainMenuCanvas != null) mainMenuCanvas.enabled = true;
    }

    // --- UI скрытие ---
    private void HideMenuUI()
    {
        SafeSetActiveButton(newGameButton, false);
        SafeSetActiveButton(continueButton, false);
        SafeSetActiveButton(gachiButton, false);
        SafeSetActiveButton(customNightButton, false);

        if (menuImage != null)
            menuImage.gameObject.SetActive(false);
    }

    // --- Переход на сцену с fade ---
    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (menuMusic != null)
            StartCoroutine(FadeOutMusic());

        if (fadePanel != null)
        {
            fadePanel.SetActive(true);
            Image img = fadePanel.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;

                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / fadeDuration;
                    c.a = Mathf.Clamp01(t);
                    img.color = c;
                    yield return null;
                }
            }
        }

        yield return new WaitForSeconds(0.2f); // короткая пауза перед загрузкой

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
        if (menuMusic == null) yield break;

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
        Image img = panel.GetComponent<Image>();
        if (img == null) yield break;

        panel.SetActive(true);
        Color c = img.color;
        c.a = 0f;
        img.color = c;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            c.a = Mathf.Lerp(0f, 1f, t);
            img.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            c.a = Mathf.Lerp(1f, 0f, t);
            img.color = c;
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
