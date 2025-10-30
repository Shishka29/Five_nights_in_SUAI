using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


public class tabcontroller : MonoBehaviour
{
    public static tabcontroller Instance;

    [Header("Tablet UI")]
    public GameObject Tablet;
    public Animator anim;
    public GameObject minimap;

    [Header("Cameras")]
    public GameObject[] cameras;
    public GameObject mainCamera;
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
    [Tooltip("Источники света для каждой камеры (индексы совпадают с массивом камер)")]
    public CameraLightGroup[] cameraLights;

    [Tooltip("Скорость плавного включения/выключения света")]
    public float lightFadeSpeed = 8f;

    private bool camerasActive = false;
    public bool CamerasActive => camerasActive;

    void Awake()
    {
        Instance = this;
        if (Tablet != null)
            anim = Tablet.GetComponent<Animator>();

        // при старте выключаем все лампы
        TurnOffAllLightsInstant();
    }

    void Update()
    {
        if (camerasActive && cameraLights != null && currentCameraIndex < cameraLights.Length)
        {
            var group = cameraLights[currentCameraIndex];
            if (group.lights != null)
            {
                bool mouseOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

                for (int i = 0; i < group.lights.Length; i++)
                {
                    var light = group.lights[i];
                    if (light == null) continue;

                    float max = (group.maxIntensities != null && i < group.maxIntensities.Length)
                        ? group.maxIntensities[i]
                        : 3f;

                    //  свет включается ТОЛЬКО если нажата ЛКМ и курсор не над UI
                    float target = (Input.GetMouseButton(0) && !mouseOverUI) ? max : 0f;
                    light.intensity = Mathf.Lerp(light.intensity, target, Time.deltaTime * lightFadeSpeed);
                }
            }
        }
        else
        {
            FadeOutAllLights();
        }
    }


    public void tabChangeVisible()
    {
        if (minimap.activeSelf)
            Close();
        else
            StartCoroutine(Open());
    }

    IEnumerator Open()
    {
        if (anim != null) anim.SetBool("isOpen", true);
        yield return new WaitForSeconds(0.4f);

        minimap.SetActive(true);
        mainCamera.SetActive(false);
        cameras[currentCameraIndex].SetActive(true);
        camerasActive = true;
    }

    void Close()
    {
        cameras[currentCameraIndex].SetActive(false);
        mainCamera.SetActive(true);
        minimap.SetActive(false);
        if (anim != null) anim.SetBool("isOpen", false);

        camerasActive = false;
        // гасим все лампы
        TurnOffAllLightsInstant();
    }

    public void ChangeCamera(int index)
    {
        if (index < 0 || index >= cameras.Length) return;

        // выключаем лампы у предыдущей камеры (чтобы не оставались включёнными)
        if (cameraLights != null && currentCameraIndex < cameraLights.Length)
        {
            var oldGroup = cameraLights[currentCameraIndex];
            if (oldGroup.lights != null)
            {
                foreach (var l in oldGroup.lights)
                {
                    if (l != null)
                        l.intensity = 0f;
                }
            }
        }

        // переключаем камеру
        cameras[currentCameraIndex].SetActive(false);
        currentCameraIndex = index;
        cameras[currentCameraIndex].SetActive(true);

        // убеждаемся, что свет новой камеры выключен при старте
        if (cameraLights != null && currentCameraIndex < cameraLights.Length)
        {
            var newGroup = cameraLights[currentCameraIndex];
            if (newGroup.lights != null)
            {
                foreach (var l in newGroup.lights)
                {
                    if (l != null)
                        l.intensity = 0f;
                }
            }
        }
    }

    // ==========================
    // 
    // ==========================

    void FadeOutAllLights()
    {
        if (cameraLights == null) return;
        foreach (var group in cameraLights)
        {
            if (group.lights == null) continue;
            foreach (var l in group.lights)
            {
                if (l != null && l.intensity > 0f)
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
            {
                if (l != null)
                    l.intensity = 0f;
            }
        }
    }

    public int CurrentCameraIndex => camerasActive ? currentCameraIndex : -1;
    public GameObject CurrentCamera => camerasActive ? cameras[currentCameraIndex] : null;
}
