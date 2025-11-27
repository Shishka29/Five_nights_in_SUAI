using UnityEngine;
using System.Collections.Generic;

public class PlayerData : MonoBehaviour
{
    public int coins = 0;
    public List<string> ownedItems = new List<string>();

    public void AddItem(string id)
    {
        ownedItems.Add(id);
        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetString("items", string.Join(",", ownedItems));
        PlayerPrefs.SetInt("coins", coins);
    }

    public void Load()
    {
        coins = PlayerPrefs.GetInt("coins", 0);

        string itemString = PlayerPrefs.GetString("items", "");
        if (!string.IsNullOrEmpty(itemString))
            ownedItems = new List<string>(itemString.Split(','));
    }
}
