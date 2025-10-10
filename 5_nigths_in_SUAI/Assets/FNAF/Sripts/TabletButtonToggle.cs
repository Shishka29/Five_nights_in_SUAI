using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TabletButtonToggle : MonoBehaviour, IPointerEnterHandler
{
    public tabcontroller tabletController;


    private bool isAnimating = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isAnimating)
        {
            StartCoroutine(ToggleTablet());
        }
    }

    private IEnumerator ToggleTablet()
    {
        isAnimating = true;

        // �������� ������������ (�������� ��� ��������)
        tabletController.tabChangeVisible();


        // ���, ���� �������� ����������� (0.4 ��� � ���� �������)
        yield return new WaitForSeconds(0.5f);

        isAnimating = false;
    }
}
