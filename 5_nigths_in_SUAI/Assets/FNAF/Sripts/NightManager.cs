using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DifficultySegment
{
    [Header("Время и сложность")]
    public float startTime;      // Когда начинается сегмент
    public float duration;       // Сколько длится
    [Range(1, 20)] public int difficulty; // Уровень сложности
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
    public float nightTimer = 0f;           // Сколько прошло времени
    public float nightLength = 120f;        // Сколько длится ночь
    public float delayBeforeMenu = 5f;      // Задержка перед главным меню
    public bool showDebugLogs = true;

    [Header("🤖 Аниматроники")]
    public List<AnimatronicSettings> animatronicSettings = new();

    [Header("🦊 Фокси")]
    public List<FoxySettings> foxySettings = new();

    private Dictionary<object, int> lastDifficulty = new();
    private bool nightEnded = false;

    // ======================
    // Инициализация
    // ======================
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

        // Останавливаем всех
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
    }

    // ======================
    // Главный цикл ночи
    // ======================
    private void Update()
    {
        if (nightEnded) return;

        nightTimer += Time.deltaTime;

        UpdateAnimatronicDifficulties();
        UpdateFoxyDifficulties();

        // Проверяем конец ночи
        if (nightTimer >= nightLength)
        {
            EndNight();
        }
    }

    // ======================
    // Управление аниматрониками
    // ======================
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

            // Изменение сложности
            if (!lastDifficulty.ContainsKey(s.animatronic) || lastDifficulty[s.animatronic] != newDifficulty)
            {
                s.animatronic.difficulty = newDifficulty;
                lastDifficulty[s.animatronic] = newDifficulty;

                if (showDebugLogs)
                    Debug.Log($"🎚 {s.animatronic.name} → новая сложность: {newDifficulty} (время {nightTimer:F1}с)");
            }

            // Активация движения
            if (activated && !s.animatronic.CanMove)
            {
                s.animatronic.CanMove = true;
                if (showDebugLogs)
                    Debug.Log($"✅ {s.animatronic.name} активирован (сложность {newDifficulty})");
            }
        }
    }

    // ======================
    // Управление Фокси
    // ======================
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

    // ======================
    // Конец ночи
    // ======================
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

    // ======================
    // Game Over
    // ======================
    public void TriggerGameOver(string killer)
    {
        Debug.Log($"💀 ИГРА ОКОНЧЕНА — {killer} добрался до офиса!");
        // Здесь можно добавить переход на сцену GameOver
    }
}
