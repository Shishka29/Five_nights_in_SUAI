using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Battery : MonoBehaviour
{
    [Header("Battery Settings")]
    public float energy = 100f;
    public float baseDischarge = 0.2f;  // базовая трата энергии

    [Header("Usage Icon Sprites")]
    public Image usageImage;
    public Sprite usage0;
    public Sprite usage1;
    public Sprite usage2;
    public Sprite usage3;

    [Header("References")]
    public Door leftDoor;
    public Door rightDoor;
    public LightButton leftLight;
    public LightButton rightLight;
    public tabcontroller tablet;

    [Header("Energy Costs")]
    public float leftDoorCost = 0.2f;
    public float rightDoorCost = 0.2f;
    public float leftLightCost = 0.1f;
    public float rightLightCost = 0.1f;
    public float camLightCost = 0.15f;
    public float tabletCost = 0.1f;

    private float currentCost;
    private int currentUsage;

    private void Awake()
    {
        InvokeRepeating(nameof(ApplyBatteryDrain), 1f, 1f);
    }

    private void Update()
    {
        CalculateUsage();
        UpdateUsageSprite();
    }

    private void CalculateUsage()
    {
        currentUsage = 0;
        currentCost = baseDischarge;

        // --- ДВЕРИ ---
        if (leftDoor && !leftDoor.isOpen)
        {
            currentUsage++;
            currentCost += leftDoorCost;
        }

        if (rightDoor && !rightDoor.isOpen)
        {
            currentUsage++;
            currentCost += rightDoorCost;
        }

        // --- ДВЕРНЫЕ ФОНАРИ ---
        if (leftLight && leftLight.doorLight && leftLight.doorLight.activeSelf)
        {
            currentUsage++;
            currentCost += leftLightCost;
        }

        if (rightLight && rightLight.doorLight && rightLight.doorLight.activeSelf)
        {
            currentUsage++;
            currentCost += rightLightCost;
        }

        // --- КАМЕРНЫЙ СВЕТ ---
        bool camLightActive =
            tablet != null &&
            tablet.minimap.activeSelf &&               // планшет открыт
            Input.GetMouseButton(0) &&                // зажата ЛКМ
            !EventSystem.current.IsPointerOverGameObject(); // не по UI

        if (camLightActive)
        {
            currentUsage++;
            currentCost += camLightCost;
        }

        // --- ПЛАНШЕТ ---
        if (tablet != null && tablet.minimap.activeSelf)
        {
            currentUsage++;
            currentCost += tabletCost;
        }
    }

    private void ApplyBatteryDrain()
    {
        energy -= currentCost;
        if (energy < 0) energy = 0;
        if (energy > 100) energy = 100;
    }

    private void UpdateUsageSprite()
    {
        if (usageImage == null) return;

        switch (currentUsage)
        {
            case 0:
                usageImage.sprite = usage0;
                break;
            case 1:
                usageImage.sprite = usage1;
                break;
            case 2:
                usageImage.sprite = usage2;
                break;
            default:
                usageImage.sprite = usage3; // 3+
                break;
        }
    }
}
