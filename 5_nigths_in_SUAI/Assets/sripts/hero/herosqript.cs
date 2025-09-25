using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class herosqript : MonoBehaviour
{
    public float sensitivy = 150f;
    float rotateZone = Screen.width / 5;

    void Update()
    {
        if (Input.mousePosition.x < rotateZone && transform.rotation.eulerAngles.y > 45)
        {
            transform.Rotate(0, -sensitivy * Time.deltaTime, 0);
        }
        if (Input.mousePosition.x > Screen.width - rotateZone && transform.rotation.eulerAngles.y < 135)
        {
            transform.Rotate(0, sensitivy * Time.deltaTime, 0);
        }
    }
}
