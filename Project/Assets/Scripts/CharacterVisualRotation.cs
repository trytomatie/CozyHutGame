using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CharacterVisualRotation : MonoBehaviour, IDragHandler
{
    public float speed = 3;
    public float timerDelay = 2;
    private float timer = 0;
    private float currentSpeed = 0;
    private float lastMouseX = 0;
    public Transform[] targets;
    private void Update()
    {
        // Decelleration
        if(timer > 0)
        {
            currentSpeed = Mathf.Lerp(0, currentSpeed, timer / timerDelay);
            float rotX =  currentSpeed * speed * Mathf.Deg2Rad;
            foreach (Transform target in targets)
            {
                target.Rotate(Vector3.up, -rotX);
            }
            timer -= Time.deltaTime;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (timer < timerDelay - 0.2f)
        {
            lastMouseX = Input.mousePosition.x;
        }
        // Mouse X Delta direction
        float mouseDeltaX = Input.mousePosition.x - lastMouseX;
        lastMouseX = Input.mousePosition.x;
        currentSpeed = mouseDeltaX;
        timer = timerDelay;
    }
}
