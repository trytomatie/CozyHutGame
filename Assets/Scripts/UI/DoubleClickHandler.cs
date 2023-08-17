using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
{
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f; // Adjust as needed
    public UnityEvent doubleClickEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - lastClickTime <= doubleClickThreshold)
        {
            // Double click detected
            doubleClickEvent.Invoke();

            // Perform double-click action here
        }
        else
        {
            // Single click
            lastClickTime = Time.time;
        }
    }
}