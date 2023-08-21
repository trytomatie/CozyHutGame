using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionConverter : MonoBehaviour
{
    public PlayerController pc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position != Vector3.zero)
        {
            print(transform.localPosition);
            pc.Move(transform.localPosition);
            transform.localPosition = Vector3.zero;
        }
    }
}
