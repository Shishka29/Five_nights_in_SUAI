using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AnimatronicPathData))]
public class AnimatronicPathEditor : Editor
{
    private AnimatronicPathData pathData;
    private Room fromRoom;
    private Room toRoom;

    private void OnEnable()
    {
        pathData = (AnimatronicPathData)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("������� ������������", EditorStyles.boldLabel);
        pathData.animatronicName = EditorGUILayout.TextField("��� ������������", pathData.animatronicName);
        pathData.targetRoom = (Room)EditorGUILayout.ObjectField("������� ������� (����)", pathData.targetRoom, typeof(Room), false);
        pathData.pathColor = EditorGUILayout.ColorField("���� ��������", pathData.pathColor);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("����� ����� ���������", EditorStyles.boldLabel);

        if (pathData.pathSegments != null && pathData.pathSegments.Length > 0)
        {
            for (int i = 0; i < pathData.pathSegments.Length; i++)
            {
                var seg = pathData.pathSegments[i];
                EditorGUILayout.BeginHorizontal();
                seg.from = (Room)EditorGUILayout.ObjectField(seg.from, typeof(Room), false);
                seg.to = (Room)EditorGUILayout.ObjectField(seg.to, typeof(Room), false);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    Undo.RecordObject(pathData, "Remove Segment");
                    var list = new List<PathSegment>(pathData.pathSegments);
                    list.RemoveAt(i);
                    pathData.pathSegments = list.ToArray();
                    EditorUtility.SetDirty(pathData);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("��� ������ ����� ���������.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("�������� ����� �����", EditorStyles.boldLabel);

        fromRoom = (Room)EditorGUILayout.ObjectField("�� �������", fromRoom, typeof(Room), false);
        toRoom = (Room)EditorGUILayout.ObjectField("� �������", toRoom, typeof(Room), false);

        if (fromRoom != null && toRoom != null && GUILayout.Button("�������� �����"))
        {
            Undo.RecordObject(pathData, "Add Segment");
            var newList = new List<PathSegment>(pathData.pathSegments ?? new PathSegment[0]);
            if (!newList.Exists(s => (s.from == fromRoom && s.to == toRoom) || (s.from == toRoom && s.to == fromRoom)))
            {
                newList.Add(new PathSegment { from = fromRoom, to = toRoom });
                pathData.pathSegments = newList.ToArray();
                EditorUtility.SetDirty(pathData);
            }
            else
            {
                EditorUtility.DisplayDialog("��������", "��� ����� ��� ����������!", "OK");
            }

            fromRoom = null;
            toRoom = null;
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("�������� �������"))
        {
            if (EditorUtility.DisplayDialog("�������������", "������� ��� �����?", "��", "������"))
            {
                Undo.RecordObject(pathData, "Clear Path");
                pathData.pathSegments = new PathSegment[0];
                EditorUtility.SetDirty(pathData);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
