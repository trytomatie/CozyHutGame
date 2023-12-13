using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace EngineTools
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        static readonly string KeyMouseX = "Mouse X";
        static readonly string KeyMouseY = "Mouse Y";

        [SerializeField, Range(1.0f, 40.0f)]
        public float CameraRotatationSentivity = 7.5f, CameraRotatationDelta = 5.0f;

        [SerializeField, Range(1.0f, 40.0f)]
        public float CameraTranslanslationSentivity = 7.5f, CameraTranslationDelta = 5.0f;

        private Camera m_Camera;
        private Quaternion m_LerpRotation;
        private Vector3 m_LerpTranslation;

        void Start()
        {
            m_Camera = GetComponent<Camera>();
            m_LerpRotation = m_Camera.transform.rotation;
            m_LerpTranslation = m_Camera.transform.position;
        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                var mouseX = Input.GetAxis(KeyMouseX) * CameraRotatationSentivity;
                var mouseY = Input.GetAxis(KeyMouseY) * CameraRotatationSentivity;

                var up = transform.InverseTransformDirection(m_Camera.transform.up);
                var right = transform.InverseTransformDirection(m_Camera.transform.right);

                m_LerpRotation *= Quaternion.AngleAxis(-mouseY, right);
                m_LerpRotation *= Quaternion.AngleAxis(+mouseX, up);
            }

            if (Input.GetKey(KeyCode.W))
                m_LerpTranslation += CameraTranslanslationSentivity * Time.deltaTime * m_Camera.transform.forward;

            if (Input.GetKey(KeyCode.S))
                m_LerpTranslation -= CameraTranslanslationSentivity * Time.deltaTime * m_Camera.transform.forward;

            if (Input.GetKey(KeyCode.A))
                m_LerpTranslation -= CameraTranslanslationSentivity * Time.deltaTime * m_Camera.transform.right;

            if (Input.GetKey(KeyCode.D))
                m_LerpTranslation += CameraTranslanslationSentivity * Time.deltaTime * m_Camera.transform.right;

            m_Camera.transform.rotation = Quaternion.Slerp(m_Camera.transform.rotation, m_LerpRotation,
                Time.deltaTime * CameraRotatationDelta);
            m_Camera.transform.position = Vector3.Lerp(m_Camera.transform.position, m_LerpTranslation,
                Time.deltaTime * CameraTranslationDelta);
        }
    }
}
