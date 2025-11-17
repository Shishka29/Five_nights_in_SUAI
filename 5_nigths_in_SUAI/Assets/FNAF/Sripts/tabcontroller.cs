using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 🟢 добавляем, чтобы работать с Image
using static Battery;

public class tabcontroller : MonoBehaviour
{
    public static tabcontroller Instance;
    public Battery energy;

    [Header("Cameras")]
    public GameObject mainCamera;
    public GameObject[] cameras;

    [Header("Minimap / Tablet")]
    public GameObject minimap;
    public GameObject Tablet;

    private Animator anim;
    private int currentCameraIndex = 0;

    [System.Serializable]
    public class CameraLightGroup
    {
        [Tooltip("Источники света для этой камеры (может быть несколько ламп)")]
        public Light[] lights;

        [Tooltip("Максимальная яркость для каждой лампы (по порядку)")]
        public float[] maxIntensities;
    }

    [Header("Camera Lights")]
    public CameraLightGroup[] cameraLights;

    [Tooltip("Скорость плавного включения/выключения света")]
    public float lightFadeSpeed = 8f;

    private bool camerasActive = false;
    public bool CamerasActive => camerasActive;

    [Header("Audio Sources")]
    public AudioSource audioSource;     // источник для планшета и переключений
    public AudioSource lightAudio;      // отдельный источник для звука света

    [Header("Sounds")]
    public AudioClip soundMain;         // звук открытия/закрытия
    public AudioClip soundLoop;         // фоновый при открытии
    public AudioClip soundTap;          // звук при переключении камеры
    public AudioClip soundLightsTab;    // жужжание света (loop)

    private bool lightLoopPlaying = false;

    // 🟢 Добавляем секцию для спрайтов
    [Header("Camera Sprites")]
    public Image cameraDisplay;         // UI-элемент, где будет показан спрайт
    public Sprite[] cameraSprites;      // спрайты для каждой камеры (по индексу)

    [Header("Room Sprites")]
    public Image roomDisplay;         
    public Sprite[] roomSprites;      

    void Awake()
    {
        Instance = this;

        if (Tablet != null)
            anim = Tablet.GetComponent<Animator>();

        TurnOffAllLightsInstant();
    }
  

    void Update()
    {
        if (energy.energy <= 0)
        {
            Close();
        }
        if (minimap != null && minimap.activeSelf)
        {
            if (camerasActive && cameraLights != null && currentCameraIndex < cameraLights.Length)
                HandleCameraLights();
            else
                FadeOutAllLights();
        }
    }

    void HandleCameraLights()
    {
        var group = cameraLights[currentCameraIndex];
        if (group.lights == null) return;

        bool mouseOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool isPressed = Input.GetMouseButton(0) && !mouseOverUI;

        // 🎧 звук включения света при зажатии
        if (isPressed && !lightLoopPlaying)
        {
            if (lightAudio && soundLightsTab)
            {
                lightAudio.clip = soundLightsTab;
                lightAudio.loop = true;
                lightAudio.Play();
                lightLoopPlaying = true;
            }
        }
        else if (!isPressed && lightLoopPlaying)
        {
            if (lightAudio && lightAudio.isPlaying)
                lightAudio.Stop();
            lightLoopPlaying = false;
        }

        // управление яркостью ламп
        for (int i = 0; i < group.lights.Length; i++)
        {
            var light = group.lights[i];
            if (light == null) continue;

            float max = (group.maxIntensities != null && i < group.maxIntensities.Length)
                ? group.maxIntensities[i]
                : 3f;

            float target = isPressed ? max : 0f;
            light.intensity = Mathf.Lerp(light.intensity, target, Time.deltaTime * lightFadeSpeed);
        }
    }

    public void tabChangeVisible()
    {
        // Не позволяем открыть планшет при нулевой энергии
        if (energy.energy <= 0)
        {
            Close();
            return;
        }
        // Переключаем состояние: если открыт - закрываем, если закрыт - открываем
        if (minimap != null && minimap.activeSelf)
        {
            Close();
        }
        else
        {
            StartCoroutine(Open());
        }
    }

    IEnumerator Open()
    {
        // звук открытия
        if (audioSource && soundMain)
            audioSource.PlayOneShot(soundMain);
        if (anim != null) anim.SetBool("isOpen", true);
        yield return new WaitForSeconds(0.4f);

        if (minimap != null) minimap.SetActive(true);
        if (mainCamera != null) mainCamera.SetActive(false);

        if (cameras != null && cameras.Length > 0)
            cameras[currentCameraIndex].SetActive(true);

        // фоновый звук планшета
        if (audioSource && soundLoop)
        {
            audioSource.clip = soundLoop;
            audioSource.loop = true;
            audioSource.Play();
        }

        camerasActive = true;

        // 🟢 Устанавливаем спрайт текущей камеры при открытии планшета
        UpdateCameraSprite(currentCameraIndex);
        UpdateRoomSprite(currentCameraIndex);
    }

    void Close()
    {
        // выключаем фоновый звук планшета
        if (audioSource && audioSource.clip == soundLoop)
            audioSource.Stop();

        // выключаем световой звук, если держали
        if (lightAudio && lightAudio.isPlaying)
            lightAudio.Stop();
        lightLoopPlaying = false;

        // звук закрытия
        if (audioSource && soundMain)
            audioSource.PlayOneShot(soundMain);

        if (cameras != null && cameras.Length > 0)
            cameras[currentCameraIndex].SetActive(false);

        if (mainCamera != null) mainCamera.SetActive(true);
        if (minimap != null) minimap.SetActive(false);

        if (anim != null) anim.SetBool("isOpen", false);

        camerasActive = false;
        TurnOffAllLightsInstant();
    }

    public void ChangeCamera(int index)
    {
        // звук смены камеры
        if (audioSource && soundTap)
            audioSource.PlayOneShot(soundTap);

        if (cameras == null || index < 0 || index >= cameras.Length) return;

        cameras[currentCameraIndex].SetActive(false);
        TurnOffLights(currentCameraIndex);

        currentCameraIndex = index;

        cameras[currentCameraIndex].SetActive(true);
        TurnOffLights(currentCameraIndex);

        // 🟢 Меняем спрайт на новый
        UpdateCameraSprite(currentCameraIndex);
        UpdateRoomSprite(currentCameraIndex);
    }

    // 🟢 Функция обновления спрайта
    void UpdateCameraSprite(int index)
    {
        if (cameraDisplay != null && cameraSprites != null && index < cameraSprites.Length && cameraSprites[index] != null)
        {
            cameraDisplay.sprite = cameraSprites[index];
        }
    }

    // 🟢 Функция обновления спрайта комнаты
    void UpdateRoomSprite(int index)
    {
        if (roomDisplay != null && roomSprites != null && index < roomSprites.Length && roomSprites[index] != null)
        {
            roomDisplay.sprite = roomSprites[index];
        }
    }


    void TurnOffLights(int cameraIndex)
    {
        if (cameraLights == null || cameraIndex >= cameraLights.Length) return;

        var group = cameraLights[cameraIndex];
        if (group.lights == null) return;

        foreach (var l in group.lights)
            if (l != null) l.intensity = 0f;
    }

    void FadeOutAllLights()
    {
        if (cameraLights == null) return;

        foreach (var group in cameraLights)
        {
            if (group.lights == null) continue;
            foreach (var l in group.lights)
            {
                if (l != null)
                    l.intensity = Mathf.Lerp(l.intensity, 0f, Time.deltaTime * lightFadeSpeed);
            }
        }
    }

    void TurnOffAllLightsInstant()
    {
        if (cameraLights == null) return;

        foreach (var group in cameraLights)
        {
            if (group.lights == null) continue;
            foreach (var l in group.lights)
                if (l != null) l.intensity = 0f;
        }
    }

    public int CurrentCameraIndex => camerasActive ? currentCameraIndex : -1;
    public GameObject CurrentCamera => camerasActive ? cameras[currentCameraIndex] : null;
}
