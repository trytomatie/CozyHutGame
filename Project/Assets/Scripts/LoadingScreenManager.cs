using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    public CanvasGroup loadingScreen;
    public CanvasGroup connectionLoadingScreen;

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
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.blocksRaycasts = true;
        loadingScreen.GetComponent<Animator>().SetInteger("Show", 1);
        Invoke("DismissLoadingScreen", 40);
    }

    public void DismissLoadingScreen()
    {
        loadingScreen.gameObject.SetActive(false);
        loadingScreen.blocksRaycasts = false;
        loadingScreen.GetComponent<Animator>().SetInteger("Show", 0);
    }

    public void CallConnectionLoadingScreen()
    {
        connectionLoadingScreen.gameObject.SetActive(true);
        connectionLoadingScreen.blocksRaycasts = true;
        connectionLoadingScreen.GetComponent<Animator>().SetInteger("Show", 1);
    }

    public void DismissConnectionLoadingScreen()
    {
        connectionLoadingScreen.gameObject.SetActive(false);
        connectionLoadingScreen.blocksRaycasts = false;
        connectionLoadingScreen.GetComponent<Animator>().SetInteger("Show", 0);
    }
}
