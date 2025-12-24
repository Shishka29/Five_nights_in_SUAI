using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Battery;

public class Door : MonoBehaviour
{
    public Vector3 openPosition;
    public Vector3 closePosition;
    public bool isOpen = true;
    public bool action = false;
    public Battery energy;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private bool soundPlayed = false; // защита от повтора

    public void ButtonPressed()
    {
        if (energy.energy > 0)
        {
            isOpen = !isOpen;
            action = true;
            PlayDoorSound();
        }
    }

    public void ForceOpen()
    {
        isOpen = true;
        action = true;
        PlayDoorSound();
    }

    private void PlayDoorSound()
    {
        if (audioSource == null) return;

        soundPlayed = false;

        if (isOpen && openSound != null)
            audioSource.PlayOneShot(openSound);
        else if (!isOpen && closeSound != null)
            audioSource.PlayOneShot(closeSound);

        soundPlayed = true;
    }

    private void Update()
    {
        Vector3 nextPosition;

        if (energy.energy <= 0)
        {
            if (!isOpen)
            {
                isOpen = true;
                action = true;
                PlayDoorSound();
            }
        }

        nextPosition = isOpen ? openPosition : closePosition;

        if (action)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                nextPosition,
                12f * Time.deltaTime
            );

            if (Vector3.Distance(transform.localPosition, nextPosition) < 0.01f)
            {
                transform.localPosition = nextPosition;
                action = false;
            }
        }
    }
}
