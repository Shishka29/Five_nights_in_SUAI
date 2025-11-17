using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static tabcontroller;

public class Battery : MonoBehaviour
{
    public float energy = 100;
    public float discharge = 0.2f;

    public GameObject[] segments;
    public tabcontroller tablet;
    public Door door1;
    public Door door2;
    public LightButton light1;
    public LightButton light2;

    private void Awake()
    {

        InvokeRepeating("Discharging", 1f, 1f);
    }
       
    private void Update()
    {
        SetDischarge();
        ViewEnergy();
    }

    private void Discharging()
    {
        energy -= discharge;
    }

    private void ViewEnergy()
    {
        if (energy < 75)
            segments[3].SetActive(false);
        if (energy < 50)
            segments[2].SetActive(false);
        if (energy < 25)
            segments[1].SetActive(false);
        if (energy < 0)
            segments[0].SetActive(false);
    }

    private void SetDischarge()
    {
        float tabletDS = 0f;
        float doorsDS = 0f;
        float lightDS = 0f;

        // Безопасная проверка для планшета
        if (tablet != null && tablet.minimap != null)
        {
            tabletDS = tablet.minimap.activeSelf ? 0.1f : 0f;
        }

        // Безопасная проверка для дверей
        if (door1 != null && door2 != null)
        {
            if (!door1.isOpen && !door2.isOpen)
                doorsDS = 0.2f;
            else if (!door1.isOpen || !door2.isOpen) // Одна дверь закрыта
                doorsDS = 0.1f;
        }

        // Безопасная проверка для фонарей
        bool isLight1On = light1 != null && light1.doorLight != null && light1.doorLight.activeSelf;
        bool isLight2On = light2 != null && light2.doorLight != null && light2.doorLight.activeSelf;

        lightDS = (isLight1On || isLight2On) ? 0.1f : 0f;

        discharge = 0.2f + tabletDS + doorsDS + lightDS;
    }

}
