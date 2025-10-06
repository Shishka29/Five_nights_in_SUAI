using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CameraPoint : MonoBehaviour
{
    [Header("Camera Settings")]
    public string cameraName = "Camera1";

    [Tooltip("������ ������������ �����-����� � �����")]
    [Range(0.1f, 2f)] public float gizmoSize = 0.5f;

    [Tooltip("���� ����������� ������ � �����")]
    public Color gizmoColor = Color.yellow;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
        Handles.Label(transform.position + Vector3.up * (gizmoSize + 0.2f), cameraName);
    }
#endif
}
