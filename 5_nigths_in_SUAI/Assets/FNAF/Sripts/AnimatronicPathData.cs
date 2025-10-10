using UnityEngine;

[CreateAssetMenu(fileName = "AnimatronicPath", menuName = "FNAF/Animatronic Path", order = 1)]
public class AnimatronicPathData : ScriptableObject
{
    [Header("Основное")]
    public string animatronicName;

    [Tooltip("Целевая комната (офис)")]
    public Room targetRoom;

    [Header("Цвет маршрута")]
    public Color pathColor = Color.yellow;

    [Header("Связи маршрута (ребра графа)")]
    public PathSegment[] pathSegments;
}

[System.Serializable]
public class PathSegment
{
    public Room from;
    public Room to;
}
