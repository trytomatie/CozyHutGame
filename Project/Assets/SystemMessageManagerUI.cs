using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SystemMessageManagerUI : MonoBehaviour
{
    public TextMeshProUGUI systemMessageText;
    public Animator systemMessageAnimator;
    private uint messageCounterPointer = 0;
    private uint messageCounter = 0;
    private bool isShowingMessage;

    // Singleton
    public static SystemMessageManagerUI instance;
    // Start is called before the first frame update
    void Start()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        
    }

    public static void ShowSystemMessage(string message)
    {
        instance.ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        StartCoroutine(ShowMessage(message, instance.messageCounter));
        instance.messageCounter++;
    }

    private IEnumerator ShowMessage(string message,uint messageCounter)
    {
        while (messageCounterPointer != messageCounter)
        {
            yield return new WaitForSeconds(0.5f);
        }
        isShowingMessage = true;
        systemMessageText.text = message;
        systemMessageAnimator.SetBool("Show", true);
        yield return new WaitForSeconds(3);
        systemMessageAnimator.SetBool("Show", false);
        isShowingMessage = false;
        messageCounterPointer++;
    }

    
}
