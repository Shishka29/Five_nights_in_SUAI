using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimatronicAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Модель аниматроника (child-объект или сам объект).")]
    public Transform animatronicModel;

    [Tooltip("ScriptableObject с маршрутом движения.")]
    public AnimatronicPathData pathData;

    [Header("AI Settings")]
    public Room currentRoom;      // Текущая комната
    public Room targetRoom;       // Финальная комната (офис)
    [Range(1f, 20f)] public int difficulty = 10; // Активность

    [Header("Movement Chances")]
    [Range(0f, 1f)] public float forwardChance = 0.7f;
    [Range(0f, 1f)] public float sideChance = 0.2f;
    [Range(0f, 1f)] public float backwardChance = 0.1f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Room lastRoom;
    private float timer;
    private int stuckCounter = 0;
    private List<Room> allowedRooms = new();
    private static List<AnimatronicAI> allAnimatronics = new();

    void Awake()
    {
        if (!allAnimatronics.Contains(this))
            allAnimatronics.Add(this);
    }

    void OnDestroy()
    {
        allAnimatronics.Remove(this);
    }

    void Start()
    {
        if (pathData != null && pathData.pathSegments != null)
        {
            var roomSet = new HashSet<Room>();
            foreach (var seg in pathData.pathSegments)
            {
                if (seg.from != null) roomSet.Add(seg.from);
                if (seg.to != null) roomSet.Add(seg.to);
            }
            allowedRooms = roomSet.ToList();
        }
        else
        {
            Debug.LogWarning($"{name}: не назначен маршрут pathData!");
        }

        if (currentRoom == null && allowedRooms.Count > 0)
            currentRoom = allowedRooms[0];

        if (currentRoom != null)
            MoveModelToRoom(currentRoom);
        else
            Debug.LogWarning($"{name}: currentRoom не задан!");
    }

    void Update()
    {
        timer += Time.deltaTime;

        float adjustedInterval = Mathf.Lerp(8f, 2f, difficulty / 20f);
        if (timer < adjustedInterval) return;

        timer = 0f;

        // 🚫 Не двигаться, если камера смотрит на эту комнату
        if (IsRoomVisible(currentRoom))
        {
            if (showDebugLogs)
                Debug.Log($"{name}: камера смотрит на {currentRoom.roomName}, жду...");
            return;
        }

        // 🎲 Шанс пропустить ход
        float chance = Random.Range(0f, 20f);
        if (chance > difficulty)
        {
            if (showDebugLogs)
                Debug.Log($"{name}: пропускает ход (chance={chance})");
            return;
        }

        UpdateChancesByDifficulty();
        MoveToNextRoom();
    }

    void UpdateChancesByDifficulty()
    {
        forwardChance = Mathf.Lerp(0.4f, 0.8f, difficulty / 20f);
        sideChance = Mathf.Lerp(0.4f, 0.15f, difficulty / 20f);
        backwardChance = Mathf.Lerp(0.2f, 0.05f, difficulty / 20f);
    }

    void MoveToNextRoom()
    {
        if (currentRoom == null)
            return;

        List<Room> connected = GetConnectedRooms(currentRoom);

        // фильтруем занятые комнаты
        connected.RemoveAll(IsRoomOccupied);

        // фильтруем комнаты, которые видит камера
        connected.RemoveAll(IsRoomVisible);

        if (connected.Count == 0)
        {
            if (showDebugLogs)
                Debug.Log($"{name}: некуда идти из {currentRoom.roomName}");
            return;
        }

        Room nextRoom = ChooseNextRoom(connected);

        if (nextRoom == null)
            return;

        if (lastRoom == nextRoom)
        {
            stuckCounter++;
            if (stuckCounter > 2)
            {
                var backwardRooms = connected.Where(r => r.stageLevel < currentRoom.stageLevel).ToList();
                if (backwardRooms.Count > 0)
                {
                    nextRoom = backwardRooms[Random.Range(0, backwardRooms.Count)];
                    if (showDebugLogs)
                        Debug.Log($"{name}: застрял, идёт назад в {nextRoom.roomName}");
                }
                stuckCounter = 0;
            }
        }
        else
        {
            stuckCounter = 0;
        }

        lastRoom = currentRoom;
        currentRoom = nextRoom;

        MoveModelToRoom(currentRoom);

        if (showDebugLogs)
            Debug.Log($"{name} перешёл из {lastRoom.roomName} → {currentRoom.roomName}");

        if (currentRoom == targetRoom)
        {
            Debug.Log($"⚠️ {name} достиг офиса!");
            GameManager.Instance?.TriggerGameOver(name);
        }
    }

    List<Room> GetConnectedRooms(Room room)
    {
        List<Room> result = new();
        if (pathData == null || pathData.pathSegments == null) return result;

        foreach (var seg in pathData.pathSegments)
        {
            if (seg.from == room && seg.to != null)
                result.Add(seg.to);
            else if (seg.to == room && seg.from != null)
                result.Add(seg.from);
        }

        return result.Distinct().ToList();
    }

    Room ChooseNextRoom(List<Room> possibleRooms)
    {
        var forwardRooms = possibleRooms.Where(r => r.stageLevel > currentRoom.stageLevel).ToList();
        var sideRooms = possibleRooms.Where(r => r.stageLevel == currentRoom.stageLevel).ToList();
        var backwardRooms = possibleRooms.Where(r => r.stageLevel < currentRoom.stageLevel).ToList();

        float adjustedBackwardChance = backwardChance + stuckCounter * 0.1f;
        adjustedBackwardChance = Mathf.Clamp(adjustedBackwardChance, backwardChance, 0.5f);

        float total = forwardChance + sideChance + adjustedBackwardChance;
        float roll = Random.value * total;

        if (roll < forwardChance && forwardRooms.Count > 0)
            return forwardRooms[Random.Range(0, forwardRooms.Count)];
        else if (roll < forwardChance + sideChance && sideRooms.Count > 0)
            return sideRooms[Random.Range(0, sideRooms.Count)];
        else if (backwardRooms.Count > 0)
            return backwardRooms[Random.Range(0, backwardRooms.Count)];
        else
            return possibleRooms[Random.Range(0, possibleRooms.Count)];
    }

    void MoveModelToRoom(Room room)
    {
        if (animatronicModel != null)
            animatronicModel.position = room.mapPosition;
        else
            transform.position = room.mapPosition;
    }

    bool IsRoomVisible(Room room)
    {
        if (room == null)
            return false;

        var tab = tabcontroller.Instance;
        if (tab == null || !tab.CamerasActive)
            return false;

        // Проверяем по индексу, если совпадает — считаем видимой
        if (tab.CurrentCameraIndex == room.cameraIndex)
            return true;


        return false;
    }


    bool IsRoomOccupied(Room room)
    {
        if (room == null) return false;

        return allAnimatronics.Any(a =>
            a != this &&
            a.currentRoom == room);
    }
}
