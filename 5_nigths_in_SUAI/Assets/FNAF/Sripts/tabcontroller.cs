using System.Collections;
using UnityEngine;

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

    // новый флаг — true когда планшет открыт и камеры активны
    private bool camerasActive = false;
    public bool CamerasActive => camerasActive;

    void Awake()
    {
        Instance = this;
        if (Tablet != null) anim = Tablet.GetComponent<Animator>();
    }

    public void tabChangeVisible()
    {
        if (minimap.activeSelf)
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
        if (anim != null) anim.SetBool("isOpen", true);
        yield return new WaitForSeconds(0.4f);

        minimap.SetActive(true);
        mainCamera.SetActive(false);
        // включаем выбранную камеру
        cameras[currentCameraIndex].SetActive(true);
        camerasActive = true;
    }

    void Close()
    {
        // выключаем текущую камеру и возвращаем основной рендер
        cameras[currentCameraIndex].SetActive(false);
        mainCamera.SetActive(true);
        minimap.SetActive(false);
        if (anim != null) anim.SetBool("isOpen", false);

        camerasActive = false;
    }

    public void ChangeCamera(int index)
    {
        if (index < 0 || index >= cameras.Length) return;

        // если камеры ещё не активны (например, вызвано извне), включим их
        if (!camerasActive)
        {
            // аналог открытия (без анимации)
            minimap.SetActive(true);
            mainCamera.SetActive(false);
            camerasActive = true;
        }

        cameras[currentCameraIndex].SetActive(false);
        currentCameraIndex = index;
        cameras[currentCameraIndex].SetActive(true);
    }

    // Возвращаем индекс активной камеры, или -1 если камеры не активны
    public int CurrentCameraIndex => camerasActive ? currentCameraIndex : -1;

    // Удобный геттер для GameObject (null если нет)
    public GameObject CurrentCamera => camerasActive ? cameras[currentCameraIndex] : null;
}
