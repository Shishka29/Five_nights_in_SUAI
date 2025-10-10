using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "FNAF/Room")]
public class Room : ScriptableObject
{

    [Header("Room Info")]
    public string roomName;
    public Room[] connectedRooms;

    [Tooltip("Объект камеры, которая показывает эту комнату (если есть)")]
    public Camera linkedCamera;

    [Tooltip("Индекс камеры (для интерфейса или переключения)")]
    public int cameraIndex;

    [Header("Map Settings")]
    public Vector3 mapPosition;
    public int stageLevel;
}
