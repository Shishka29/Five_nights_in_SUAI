using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightButton : MonoBehaviour
{
    public GameObject doorLight;
    Vector3 position;
    void Awake()
    {
        position = transform.localPosition;
    }
    
    void OnMouseDown()
    {
        transform.localPosition = position - transform.forward * -0.03f;
        doorLight.SetActive(true);
    }
    void OnMouseUp()
    {
        transform.localPosition = position;
        doorLight.SetActive(false);
    }
}
