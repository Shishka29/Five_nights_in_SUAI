using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "FNAF/Room")]
public class Room : ScriptableObject
{
    [Header("Room Info")]
    public string roomName; // Название комнаты (Stage, Office и т.д.)
    public Room[] connectedRooms; // Связанные комнаты
    public int cameraIndex; // Индекс камеры (если нужно)

    [Header("Map Settings")]
    public Vector3 mapPosition; // Позиция на карте (для моделей, UI)
    public int stageLevel; // <-- ВАЖНО: уровень комнаты (насколько близко к офису)
}
