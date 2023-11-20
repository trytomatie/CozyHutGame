using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotificationSlotUI : MonoBehaviour
{
    public Item notificationItem;
    private int stackSize;
    public TextMeshProUGUI text;
    public TextMeshProUGUI amount;
    public Image image;
    public UnityEvent setupEvent;
    public UnityEvent addEvent;
    public UnityEvent removeEvent;
    public bool isAvailable = true;
    public void SetupNotification(Item item,float notificationOnlineTime)
    {
        notificationItem = item;
        stackSize = item.stackSize;
        text.text = notificationItem.itemName;
        amount.text = ""+ stackSize;
        image.sprite = notificationItem.sprite;
        isAvailable = false;
        setupEvent.Invoke();

        CancelInvoke();
        Invoke("RemoveNotification",notificationOnlineTime);
    }

    public void AddToItem(Item item,float notificationOnlineTime)
    {
        stackSize += item.stackSize;
        amount.text = ""+ stackSize;
        addEvent.Invoke();

        CancelInvoke();
        Invoke("RemoveNotification", notificationOnlineTime);
    }

    public void RemoveNotification()
    {
        stackSize = 0;
        removeEvent.Invoke();
        isAvailable = true;
        notificationItem = null;
    }

    public bool IsAvailable()
    {
        return isAvailable;
    }
}
