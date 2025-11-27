using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class herosqript : MonoBehaviour
{
    private bool canLook = true;

    public tabcontroller tabletController;
    public float sensitivy = 375f;
    float rotateZone = Screen.width / 5;

    void Update()
    {
        if (!canLook) return; // если заблокировано, не вращаемс€

        if (tabletController != null && tabletController.minimap.activeSelf)
        {
            // ѕланшет открыт Ч не вращаем голову
            return;
        }

        if (Input.mousePosition.x < rotateZone && transform.rotation.eulerAngles.y > 135)
        {
            transform.Rotate(0, -sensitivy * Time.deltaTime, 0);
        }
        if (Input.mousePosition.x > Screen.width - rotateZone && transform.rotation.eulerAngles.y < 225)
        {
            transform.Rotate(0, sensitivy * Time.deltaTime, 0);
        }
    }

    public void BlockLook(bool state)
    {
        canLook = !state; // true = блокируем
    }

}
