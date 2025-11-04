using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    [Header("Progress")]
    public int completedNight = 0; // Сколько ночей пройдено

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProgress(); // Загружаем прогресс при запуске
    }

    public void CompleteNight(int nightIndex)
    {
        if (completedNight < nightIndex)
        {
            completedNight = nightIndex;
            SaveProgress();
        }
    }

    public void UnlockNextNight()
    {
        completedNight++;
        SaveProgress();
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("CompletedNight", completedNight);
        PlayerPrefs.Save();
        Debug.Log($"💾 Прогресс сохранён — пройдено ночей: {completedNight}");
    }

    public void LoadProgress()
    {
        completedNight = PlayerPrefs.GetInt("CompletedNight", 0);
        Debug.Log($"📂 Прогресс загружен — пройдено ночей: {completedNight}");
    }
}
