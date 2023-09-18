using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public TextMeshProUGUI woodText;
    public Canvas canvas;


    private void Awake()
    {
        GameManager.Instance.gameUI = this;
        woodText.text = "0 Wood";

    }

}
