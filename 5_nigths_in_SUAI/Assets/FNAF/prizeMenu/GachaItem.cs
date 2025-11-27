using UnityEngine;

[System.Serializable]
public class GachaItem
{
    public string id;                // уникальный ID
    public Sprite icon;              // картинка предмета
    [Range(0, 100)] public float dropChance; // шанс выпадения %
}
