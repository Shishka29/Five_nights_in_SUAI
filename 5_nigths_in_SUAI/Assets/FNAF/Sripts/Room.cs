using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "FNAF/Room")]
public class Room : ScriptableObject
{
    [Header("Room Info")]
    public string roomName; // �������� ������� (Stage, Office � �.�.)
    public Room[] connectedRooms; // ��������� �������
    public int cameraIndex; // ������ ������ (���� �����)

    [Header("Map Settings")]
    public Vector3 mapPosition; // ������� �� ����� (��� �������, UI)
    public int stageLevel; // <-- �����: ������� ������� (��������� ������ � �����)
}
