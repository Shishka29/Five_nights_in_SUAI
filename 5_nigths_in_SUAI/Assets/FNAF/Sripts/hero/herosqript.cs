using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class herosqript : MonoBehaviour
{
    public tabcontroller tabletController;
    public float sensitivy = 375f;
    float rotateZone = Screen.width / 5;

    void Update()
    {
        if (tabletController != null && tabletController.minimap.activeSelf)
        {
            // ѕланшет открыт Ч не вращаем голову
            return;
        }

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
