using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private InputManager instance;

    private int currentDevice = 0; // 0 == Keyboard, 1 == Controller


    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public static Vector2 MouseDelta()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * Options.Instance.mouseRotationSpeed);
    }

    public static int CurrentDevice()
    {
        if (Input.anyKey)
            return 0;
        return 1;
    }


    public InputManager Instance { get => instance; }
}
