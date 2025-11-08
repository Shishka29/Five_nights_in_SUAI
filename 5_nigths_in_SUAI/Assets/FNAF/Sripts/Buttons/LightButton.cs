using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightButton : MonoBehaviour
{
    public string lightID = "Left"; // "Left" или "Right"
    public GameObject doorLight;
    Vector3 position;

    // свойство для проверки света
    public bool IsLightOn => doorLight != null && doorLight.activeSelf;

    void Awake()
    {
        position = transform.localPosition;
        doorLight.SetActive(false);
    }

    void OnMouseDown()
    {
        transform.localPosition = position - transform.forward * 0.03f;
        doorLight.SetActive(true);
    }

    void OnMouseUp()
    {
        transform.localPosition = position;
        doorLight.SetActive(false);
    }
}
