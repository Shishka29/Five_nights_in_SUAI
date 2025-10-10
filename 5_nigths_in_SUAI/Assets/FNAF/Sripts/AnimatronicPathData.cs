using UnityEngine;

[CreateAssetMenu(fileName = "AnimatronicPath", menuName = "FNAF/Animatronic Path", order = 1)]
public class AnimatronicPathData : ScriptableObject
{
    [Header("��������")]
    public string animatronicName;

    [Tooltip("������� ������� (����)")]
    public Room targetRoom;

    [Header("���� ��������")]
    public Color pathColor = Color.yellow;

    [Header("����� �������� (����� �����)")]
    public PathSegment[] pathSegments;
}

[System.Serializable]
public class PathSegment
{
    public Room from;
    public Room to;
}
