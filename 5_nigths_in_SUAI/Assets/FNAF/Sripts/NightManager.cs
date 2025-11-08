using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // нужно для кнопки и спрайта

[System.Serializable]
public class DifficultySegment
{
    [Header("Время и сложность")]
    public float startTime;
    public float duration;
    [Range(1, 20)] public int difficulty;
}

[System.Serializable]
public class AnimatronicSettings
{
    [Header("Аниматроник и его сложность")]
    public AnimatronicAI animatronic;
    public List<DifficultySegment> segments = new();
}

[System.Serializable]
public class FoxySettings
{
    [Header("Фокси и его сложность")]
    public FoxyAI foxy;
    public List<DifficultySegment> segments = new();
}

public class NightManager : MonoBehaviour
{
    public static NightManager Instance { get; private set; }

    [Header("⚙️ Параметры ночи")]
    public float nightTimer = 0f;
    public float nightLength = 120f;
    public float delayBeforeMenu = 5f;
    public bool showDebugLogs = true;

    [Header("🤖 Аниматроники")]
    public List<AnimatronicSettings> animatronicSettings = new();

    [Header("🦊 Фокси")]
    public List<FoxySettings> foxySettings = new();

    [Header("📞 Телефонный звонок")]
    [Tooltip("Через сколько секунд после старта ночи включается звонок (например 5–7)")]
    public float phoneStartDelay = 6f;
    public AudioSource phoneAudio;       // звук звонка
    public Image phoneSprite;            // спрайт телефона
    public Button phoneButton;           // кнопка отключения

    private Dictionary<object, int> lastDifficulty = new();
    private bool nightEnded = false;
    private bool phoneActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        nightTimer = 0f;
        nightEnded = false;
        lastDifficulty.Clear();

        foreach (var s in animatronicSettings)
        {
            if (s.animatronic != null)
            {
                s.animatronic.CanMove = false;
                lastDifficulty[s.animatronic] = -1;
            }
        }

        foreach (var f in foxySettings)
        {
            if (f.foxy != null)
            {
                f.foxy.CanMove = false;
                lastDifficulty[f.foxy] = -1;
            }
        }

        if (showDebugLogs)
            Debug.Log("🌙 Ночь началась!");

        // Настройка телефона
        if (phoneAudio != null) phoneAudio.Stop();
        if (phoneSprite != null) phoneSprite.enabled = false;

        if (phoneButton != null)
        {
            phoneButton.onClick.RemoveAllListeners();
            phoneButton.onClick.AddListener(StopPhoneCall);
        }

        StartCoroutine(StartPhoneAfterDelay());
    }

    private IEnumerator StartPhoneAfterDelay()
    {
        yield return new WaitForSeconds(phoneStartDelay);
        StartPhoneCall();
    }

    private void StartPhoneCall()
    {
        if (phoneActive || phoneAudio == null || phoneSprite == null) return;

        phoneActive = true;
        phoneSprite.enabled = true;
        phoneSprite.color = Color.white; // делаем видимым

        phoneAudio.Play();

        if (showDebugLogs)
            Debug.Log("📞 Телефонный звонок начался!");
    }

    public void StopPhoneCall()
    {
        if (!phoneActive) return;

        phoneActive = false;

        if (phoneAudio != null)
            phoneAudio.Stop();

        if (phoneSprite != null)
        {
            // Прозрачный, но остаётся объект
            phoneSprite.color = new Color(1, 1, 1, 0);
            phoneSprite.enabled = false;
        }

        if (showDebugLogs)
            Debug.Log("📴 Телефон отключен игроком.");
    }

    private void Update()
    {
        if (nightEnded) return;

        nightTimer += Time.deltaTime;

        UpdateAnimatronicDifficulties();
        UpdateFoxyDifficulties();
        UpdatePhoneCall(); // проверка окончания звука

        if (nightTimer >= nightLength)
        {
            EndNight();
        }
    }


    private void UpdateAnimatronicDifficulties()
    {
        foreach (var s in animatronicSettings)
        {
            if (s.animatronic == null || s.segments.Count == 0) continue;

            int newDifficulty = s.animatronic.difficulty;
            bool activated = false;

            foreach (var seg in s.segments)
            {
                if (nightTimer >= seg.startTime && nightTimer < seg.startTime + seg.duration)
                {
                    newDifficulty = seg.difficulty;
                    activated = true;
                    break;
                }
            }

            if (!lastDifficulty.ContainsKey(s.animatronic) || lastDifficulty[s.animatronic] != newDifficulty)
            {
                s.animatronic.difficulty = newDifficulty;
                lastDifficulty[s.animatronic] = newDifficulty;

                if (showDebugLogs)
                    Debug.Log($"🎚 {s.animatronic.name} → новая сложность: {newDifficulty} (время {nightTimer:F1}с)");
            }

            if (activated && !s.animatronic.CanMove)
            {
                s.animatronic.CanMove = true;
                if (showDebugLogs)
                    Debug.Log($"✅ {s.animatronic.name} активирован (сложность {newDifficulty})");
            }
        }
    }

    private void UpdateFoxyDifficulties()
    {
        foreach (var f in foxySettings)
        {
            if (f.foxy == null || f.segments.Count == 0) continue;

            int newDifficulty = f.foxy.difficulty;
            bool activated = false;

            foreach (var seg in f.segments)
            {
                if (nightTimer >= seg.startTime && nightTimer < seg.startTime + seg.duration)
                {
                    newDifficulty = seg.difficulty;
                    activated = true;
                    break;
                }
            }

            if (!lastDifficulty.ContainsKey(f.foxy) || lastDifficulty[f.foxy] != newDifficulty)
            {
                f.foxy.difficulty = newDifficulty;
                lastDifficulty[f.foxy] = newDifficulty;

                if (showDebugLogs)
                    Debug.Log($"🦊 {f.foxy.name} → новая сложность: {newDifficulty} (время {nightTimer:F1}с)");
            }

            if (activated && !f.foxy.CanMove)
            {
                f.foxy.CanMove = true;
                if (showDebugLogs)
                    Debug.Log($"🏁 {f.foxy.name} активирован (сложность {newDifficulty})");
            }
        }
    }

    private void EndNight()
    {
        nightEnded = true;
        Debug.Log("🌅 Ночь завершена! 6:00 AM — Возврат в главное меню...");

        foreach (var s in animatronicSettings)
        {
            if (s.animatronic != null)
                s.animatronic.CanMove = false;
        }

        foreach (var f in foxySettings)
        {
            if (f.foxy != null)
                f.foxy.CanMove = false;
        }

        StartCoroutine(ReturnToMainMenu());
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(delayBeforeMenu);
        Debug.Log("🎬 Загрузка главного меню...");
        SceneManager.LoadScene("MainMenu");
    }

    private void UpdatePhoneCall()
    {
        if (phoneActive && phoneAudio != null && !phoneAudio.isPlaying)
        {
            // Звук закончился — выключаем всё
            StopPhoneCall();
            if (showDebugLogs)
                Debug.Log("📴 Телефонный звонок завершён автоматически.");
        }
    }


    public void TriggerGameOver(string killer)
    {
        Debug.Log($"💀 ИГРА ОКОНЧЕНА — {killer} добрался до офиса!");
    }
}
