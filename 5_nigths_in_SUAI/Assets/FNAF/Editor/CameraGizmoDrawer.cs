using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class CameraGizmoDrawer
{
    static CameraGizmoDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        // Получаем все точки камер
        CameraPoint[] allCameras = Resources.FindObjectsOfTypeAll<CameraPoint>();

        foreach (CameraPoint cam in allCameras)
        {
            if (cam == null)
                continue;

            Handles.color = cam.gizmoColor;

            // Рисуем шар и подпись всегда
            Handles.SphereHandleCap(0, cam.transform.position, Quaternion.identity, cam.gizmoSize, EventType.Repaint);
            Handles.Label(cam.transform.position + Vector3.up * (cam.gizmoSize + 0.2f), cam.cameraName);

            // Проверяем, выбрана ли одна из камер
            if (Selection.activeGameObject != null && Selection.activeGameObject.name.StartsWith("Camera"))
            {
                EditorGUI.BeginChangeCheck();

                // Ручки для перемещения и вращения
                Vector3 newPos = Handles.PositionHandle(cam.transform.position, cam.transform.rotation);
                Quaternion newRot = Handles.RotationHandle(cam.transform.rotation, cam.transform.position);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cam.transform, "Move or Rotate Camera Point");
                    cam.transform.position = newPos;
                    cam.transform.rotation = newRot;
                    EditorUtility.SetDirty(cam);
                }
            }
        }
    }
}
