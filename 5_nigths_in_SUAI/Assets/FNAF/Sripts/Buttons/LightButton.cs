using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Battery;

public class LightButton : MonoBehaviour
{
    public string lightID = "Left";
    public GameObject doorLight;
    Vector3 position;
    public Battery energy;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    public bool IsLightOn => doorLight != null && doorLight.activeSelf;

    void Awake()
    {
        position = transform.localPosition;
        doorLight.SetActive(false);
    }

    private void Update()
    {
        if (energy.energy <= 0)
        {
            doorLight.SetActive(false);
        }
    }

    public void ForceOff()
    {
        if (doorLight != null)
            doorLight.SetActive(false);
    }

    void OnMouseDown()
    {
        transform.localPosition = position - transform.forward * 0.03f;

        // 🔊 звук нажатия
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        if (energy.energy > 0)
        {
            doorLight.SetActive(true);
        }
    }

    void OnMouseUp()
    {
        transform.localPosition = position;
        doorLight.SetActive(false);
    }
}