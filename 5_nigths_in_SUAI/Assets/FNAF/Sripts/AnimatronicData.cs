using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimatronicData", menuName = "FNAF/Animatronic Data", order = 1)]
[System.Serializable]
public class AnimatronicData : ScriptableObject
{
    public string animatronicName;
    public GameObject animatronicObject;
    [Range(1, 20)] public int initialDifficulty = 10;
    [HideInInspector] public bool isActive = false;

    public void SetDifficulty(int difficulty)
    {
        initialDifficulty = Mathf.Clamp(difficulty, 1, 20);
    }
}

