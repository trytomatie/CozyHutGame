using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotificationSlotUI : MonoBehaviour
{
    public Item notificationItem;
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
        text.text = notificationItem.itemName;
        amount.text = ""+ notificationItem.stackSize;
        image.sprite = notificationItem.sprite;
        isAvailable = false;
        setupEvent.Invoke();

        CancelInvoke();
        Invoke("RemoveNotification",notificationOnlineTime);
    }

    public void AddToItem(Item item,float notificationOnlineTime)
    {
        notificationItem.stackSize += item.stackSize;
        amount.text = ""+notificationItem.stackSize;
        addEvent.Invoke();

        CancelInvoke();
        Invoke("RemoveNotification", notificationOnlineTime);
    }

    public void RemoveNotification()
    {
        removeEvent.Invoke();
        isAvailable = true;
        notificationItem = null;
    }

    public bool IsAvailable()
    {
        return isAvailable;
    }
}
