using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class tabcontroller : MonoBehaviour
{
    public static tabcontroller Instance;

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

    [Header("Camera Lights")] // <-- Header только над полем
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

        TurnOffAllLightsInstant();
    }

    void Update()
    {
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

        for (int i = 0; i < group.lights.Length; i++)
        {
            var light = group.lights[i];
            if (light == null) continue;

            float max = (group.maxIntensities != null && i < group.maxIntensities.Length)
                ? group.maxIntensities[i]
                : 3f;

            float target = (Input.GetMouseButton(0) && !mouseOverUI) ? max : 0f;
            light.intensity = Mathf.Lerp(light.intensity, target, Time.deltaTime * lightFadeSpeed);
        }
    }

    public void tabChangeVisible()
    {
        if (minimap != null && minimap.activeSelf)
            Close();
        else
            StartCoroutine(Open());
    }

    IEnumerator Open()
    {
        if (anim != null) anim.SetBool("isOpen", true);
        yield return new WaitForSeconds(0.4f);

        if (minimap != null) minimap.SetActive(true);
        if (mainCamera != null) mainCamera.SetActive(false);

        if (cameras != null && cameras.Length > 0)
            cameras[currentCameraIndex].SetActive(true);

        camerasActive = true;
    }

    void Close()
    {
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
        if (cameras == null || index < 0 || index >= cameras.Length) return;

        cameras[currentCameraIndex].SetActive(false);
        TurnOffLights(currentCameraIndex);

        currentCameraIndex = index;

        cameras[currentCameraIndex].SetActive(true);
        TurnOffLights(currentCameraIndex);
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
