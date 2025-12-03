using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FoxyAI : MonoBehaviour
{
    [Header("Настройки поведения")]
    [Range(1, 20)] public int startDifficulty = 5;
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

    [Header("Точки появления стадий")]
    public Transform stage0Pos;
    public Transform stage1Pos;
    public Transform stage2Pos;

    [Header("Скример")]
    public GameObject screamerModel;
    public Animator screamerAnimator;
    public AudioSource screamerAudioSource;
    public AudioClip screamerSound;
    public string screamerAnimationName = "Jumpscare";
    public float screamerDelay = 2f;
    public float gameOverDelay = 2.5f;
    public string gameOverScene = "GameOver";

    [Header("Player & Camera Settings")]
    public herosqript playerLook;
    public Vector3 inspectorPlayerEulerAngles;
    public float playerRotationSpeed = 5f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    [HideInInspector] public bool CanMove = false;
    [HideInInspector] public int difficulty;

    private int foxyStage = 0;
    private bool isRunning = false;
    private bool isRecovering = false;


    void Start()
    {
        foxyStage = 0;
        difficulty = startDifficulty;

        MoveFoxyToStage();
        UpdateModelVisibility();

        if (screamerModel) screamerModel.SetActive(false);

        StartCoroutine(BehaviorLoop());
    }

    IEnumerator BehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

            if (!CanMove || isRunning || isRecovering)
                continue;

            if (IsPlayerWatching())
            {
                if (showDebugLogs)
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
                MoveFoxyToStage();
                UpdateModelVisibility();

                if (showDebugLogs)
                    Debug.Log($"🦊 Фокси перешёл в стадию {foxyStage}");
            }
            else if (foxyStage == 2)
            {
                StartCoroutine(RunToOffice());
            }
        }
    }

    // -------------------------------------------------------
    //     ФОКСИ ВЫБЕГАЕТ — БЕЗ АНИМАЦИИ, ПРОСТО ТЕЛЕПОРТ
    // -------------------------------------------------------
    IEnumerator RunToOffice()
    {
        isRunning = true;

        if (showDebugLogs)
            Debug.Log("🏃‍♂️ Фокси выбежал!");

        // выключить визуальные стадии полностью
        SetAllModelsActive(false);

        // Телепорт в офис
        if (officeRoom != null)
            transform.position = officeRoom.mapPosition;

        // Пауза перед проверкой двери
        yield return new WaitForSeconds(screamerDelay);

        bool doorClosed = (officeDoor != null && !officeDoor.isOpen);

        if (doorClosed)
        {
            StartCoroutine(Recover());
        }
        else
        {
            StartCoroutine(TriggerScreamer());
        }

        isRunning = false;
    }

    IEnumerator TriggerScreamer()
    {
        var tab = tabcontroller.Instance;
        if (tab != null && tab.IsTabletOpen)
            tab.Close();

        if (playerLook != null)
            playerLook.BlockLook(true);

        if (screamerModel) screamerModel.SetActive(true);

        StartCoroutine(RotatePlayerToInspectorRotation());

        if (screamerAnimator)
            screamerAnimator.Play(screamerAnimationName, 0, 0f);

        if (screamerAudioSource && screamerSound)
            screamerAudioSource.PlayOneShot(screamerSound);

        yield return new WaitForSeconds(gameOverDelay);
        SceneManager.LoadScene(gameOverScene);
    }

    IEnumerator RotatePlayerToInspectorRotation()
    {
        Quaternion target = Quaternion.Euler(inspectorPlayerEulerAngles);

        while (Quaternion.Angle(playerLook.transform.rotation, target) > 0.5f)
        {
            playerLook.transform.rotation = Quaternion.Slerp(
                playerLook.transform.rotation,
                target,
                Time.deltaTime * playerRotationSpeed
            );
            yield return null;
        }

        playerLook.transform.rotation = target;
    }

    IEnumerator Recover()
    {
        isRecovering = true;
        PlayerData.Instance.AddReflectedAttack();

        yield return new WaitForSeconds(5f);

        foxyStage = 0;
        MoveFoxyToStage();
        UpdateModelVisibility();

        if (showDebugLogs)
            Debug.Log("🔄 Фокси вернулся в Pirate Cove.");

        TeleportModel();
        isRecovering = false;
    }

    bool IsPlayerWatching()
    {
        var tab = tabcontroller.Instance;
        if (tab == null || !tab.CamerasActive)
            return false;

        return tab.CurrentCameraIndex == pirateCoveRoom.cameraIndex;
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
    }

    void UpdateModelVisibility()
    {
        SetAllModelsActive(false);

        if (foxyStage == 0 && foxyStage0) foxyStage0.SetActive(true);
        if (foxyStage == 1 && foxyStage1) foxyStage1.SetActive(true);
        if (foxyStage == 2 && foxyStage2) foxyStage2.SetActive(true);
    }

    void SetAllModelsActive(bool active)
    {
        if (foxyStage0) foxyStage0.SetActive(active);
        if (foxyStage1) foxyStage1.SetActive(active);
        if (foxyStage2) foxyStage2.SetActive(active);
    }

    public Transform targetPoint;   // точка возврата
    public GameObject model;        // модель фокси

    public void TeleportModel()
    {
        if (model != null && targetPoint != null)
        {
            model.transform.position = targetPoint.position;
            model.transform.rotation = targetPoint.rotation;
        }
    }
}
