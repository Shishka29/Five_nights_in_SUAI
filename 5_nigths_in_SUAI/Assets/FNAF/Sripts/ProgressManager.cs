using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    public int completedNight = 0;   // текущая пройденная ночь
    public int coins = 0;            // монетки игрока

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadProgress()
    {
        completedNight = PlayerPrefs.GetInt("completedNight", 0);
        coins = PlayerPrefs.GetInt("coins", 0);
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("completedNight", completedNight);
        PlayerPrefs.SetInt("coins", coins);
        PlayerPrefs.Save();
    }

    public void ResetNights()
    {
        completedNight = 0;
        SaveProgress(); // сохраняем сразу сброс
    }

    // Сохраняем монетки отдельно
    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("coins", coins);
        PlayerPrefs.Save();
    }
}
