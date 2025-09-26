using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tabcontroller : MonoBehaviour
{
    public GameObject Tablet;
    public Animator anim;
    public GameObject minimap;
    public GameObject[] cameras;
    public GameObject mainCamera;
    private int currentCameraIndex = 0;

    void Awake()
    {
        anim = Tablet.GetComponent<Animator>();
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
        anim.SetBool("isOpen", true);
        yield return new WaitForSeconds(0.4f);
        minimap.SetActive(true);
        mainCamera.SetActive(false);
        cameras[currentCameraIndex].SetActive(true);
    }

    void Close()
    {
        cameras[currentCameraIndex].SetActive(false);
        mainCamera.SetActive(true);
        minimap.SetActive(false);
        anim.SetBool("isOpen", false);
    }

    public void ChangeCamera(int index)
    {
        cameras[currentCameraIndex].SetActive(false);
        currentCameraIndex = index;
        cameras[currentCameraIndex].SetActive(true);
    }
}
