using UnityEngine;
using System.Collections.Generic;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public int coins = 0;
    public int reflectedAttacks = 0;   // ✅ ВСЕ АТАКИ ЗА НОЧЬ
    public List<string> ownedItems = new List<string>();

    public int energyLeft = 100;

    // Новое поле для временной награды
    public int pendingReward = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void AddPendingReward(int amount)
    {
        pendingReward += amount;
        Save();
    }

    public void CollectPendingReward()
    {
        AddCoins(pendingReward);
        pendingReward = 0;
        Save();
    }

    public void AddItem(string id)
    {
        ownedItems.Add(id);
        Save();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        Save();
    }

    // ✅ ВЫЗЫВАЕШЬ ПРИ КАЖДОЙ ОТРАЖЁННОЙ АТАКЕ
    public void AddReflectedAttack()
    {
        reflectedAttacks++;
    }

    // ✅ ОБНУЛЯТЬ В НАЧАЛЕ НОЧИ
    public void ResetNightStats()
    {
        reflectedAttacks = 0;
    }

    public void Save()
    {
        PlayerPrefs.SetString("items", string.Join(",", ownedItems));
        PlayerPrefs.SetInt("coins", coins);
        PlayerPrefs.SetInt("pendingReward", pendingReward);  // сохраняем временную награду
        PlayerPrefs.Save();
    }

    public void Load()
    {
        coins = PlayerPrefs.GetInt("coins", 0);
        pendingReward = PlayerPrefs.GetInt("pendingReward", 0); // загружаем временную награду

        string itemString = PlayerPrefs.GetString("items", "");
        if (!string.IsNullOrEmpty(itemString))
            ownedItems = new List<string>(itemString.Split(','));
        else
            ownedItems = new List<string>();
    }
}
