using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class GachaRoller : MonoBehaviour
{
    [Header("Гача настройки")]
    public GachaItem[] items;
    public int rollPrice = 50;
    public float rollTime = 1f;
    public float changeInterval = 0.1f;

    [Header("UI")]
    public Image display;
    public TMP_Text coinsText;

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip rollTickSound;
    public AudioClip finalDropSound;
    public AudioClip noMoneySound;

    [Header("Система игрока")]
    public PlayerData player;

    [Header("Назад в меню")]
    public string menuSceneName = "MainMenu";

    private bool isRolling = false;   // ← Защита от повторных кликов

    void Start()
    {
        player.Load();
        UpdateUI();
    }

    public void Back()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void Roll()
    {
        // защита от повторного запуска
        if (isRolling)
            return;

        if (player.coins < rollPrice)
        {
            if (noMoneySound != null)
                audioSource.PlayOneShot(noMoneySound);
            return;
        }

        player.coins -= rollPrice;
        UpdateUI();

        StartCoroutine(RollAnimation());
    }

    IEnumerator RollAnimation()
    {
        isRolling = true;  // блокируем кнопку

        float timer = 0f;

        while (timer < rollTime)
        {
            display.sprite = items[Random.Range(0, items.Length)].icon;

            if (rollTickSound != null)
                audioSource.PlayOneShot(rollTickSound);

            timer += changeInterval;
            yield return new WaitForSeconds(changeInterval);
        }

        // финальный предмет
        GachaItem final = GetItemByChance();
        display.sprite = final.icon;

        if (finalDropSound != null)
            audioSource.PlayOneShot(finalDropSound);

        player.AddItem(final.id);
        UpdateUI();

        isRolling = false;  // гача завершена, можно снова кликать
    }

    GachaItem GetItemByChance()
    {
        float total = items.Sum(i => i.dropChance);
        float randomPoint = Random.Range(0, total);
        float current = 0;

        foreach (var item in items)
        {
            current += item.dropChance;
            if (randomPoint <= current)
                return item;
        }
        return items[items.Length - 1];
    }

    void UpdateUI()
    {
        coinsText.text = player.coins.ToString();
    }
}
