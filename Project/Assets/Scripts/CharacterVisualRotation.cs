using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterVisualRotation : MonoBehaviour
{
    public float speed = 3;
    public float timerDelay = 2;
    private float timer = 0;
    private float currentSpeed = 0;
    private float lastMouseX = 0;
    private void Update()
    {
        // Decelleration
        if(timer > 0)
        {
            currentSpeed = Mathf.Lerp(0, currentSpeed, timer / timerDelay);
            float rotX =  currentSpeed * speed * Mathf.Deg2Rad;
            transform.Rotate(Vector3.up, -rotX);
            timer -= Time.deltaTime;
        }
    }

    private void OnMouseDrag()
    {
        if(timer < timerDelay-0.2f)
        {
            print("Mouse Drag");
            lastMouseX = Input.mousePosition.x;
        }
        // Mouse X Delta direction
        float mouseDeltaX = Input.mousePosition.x - lastMouseX;
        lastMouseX = Input.mousePosition.x;
        currentSpeed = mouseDeltaX;
        timer = timerDelay;
    }

}
