using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    SerializedProperty connectedRoomsProp;
    Room room;
    Room roomToAdd;

    void OnEnable()
    {
        room = (Room)target;
        connectedRoomsProp = serializedObject.FindProperty("connectedRooms");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Room Settings", EditorStyles.boldLabel);
        room.roomName = EditorGUILayout.TextField("Room Name", room.roomName);
        room.stageLevel = EditorGUILayout.IntField("Stage Level", room.stageLevel);
        room.cameraIndex = EditorGUILayout.IntField("Camera Index", room.cameraIndex);
        room.mapPosition = EditorGUILayout.Vector3Field("Map Position", room.mapPosition);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);

        // Список связанных комнат
        for (int i = 0; i < connectedRoomsProp.arraySize; i++)
        {
            SerializedProperty el = connectedRoomsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(el, GUIContent.none);

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = el.objectReferenceValue;
            }
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                connectedRoomsProp.DeleteArrayElementAtIndex(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        // Добавление новой связи
        roomToAdd = (Room)EditorGUILayout.ObjectField("Add Connection", roomToAdd, typeof(Room), false);
        if (roomToAdd != null && GUILayout.Button("Add"))
        {
            bool exists = false;
            for (int i = 0; i < connectedRoomsProp.arraySize; i++)
            {
                if (connectedRoomsProp.GetArrayElementAtIndex(i).objectReferenceValue == roomToAdd)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                connectedRoomsProp.arraySize++;
                connectedRoomsProp.GetArrayElementAtIndex(connectedRoomsProp.arraySize - 1).objectReferenceValue = roomToAdd;
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "Connection already exists!", "OK");
            }
        }

        EditorGUILayout.Space();

        // Создание новой комнаты
        if (GUILayout.Button("Create new Room asset and connect"))
        {
            string folder = "Assets/FNAF/Data";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/FNAF", "Data");

            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/Room.asset");
            Room newRoom = CreateInstance<Room>();
            newRoom.roomName = "New Room";
            newRoom.stageLevel = room.stageLevel + 1;
            AssetDatabase.CreateAsset(newRoom, path);
            AssetDatabase.SaveAssets();

            connectedRoomsProp.arraySize++;
            connectedRoomsProp.GetArrayElementAtIndex(connectedRoomsProp.arraySize - 1).objectReferenceValue = newRoom;
            EditorGUIUtility.PingObject(newRoom);
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(room);
    }
}
