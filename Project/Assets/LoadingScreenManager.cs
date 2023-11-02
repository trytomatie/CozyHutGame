using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public static LoadingScreenManager Instance { get; protected set; }
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {

            Instance = this;
        }
        DontDestroyOnLoad(this);
    }
    public void CallLoadingScreen()
    {
        CancelInvoke();
        StopAllCoroutines();
        canvasGroup.blocksRaycasts = true;
        StartCoroutine(SetCanvasAlpha1());
        Invoke("DismissLoadingScreen", 40);
    }

    IEnumerator SetCanvasAlpha1()
    {
        while(canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator SetCanvasAlpha0()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    public void DismissLoadingScreen()
    {
        CancelInvoke();
        StopAllCoroutines();
        canvasGroup.blocksRaycasts = false;
        StartCoroutine(SetCanvasAlpha0());
    }
}
