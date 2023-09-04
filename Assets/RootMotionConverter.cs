using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionConverter : MonoBehaviour
{
    public PlayerController pc;
    private Vector3 pivotWorldPosition;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.localPosition != Vector3.zero)
        {
            Quaternion rotation = Quaternion.Euler(transform.eulerAngles);

            pc.rootMotionMotion = rotation * transform.localPosition;
        }
        transform.localPosition = Vector3.zero;
        pivotWorldPosition = transform.position;
    }
}
