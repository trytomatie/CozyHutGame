using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ObserverCursor : NetworkBehaviour
{
    public Image image;
    public TextMeshProUGUI text;
    private RectTransform rectTransform;
    private bool isVisible = false;

    public NetworkVariable<float> mouseX = new NetworkVariable<float>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> mouseY = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public int frame = 0;
    // Start is called before the first frame update
    void Start()
    {
        SetVisible(false);
        rectTransform = GetComponent<RectTransform>();
    }

    public void FixedUpdate()
    {
        print(IsLocalPlayer);
        if (IsLocalPlayer && isVisible && frame % 3 == 0) // Updated 10 times per second max
        {
            mouseX.Value = Input.mousePosition.x / Screen.width;
            mouseY.Value = Input.mousePosition.y / Screen.height;
        }
        frame++;
        if(frame == 30)
        {
            frame = 0;
        }
    }

    public void Update()
    {
        if(isVisible)
        {
            rectTransform.position = Vector2.Lerp(rectTransform.position, new Vector2(mouseX.Value * Screen.width, mouseY.Value * Screen.height), 0.1f);
        }
    }

    public void SetVisible(bool value)
    {
        if(IsLocalPlayer)
        {
            image.enabled = false; // Usualy false
            text.enabled = false;
        }
        else
        {
            image.enabled = value;
            text.enabled = value;
        }

        isVisible = value;
    }

}
