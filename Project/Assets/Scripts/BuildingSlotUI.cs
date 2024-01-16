using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BuildingSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent<GameObject> hoverEnterEvent;
    public UnityEvent hoverExitEvent;
    public BuildingObject buildingData;
    public UnityEvent<int> buildingSelectionEvent;
    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverEnterEvent.Invoke(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverExitEvent.Invoke();
    }

    public void SelectBuilding()
    {
        buildingSelectionEvent.Invoke((int)buildingData.buildingId);
    }

}
