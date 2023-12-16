using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;

    [SerializeField] private float yMaxSpeed;
    [SerializeField] private float xMaxSpeed;

    public virtual void DisableCameraControlls(bool value)
    {
        if(value && freeLookCamera.m_YAxis.m_MaxSpeed > 0)
        {
            yMaxSpeed = freeLookCamera.m_YAxis.m_MaxSpeed;
            xMaxSpeed = freeLookCamera.m_XAxis.m_MaxSpeed;
            freeLookCamera.m_XAxis.m_MaxSpeed = 0;
            freeLookCamera.m_YAxis.m_MaxSpeed = 0;
        }
        else
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = xMaxSpeed;
            freeLookCamera.m_YAxis.m_MaxSpeed = yMaxSpeed;
        }

    }

    private void OnDisable()
    {
        freeLookCamera.m_XAxis.m_MaxSpeed = xMaxSpeed;
        freeLookCamera.m_YAxis.m_MaxSpeed = yMaxSpeed;
    }

}
