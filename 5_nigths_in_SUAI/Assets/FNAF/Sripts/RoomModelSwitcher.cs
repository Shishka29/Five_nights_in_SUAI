using System.Collections.Generic;
using UnityEngine;

public class RoomModelSwitcher : MonoBehaviour
{
    [Header("AI Reference")]
    public AnimatronicAI ai; // ссылка на твой AI

    [Header("ћодели по комнатам")]
    public List<RoomModelData> roomModels = new List<RoomModelData>();

    private Room lastRoom;

    void Start()
    {
        if (ai == null) ai = GetComponent<AnimatronicAI>();
        UpdateModel();
    }

    void Update()
    {
        if (ai == null || ai.currentRoom == null) return;

        if (lastRoom != ai.currentRoom)
        {
            UpdateModel();
            lastRoom = ai.currentRoom;
        }
    }

    void UpdateModel()
    {
        // выключаем все модели
        foreach (var modelData in roomModels)
        {
            if (modelData.model != null)
                modelData.model.SetActive(false);
        }

        // включаем только ту, что относитс€ к комнате
        foreach (var modelData in roomModels)
        {
            if (modelData.room == ai.currentRoom && modelData.model != null)
            {
                modelData.model.SetActive(true);
                return;
            }
        }
    }
}

[System.Serializable]
public class RoomModelData
{
    public Room room;         // ссылка на комнату
    public GameObject model;  // модель, котора€ должна включатьс€ в этой комнате
}
