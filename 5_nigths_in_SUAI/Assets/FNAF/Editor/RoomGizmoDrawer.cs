using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class RoomGizmoDrawer
{
    static RoomGizmoDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Handles.color = Color.cyan;

        Room[] allRooms = Resources.FindObjectsOfTypeAll<Room>();
        foreach (Room room in allRooms)
        {
            if (room == null) continue;

            Vector3 pos = room.mapPosition;
            Handles.SphereHandleCap(0, pos, Quaternion.identity, 0.3f, EventType.Repaint);
            Handles.Label(pos + Vector3.up * 0.4f, $"{room.roomName} ({room.stageLevel})");

            if (room.connectedRooms != null)
            {
                foreach (Room connected in room.connectedRooms)
                {
                    if (connected == null) continue;
                    Handles.DrawLine(room.mapPosition, connected.mapPosition);
                }
            }
        }
    }
}
