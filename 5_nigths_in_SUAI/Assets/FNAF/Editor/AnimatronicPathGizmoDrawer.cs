using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AnimatronicPathGizmoDrawer
{
    static AnimatronicPathGizmoDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        // Находим все маршруты в проекте
        AnimatronicPathData[] allPaths = Resources.FindObjectsOfTypeAll<AnimatronicPathData>();
        if (allPaths == null || allPaths.Length == 0)
            return;

        foreach (var path in allPaths)
        {
            if (path.pathSegments == null || path.pathSegments.Length == 0)
                continue;

            Handles.color = path.pathColor;

            foreach (var seg in path.pathSegments)
            {
                if (seg == null || seg.from == null || seg.to == null)
                    continue;

                Vector3 fromPos = seg.from.mapPosition;
                Vector3 toPos = seg.to.mapPosition;

                // Рисуем соединительную линию
                Handles.DrawLine(fromPos, toPos);

                // Отображаем точки и подписи
                Handles.SphereHandleCap(0, fromPos, Quaternion.identity, 0.3f, EventType.Repaint);
                Handles.SphereHandleCap(0, toPos, Quaternion.identity, 0.3f, EventType.Repaint);

                Handles.Label((fromPos + toPos) / 2 + Vector3.up * 0.2f,
                    $"{path.animatronicName}", new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = path.pathColor },
                        fontStyle = FontStyle.Bold
                    });
            }
        }
    }
}
