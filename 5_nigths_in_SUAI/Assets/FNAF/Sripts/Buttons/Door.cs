using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 openPosition;
    public Vector3 closePosition;
    public bool isOpen = true;
    public bool action = false;

    public void ButtonPressed()
    {
        isOpen = !isOpen;
        action = true;
    }
    private void Update()
    {
        Vector3 NextPosition;
        if (isOpen)
        {
            NextPosition = openPosition;
        }
        else
        {
            NextPosition = closePosition;
        }
        if (action)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, NextPosition, 12f * Time.deltaTime);
            if (Vector3.Distance(transform.localPosition, NextPosition) < 0.01f)
{
                transform.localPosition = NextPosition;
                action = false;
            }
        }
    }
}
