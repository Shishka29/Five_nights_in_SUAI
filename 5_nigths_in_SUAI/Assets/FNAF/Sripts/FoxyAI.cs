using System.Collections;
using UnityEngine;

public class FoxyAI : MonoBehaviour
{
    [Header("Настройки поведения")]
    [Range(1, 20)] public int difficulty = 5;
    public float minWaitTime = 5f;
    public float maxWaitTime = 15f;

    [Header("Комнаты и объекты")]
    public Room pirateCoveRoom;
    public Room officeRoom;
    public Door officeDoor;

    [Header("Модели по стадиям")]
    public GameObject foxyStage0;
    public GameObject foxyStage1;
    public GameObject foxyStage2;

    [Header("Точки появления стадий (пустые объекты в комнате)")]
    public Transform stage0Pos;
    public Transform stage1Pos;
    public Transform stage2Pos;

    private int foxyStage = 0;
    private bool isRunning = false;
    private bool isRecovering = false;

    void Start()
    {
        if (pirateCoveRoom == null)
        {
            Debug.LogError("❌ У Фокси не назначена комната Pirate Cove!");
            return;
        }

        foxyStage = 0;
        MoveFoxyToStage(); // Телепортируем в стартовую позицию
        UpdateModelVisibility();

        StartCoroutine(BehaviorLoop());
    }

    IEnumerator BehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            if (isRunning || isRecovering) continue;

            // 🚫 Если игрок смотрит на Pirate Cove, Фокси не двигается
            if (IsPlayerWatching())
            {
                Debug.Log("📷 Игрок смотрит на Pirate Cove — Фокси ждёт...");
                continue;
            }

            HandleBehavior();
        }
    }

    void HandleBehavior()
    {
        int roll = Random.Range(0, 20);
        if (roll < difficulty)
        {
            if (foxyStage < 2)
            {
                foxyStage++;
                Debug.Log($"🦊 Фокси выглядывает из Pirate Cove (стадия {foxyStage})");
                MoveFoxyToStage();
                UpdateModelVisibility();
            }
            else if (foxyStage == 2)
            {
                StartCoroutine(RunToOffice());
            }
        }
        else
        {
            Debug.Log("🦊 Фокси остаётся в Pirate Cove, наблюдает...");
        }
    }

    IEnumerator RunToOffice()
    {
        isRunning = true;
        Debug.Log("🏃‍♂️ Фокси выбежал из Pirate Cove и мчится к офису!");

        SetAllModelsActive(false); // Скрываем модели во время бега

        if (officeRoom != null)
            transform.position = officeRoom.mapPosition;

        yield return new WaitForSeconds(2f);

        bool doorClosed = (officeDoor != null && !officeDoor.isOpen);

        if (doorClosed)
        {
            Debug.Log("💥 Фокси врезался в закрытую дверь и возвращается обратно!");
        }
        else
        {
            Debug.Log("⚠️ Фокси ворвался в офис! Игра окончена!");
            GameManager.Instance?.TriggerGameOver("Foxy");
        }

        StartCoroutine(Recover());
    }

    IEnumerator Recover()
    {
        isRunning = false;
        isRecovering = true;

        yield return new WaitForSeconds(5f);

        foxyStage = 0;
        MoveFoxyToStage(); // Возвращаемся в стартовую позицию
        UpdateModelVisibility();

        isRecovering = false;
        Debug.Log("🔄 Фокси вернулся в Pirate Cove и спрятался за занавеской.");
    }

    bool IsPlayerWatching()
    {
        if (pirateCoveRoom == null)
            return false;

        var tab = tabcontroller.Instance;
        if (tab == null || !tab.CamerasActive)
            return false;

        // сравниваем индекс текущей камеры с индексом комнаты
        return tab.CurrentCameraIndex == pirateCoveRoom.cameraIndex;
    }


    void UpdateModelVisibility()
    {
        SetAllModelsActive(false);

        switch (foxyStage)
        {
            case 0: if (foxyStage0) foxyStage0.SetActive(true); break;
            case 1: if (foxyStage1) foxyStage1.SetActive(true); break;
            case 2: if (foxyStage2) foxyStage2.SetActive(true); break;
        }
    }

    void SetAllModelsActive(bool active)
    {
        if (foxyStage0) foxyStage0.SetActive(active);
        if (foxyStage1) foxyStage1.SetActive(active);
        if (foxyStage2) foxyStage2.SetActive(active);
    }

    void MoveFoxyToStage()
    {
        Transform targetPos = foxyStage switch
        {
            0 => stage0Pos,
            1 => stage1Pos,
            2 => stage2Pos,
            _ => stage0Pos
        };

        if (targetPos != null)
        {
            transform.position = targetPos.position;
            transform.rotation = targetPos.rotation;
        }
        else
        {
            Debug.LogWarning("⚠️ Не назначена точка для стадии Фокси!");
        }
    }
}
