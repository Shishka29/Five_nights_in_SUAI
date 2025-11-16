using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        float tabletDS;
        float doorsDS;
        float lightDS;

        if (tablet.minimap.activeSelf)
            tabletDS = 0.1f;
        else
            tabletDS = 0f;

        if (door1.isOpen == false && door2.isOpen == false)
            doorsDS = 0.2f;
        else if (door1.isOpen == false && door2.isOpen == true)
            doorsDS = 0.1f;
        else if (door1.isOpen == true && door2.isOpen == false)
            doorsDS = 0.1f;
        else 
            doorsDS = 0f;

        if (light1.doorLight.activeSelf == true || light2.doorLight.activeSelf == true)
            lightDS = 0.1f;
        else
            lightDS = 0f;

        discharge = 0.2f + tabletDS + doorsDS + lightDS;
    }

}
