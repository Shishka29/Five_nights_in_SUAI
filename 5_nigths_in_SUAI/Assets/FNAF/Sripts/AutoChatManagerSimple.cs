using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ChatCharacter
{
    public string name;
    [TextArea] public string systemPrompt; // для характеристики
    public Color textColor = Color.white;  // цвет текста
}

public class AutoChatManagerSimple : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI chatText; // одно текстовое поле

    [Header("Персонажи")]
    [SerializeField] private List<ChatCharacter> characters = new List<ChatCharacter>();

    [Header("Время показа (сек)")]
    [SerializeField] private float minDelay = 2f;
    [SerializeField] private float maxDelay = 6f;

    void Start()
    {
        // Запуск автономного цикла для показа сообщений
        StartCoroutine(DisplayRandomMessages());
    }

    private IEnumerator DisplayRandomMessages()
    {
        while (true)
        {
            // Случайный персонаж
            int charIndex = Random.Range(0, characters.Count);
            ChatCharacter character = characters[charIndex];

            // Генерация случайной короткой фразы
            string message = GenerateRandomPhrase(character);

            // Показ текста с цветом
            chatText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(character.textColor)}>{character.name}: {message}</color>";

            // Ждем случайное время, потом очистка
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            // Очищаем текст
            chatText.text = "";

            // Ждем ещё немного перед следующей фразой
            yield return new WaitForSeconds(0.5f);
        }
    }

    private string GenerateRandomPhrase(ChatCharacter character)
    {
        // Примеры фраз для персонажей (можно добавить больше)
        List<string> phrases = new List<string>();

        switch (character.name)
        {
            case "Килимник":
                phrases.Add("Смотри на меня внимательнее!");
                phrases.Add("Твоя ошибка была очевидной.");
                phrases.Add("Не надейся на чудо.");
                break;

            case "Соловьев":
                phrases.Add("Вы меня удивили, но слабо.");
                phrases.Add("Надо работать лучше.");
                phrases.Add("Это просто недопустимо.");
                break;

            case "Аксенов":
                phrases.Add("Быстрее реагируй, студент!");
                phrases.Add("Ха-ха, ты опять промахнулся!");
                phrases.Add("Учись современно, а не по старинке.");
                break;

            case "Чернышев":
                phrases.Add("Твоя шутка второсортная.");
                phrases.Add("HR оценил бы так же.");
                phrases.Add("Не удивляйся моему сарказму.");
                break;

            default:
                phrases.Add("...");
                break;
        }

        int index = Random.Range(0, phrases.Count);
        return phrases[index];
    }
}
