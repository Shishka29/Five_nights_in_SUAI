using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement; // <- добавляем

public class AnimatronicAI : MonoBehaviour
{
    [Header("Стартовая сложность")]
    [Range(1, 20)] public int startDifficulty = 10;

    [Header("References")]
    public Transform animatronicModel;
    public AnimatronicPathData pathData;

    [Header("AI Settings")]
    public Room currentRoom;
    public Room targetRoom; // офис

    [Header("Door Settings")]
    public Door officeDoor;
    public Room startRoom;

    [Header("Light Check Rooms (комнаты у дверей)")]
    public Room leftDoorRoom;    // комната у левой двери
    public Room rightDoorRoom;   // комната у правой двери
    public LightButton leftLight;   // ссылка на левый свет
    public LightButton rightLight;  // ссылка на правый свет

    [Header("Movement Chances")]
    [Range(0f, 1f)] public float forwardChance = 0.7f;
    [Range(0f, 1f)] public float sideChance = 0.2f;
    [Range(0f, 1f)] public float backwardChance = 0.1f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Control")]
    public bool CanMove = false;
    public int difficulty;

    private Room lastRoom;
    private float timer;
    private int stuckCounter = 0;
    private List<Room> allowedRooms = new();
    private static List<AnimatronicAI> allAnimatronics = new();
    private bool isRecovering = false;

    [Header("Screamer Settings")]
    public bool hasScreamer = false;              // Включить скример для этого аниматроника
    public GameObject screamerModel;              // Модель скримера
    public Animator screamerAnimator;             // Аниматор скримера
    public AudioSource screamerAudioSource;       // Аудио проигрыватель
    public AudioClip screamerSound;               // Звук скримера
    public string screamerAnimationName = "Jumpscare"; // Имя анимации
    public float gameOverDelay = 2.5f;            // Задержка перед сценой GameOver
    public string gameOverScene = "GameOver";     // Имя сцены GameOver

    [Header("Player & Camera Settings")]
    public herosqript playerLook;               // Скрипт вращения игрока на объекте Player
    public Camera mainCamera;                   // Камера, которая висит на Player
    public Vector3 inspectorPlayerEulerAngles;  // Поворот Player во время скримера
    public float playerRotationSpeed = 5f;      // Скорость плавного поворота Player


    [Header("Screamer Trigger Settings")]
    public bool startScreamerIfTabletOpen = true; // Можно запускать, если планшет открыт
    public Vector2 playerYawRange = new Vector2(150f, 210f);   // диапазон углов по Y (лево-право)
    public Vector2 playerPitchRange = new Vector2(-10f, 30f);  // диапазон углов по X (вверх-вниз)


    void Awake()
    {
        if (!allAnimatronics.Contains(this))
            allAnimatronics.Add(this);
    }

    void OnDestroy()
    {
        allAnimatronics.Remove(this);
    }

    void Start()
    {
        // Собираем список комнат из pathData
        if (pathData != null && pathData.pathSegments != null)
        {
            var roomSet = new HashSet<Room>();
            foreach (var seg in pathData.pathSegments)
            {
                if (seg.from != null) roomSet.Add(seg.from);
                if (seg.to != null) roomSet.Add(seg.to);
            }
            allowedRooms = roomSet.ToList();
        }

        if (currentRoom == null && allowedRooms.Count > 0)
            currentRoom = allowedRooms[0];

        if (currentRoom != null)
            MoveModelToRoom(currentRoom);

        difficulty = startDifficulty;

        if (showDebugLogs)
            Debug.Log($"{name}: ждёт разрешения на движение...");
    }

    void Update()
    {
        if (!CanMove || isRecovering) return;

        // 🚫 Если сейчас свет горит в комнате, где он стоит — замираем
        if (IsLightAffectingCurrentRoom())
        {
            if (showDebugLogs)
                Debug.Log($"{name}: стоит в {currentRoom.roomName}, свет включён — жду...");
            return;
        }

        timer += Time.deltaTime;
        float minInterval = 4f;
        float maxInterval = 8f;
        float adjustedInterval = Mathf.Lerp(maxInterval, minInterval, difficulty / 20f);

        if (timer < adjustedInterval) return;
        timer = 0f;

        // Если камера видит текущую комнату — ждем
        if (IsRoomVisible(currentRoom)) return;

        // Шанс на пропуск хода
        float chance = Random.Range(0f, 20f);
        if (chance > difficulty) return;

        UpdateChancesByDifficulty();
        MoveToNextRoom();
    }

    public void ActivateAI()
    {
        CanMove = true;
        difficulty = startDifficulty;
        timer = 0f;

        if (showDebugLogs)
            Debug.Log($"{name} активирован! Сложность = {difficulty}");
    }

    void UpdateChancesByDifficulty()
    {
        forwardChance = Mathf.Lerp(0.4f, 0.8f, difficulty / 20f);
        sideChance = Mathf.Lerp(0.4f, 0.15f, difficulty / 20f);
        backwardChance = Mathf.Lerp(0.2f, 0.05f, difficulty / 20f);
    }

    private bool hasTriggeredScreamer = false;

    void MoveToNextRoom()
    {
        if (currentRoom == null) return;

        // Получаем все доступные соседние комнаты
        List<Room> connected = GetConnectedRooms(currentRoom);
        connected.RemoveAll(IsRoomOccupied);
        connected.RemoveAll(IsRoomVisible);
        connected = connected.Where(r => !IsLightBlocking(r)).ToList(); // не идти в комнаты с включенным светом

        if (connected.Count == 0)
        {
            if (showDebugLogs)
                Debug.Log($"{name}: некуда идти из {currentRoom.roomName}");
            return;
        }

        Room nextRoom = ChooseNextRoom(connected);
        if (nextRoom == null) return;

        lastRoom = currentRoom;

        // 🔹 Если следующая комната — офис
        if (nextRoom == targetRoom)
        {
            // Дверь закрыта — возвращаемся на старт
            if (officeDoor != null && !officeDoor.isOpen)
            {
                PlayerData.Instance.AddReflectedAttack();
                if (showDebugLogs)
                    Debug.Log($"{name}: дверь офиса закрыта, возвращаюсь на старт!");
                StartCoroutine(Recover());
                return;
            }

            // Условия для скримера не выполнены — остаёмся на месте
            if (!CheckScreamerConditions())
            {
                if (showDebugLogs)
                    Debug.Log($"{name}: жду условий для скримера, не вхожу в офис.");
                return;
            }
        }

        // Можно переходить в комнату
        currentRoom = nextRoom;
        MoveModelToRoom(currentRoom);

        if (showDebugLogs)
            Debug.Log($"{name} перешёл из {lastRoom.roomName} → {currentRoom.roomName}");

        // Если это офис — запускаем скример
        if (currentRoom == targetRoom)
        {
            TriggerScreamer();
            NightManager.Instance.TriggerGameOver(name);
        }
    }



    bool CheckScreamerConditions()
    {
        // Планшет открыт
        var tab = tabcontroller.Instance;
        if (startScreamerIfTabletOpen && tab != null && tab.IsTabletOpen)
            return true;

        // Игрок смотрит в диапазон
        if (playerLook != null)
        {
            Vector3 euler = playerLook.transform.eulerAngles;
            float yaw = NormalizeAngle(euler.y);
            float pitch = NormalizeAngle(euler.x);

            if (yaw >= playerYawRange.x && yaw <= playerYawRange.y &&
                pitch >= playerPitchRange.x && pitch <= playerPitchRange.y)
                return true;
        }

        return false;
    }


    private bool ShouldTriggerScreamer()
    {
        var tab = tabcontroller.Instance;
        bool tabletOpen = startScreamerIfTabletOpen && tab != null && tab.IsTabletOpen;

        bool playerLooking = false;
        if (playerLook != null)
        {
            Vector3 euler = playerLook.transform.eulerAngles;
            float yaw = NormalizeAngle(euler.y);
            float pitch = NormalizeAngle(euler.x);
            playerLooking = yaw >= playerYawRange.x && yaw <= playerYawRange.y &&
                            pitch >= playerPitchRange.x && pitch <= playerPitchRange.y;
        }

        return tabletOpen || playerLooking;
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    void TriggerScreamer()
    {
        if (!hasScreamer || screamerModel == null) return;

        // 🔹 Блокируем вращение Player
        if (playerLook != null)
            playerLook.BlockLook(true);

        // 🔹 Скрываем обычную модель аниматроника
        if (animatronicModel != null)
            animatronicModel.gameObject.SetActive(false);

        // 🔹 Закрываем планшет
        var tab = tabcontroller.Instance;
        if (tab != null && tab.IsTabletOpen)
            tab.Close();

        // 🔹 Включаем модель скримера
        screamerModel.SetActive(true);

        // 🔹 Поворот Player на инспекторные углы
        if (playerLook != null)
            StartCoroutine(RotatePlayerToInspectorRotation());

        // 🔹 Анимация скримера
        if (screamerAnimator != null && !string.IsNullOrEmpty(screamerAnimationName))
            screamerAnimator.Play(screamerAnimationName, 0, 0f);

        // 🔹 Проигрываем звук
        if (screamerAudioSource != null && screamerSound != null)
            screamerAudioSource.PlayOneShot(screamerSound);

        // 🔹 Переход в GameOver
        StartCoroutine(GameOverDelayCoroutine());
    }

    private IEnumerator RotatePlayerToInspectorRotation()
    {
        Quaternion targetRotation = Quaternion.Euler(inspectorPlayerEulerAngles);

        while (Quaternion.Angle(playerLook.transform.rotation, targetRotation) > 0.5f)
        {
            playerLook.transform.rotation = Quaternion.Slerp(
                playerLook.transform.rotation,
                targetRotation,
                Time.deltaTime * playerRotationSpeed
            );
            yield return null;
        }

        playerLook.transform.rotation = targetRotation;
    }


    // Корутина для GameOver
    private IEnumerator GameOverDelayCoroutine()
    {
        yield return new WaitForSeconds(gameOverDelay);
        SceneManager.LoadScene(gameOverScene);
    }

   






    // 🔸 Проверка: свет мешает ли входу в комнату
    bool IsLightBlocking(Room nextRoom)
    {
        if (leftLight != null && leftDoorRoom == nextRoom && leftLight.IsLightOn)
        {
            if (showDebugLogs)
                Debug.Log($"{name}: не может подойти к {nextRoom.roomName}, левый свет включён!");
            return true;
        }
        if (rightLight != null && rightDoorRoom == nextRoom && rightLight.IsLightOn)
        {
            if (showDebugLogs)
                Debug.Log($"{name}: не может подойти к {nextRoom.roomName}, правый свет включён!");
            return true;
        }
        return false;
    }

    // 🔸 Проверка: свет мешает ли текущей комнате
    bool IsLightAffectingCurrentRoom()
    {
        if (leftLight != null && leftDoorRoom == currentRoom && leftLight.IsLightOn)
            return true;
        if (rightLight != null && rightDoorRoom == currentRoom && rightLight.IsLightOn)
            return true;
        return false;
    }

    IEnumerator Recover()
    {
        isRecovering = true;
        yield return new WaitForSeconds(0.3f);

        if (startRoom != null)
        {
            currentRoom = startRoom;
            lastRoom = null;
            MoveModelToRoom(currentRoom);
            if (showDebugLogs)
                Debug.Log($"{name} вернулся на стартовую позицию ({startRoom.roomName})");
        }

        isRecovering = false;
    }

    List<Room> GetConnectedRooms(Room room)
    {
        List<Room> result = new();
        if (pathData == null || pathData.pathSegments == null) return result;

        foreach (var seg in pathData.pathSegments)
        {
            if (seg.from == room && seg.to != null) result.Add(seg.to);
            else if (seg.to == room && seg.from != null) result.Add(seg.from);
        }

        return result.Distinct().ToList();
    }

    Room ChooseNextRoom(List<Room> possibleRooms)
    {
        var forwardRooms = possibleRooms.Where(r => r.stageLevel > currentRoom.stageLevel).ToList();
        var sideRooms = possibleRooms.Where(r => r.stageLevel == currentRoom.stageLevel).ToList();
        var backwardRooms = possibleRooms.Where(r => r.stageLevel < currentRoom.stageLevel).ToList();

        float adjustedBackwardChance = backwardChance + stuckCounter * 0.1f;
        adjustedBackwardChance = Mathf.Clamp(adjustedBackwardChance, backwardChance, 0.5f);

        float total = forwardChance + sideChance + adjustedBackwardChance;
        float roll = Random.value * total;

        if (roll < forwardChance && forwardRooms.Count > 0)
            return forwardRooms[Random.Range(0, forwardRooms.Count)];
        else if (roll < forwardChance + sideChance && sideRooms.Count > 0)
            return sideRooms[Random.Range(0, sideRooms.Count)];
        else if (backwardRooms.Count > 0)
            return backwardRooms[Random.Range(0, backwardRooms.Count)];
        else
            return possibleRooms[Random.Range(0, possibleRooms.Count)];
    }

    void MoveModelToRoom(Room room)
    {
        if (animatronicModel != null) animatronicModel.position = room.mapPosition;
        else transform.position = room.mapPosition;
    }

    bool IsRoomVisible(Room room)
    {
        if (room == null) return false;

        var tab = tabcontroller.Instance;
        if (tab == null || !tab.CamerasActive) return false;

        return tab.CurrentCameraIndex == room.cameraIndex;
    }

    bool IsRoomOccupied(Room room)
    {
        if (room == null) return false;

        return allAnimatronics.Any(a => a != this && a.currentRoom == room);
    }
}
