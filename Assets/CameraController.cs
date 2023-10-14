using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private Transform followTarget;
    private Vector3 offset;
    private float rotationX;

    public CinemachineVirtualCamera[] cinemachineCameras;

    private void Start()
    {
        cinemachineCameras = FindObjectsOfType<CinemachineVirtualCamera>();
        followTarget = transform.parent;
        offset = transform.localPosition;
        transform.parent = null;
        InputManager.Instance.CameraZoomDeltaPerformed += ScrollCamera;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = followTarget.transform.position + offset;
        
        // Apply rotation based on mouse delta
        transform.Rotate(Vector3.up, InputManager.Instance.cameraMovement.x * Options.Instance.mouseRotationSpeed, Space.World);

        rotationX -= InputManager.Instance.cameraMovement.y * Options.Instance.mouseRotationSpeed;
        rotationX = Mathf.Clamp(rotationX, -90, 90);

        transform.localRotation = Quaternion.Euler(rotationX, transform.localRotation.eulerAngles.y, 0);
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, -90, 90);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    public void ScrollCamera(object sender, float value)
    {
        foreach(CinemachineVirtualCamera c in cinemachineCameras)
        {
            Cinemachine3rdPersonFollow composer = c.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if(composer != null)
            {
                composer.CameraDistance += -value * Options.Instance.mouseScrollSpeed * 0.01f;
            }

        }
    }
}
