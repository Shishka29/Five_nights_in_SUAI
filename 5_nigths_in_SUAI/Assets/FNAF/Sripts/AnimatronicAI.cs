using System.Collections.Generic;
using UnityEngine;

public class AnimatronicAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Модель аниматроника, которую нужно двигать (может быть child).")]
    public Transform animatronicModel;

    [Header("AI Settings")]
    public Room currentRoom; // Текущая комната
    public Room targetRoom;  // Целевая комната (например, Office)
    public float moveInterval = 5f; // Интервал между перемещениями
    [Range(1, 20)] public int difficulty = 10; // Активность (1–20)

    [Header("Movement Chances")]
    [Range(0f, 1f)] public float forwardChance = 0.7f;
    [Range(0f, 1f)] public float sideChance = 0.2f;
    [Range(0f, 1f)] public float backwardChance = 0.1f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Room lastRoom;
    private float timer;

    void Start()
    {
        if (currentRoom != null)
        {
            MoveModelToRoom(currentRoom);
        }
        else
        {
            Debug.LogWarning($"{name}: currentRoom не задан!");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Делаем интервал движения зависимым от уровня сложности
        float adjustedInterval = Mathf.Lerp(8f, 2f, difficulty / 20f);
        // При difficulty=1 → 8 сек, при difficulty=20 → 2 сек

        if (timer >= adjustedInterval)
        {
            timer = 0f;

            // Шанс, что аниматроник вообще сходит
            float chance = Random.Range(0f, 20f);
            if (chance > difficulty)
            {
                if (showDebugLogs)
                    Debug.Log($"{name}: пропускает ход (chance={chance})");
                return;
            }

            // Адаптация поведения (чем выше сложность — тем больше forwardChance)
            forwardChance = Mathf.Lerp(0.4f, 0.8f, difficulty / 20f);
            sideChance = Mathf.Lerp(0.4f, 0.15f, difficulty / 20f);
            backwardChance = Mathf.Lerp(0.2f, 0.05f, difficulty / 20f);

            MoveToNextRoom();
        }
    }


    void MoveToNextRoom()
    {
        if (currentRoom == null || currentRoom.connectedRooms.Length == 0)
            return;

        Room nextRoom = ChooseNextRoom();
        if (nextRoom == null)
            return;

        lastRoom = currentRoom;
        currentRoom = nextRoom;

        MoveModelToRoom(currentRoom);

        if (showDebugLogs)
            Debug.Log($"{name} перешёл из {lastRoom.roomName} → {currentRoom.roomName}");

        if (currentRoom == targetRoom)
        {
            Debug.Log($" {name} достиг офиса!");
            GameManager.Instance?.TriggerGameOver(name);
        }

    }

    Room ChooseNextRoom()
    {
        List<Room> forwardRooms = new List<Room>();
        List<Room> sideRooms = new List<Room>();
        List<Room> backwardRooms = new List<Room>();

        foreach (Room r in currentRoom.connectedRooms)
        {
            if (r == lastRoom)
                backwardRooms.Add(r);
            else if (r.stageLevel > currentRoom.stageLevel)
                forwardRooms.Add(r);
            else if (r.stageLevel == currentRoom.stageLevel)
                sideRooms.Add(r);
            else
                backwardRooms.Add(r);
        }

        float roll = Random.value;
        Room chosen = null;

        if (roll < forwardChance && forwardRooms.Count > 0)
            chosen = forwardRooms[Random.Range(0, forwardRooms.Count)];
        else if (roll < forwardChance + sideChance && sideRooms.Count > 0)
            chosen = sideRooms[Random.Range(0, sideRooms.Count)];
        else if (backwardRooms.Count > 0)
            chosen = backwardRooms[Random.Range(0, backwardRooms.Count)];

        if (chosen == null && currentRoom.connectedRooms.Length > 0)
            chosen = currentRoom.connectedRooms[Random.Range(0, currentRoom.connectedRooms.Length)];

        return chosen;
    }

    void MoveModelToRoom(Room room)
    {
        if (animatronicModel != null)
        {
            animatronicModel.position = room.mapPosition;
        }
        else
        {
            transform.position = room.mapPosition;
        }
    }
}
