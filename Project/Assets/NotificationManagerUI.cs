using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManagerUI : MonoBehaviour
{
    public NotificationSlotUI[] notifications;

    private float notificationTimer = 2.5f;

    private static NotificationManagerUI instance;

    public void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }


    public void SetNotification(Item item)
    {
        foreach (NotificationSlotUI notifcation in notifications)
        {
            if (notifcation.notificationItem != null && notifcation.notificationItem.itemId == item.itemId)
            {
                notifcation.AddToItem(item, notificationTimer);
                return;
            }
        }
        foreach (NotificationSlotUI notifcation in notifications)
        {
            if (notifcation.IsAvailable())
            {
                notifcation.SetupNotification(item, notificationTimer);
                return;
            }
        }
    }

    public static NotificationManagerUI Instance { get => instance; }
}
