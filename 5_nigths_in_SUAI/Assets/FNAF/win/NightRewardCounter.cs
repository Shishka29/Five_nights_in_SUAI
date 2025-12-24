using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class NightRewardCounter : MonoBehaviour
{
    [Header("Manual Override (если не используешь внешние источники)")]
    public bool useManualBase = false;
    public int manualBase;

    [Header("TextMeshPro Displays")]
    public TMP_Text baseText;
    public TMP_Text energyText;
    public TMP_Text reflectText;
    public TMP_Text totalText;

    [Header("Animation Settings")]
    public float countSpeed = 1.5f;

    [Header("Main Menu Settings")]
    public string mainMenuScene = "MainMenu";
    public float delayBeforeMenu = 2f;

    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Video Settings")]
    public RawImage videoRawImage;
    public VideoPlayer videoPlayer; // контролирует видео

    private int baseValue;
    private int energyValue;
    private int reflectValue;

    void Start()
    {
        if (fadeImage != null)
            fadeImage.gameObject.SetActive(false);

        ReadValues();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd; // событие конца видео
            videoPlayer.Play();
        }

        StartCoroutine(AnimateAll());
    }

    void ReadValues()
    {
        var pd = PlayerData.Instance;
        if (pd == null)
        {
            Debug.LogError("PlayerData.Instance не найден!");
            baseValue = energyValue = reflectValue = 0;
            return;
        }

        baseValue = useManualBase ? manualBase : pd.pendingReward;
        energyValue = pd.energyLeft;
        reflectValue = pd.reflectedAttacks;

        pd.pendingReward = 0;
        pd.energyLeft = 0;
        pd.reflectedAttacks = 0;
        pd.Save();
    }

    private bool videoEnded = false;

    void OnVideoEnd(VideoPlayer vp)
    {
        videoEnded = true;

        // Выключаем RawImage после окончания видео
        if (videoRawImage != null)
            videoRawImage.gameObject.SetActive(false);
    }

    IEnumerator AnimateAll()
    {
        // Ждём, пока видео не закончится
        while (videoPlayer != null && !videoEnded)
            yield return null;

        int baseCurrent = 0;
        int energyCurrent = 0;
        int reflectCurrent = 0;
        int totalCurrent = 0;

        int totalTarget = baseValue + energyValue + reflectValue;

        while (
            baseCurrent < baseValue ||
            energyCurrent < energyValue ||
            reflectCurrent < reflectValue ||
            totalCurrent < totalTarget
        )
        {
            baseCurrent = Mathf.Min(baseCurrent + Mathf.CeilToInt(Time.deltaTime * baseValue * countSpeed), baseValue);
            energyCurrent = Mathf.Min(energyCurrent + Mathf.CeilToInt(Time.deltaTime * energyValue * countSpeed), energyValue);
            reflectCurrent = Mathf.Min(reflectCurrent + Mathf.CeilToInt(Time.deltaTime * reflectValue * countSpeed), reflectValue);

            totalCurrent = baseCurrent + energyCurrent + reflectCurrent;

            if (baseText) baseText.text = baseCurrent.ToString();
            if (energyText) energyText.text = energyCurrent.ToString();
            if (reflectText) reflectText.text = reflectCurrent.ToString();
            if (totalText) totalText.text = totalCurrent.ToString();

            yield return null;
        }

        // Точные значения в конце
        if (baseText) baseText.text = baseValue.ToString();
        if (energyText) energyText.text = energyValue.ToString();
        if (reflectText) reflectText.text = reflectValue.ToString();
        if (totalText) totalText.text = totalTarget.ToString();

        // Добавляем монеты игроку
        PlayerData.Instance.coins += totalTarget;
        PlayerData.Instance.Save();

        // Ждём небольшую паузу перед затемнением
        yield return new WaitForSeconds(delayBeforeMenu);

        // Плавное затемнение и переход
        yield return StartCoroutine(FadeAndLoad(mainMenuScene));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Clamp01(timer / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }
        }

        SceneManager.LoadScene(sceneName);
    }
}
