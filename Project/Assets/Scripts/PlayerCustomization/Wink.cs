using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wink : MonoBehaviour
{
    public Material eyes;
    public float originalValue;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("CloseEye", 0, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseEye()
    {
        originalValue = eyes.GetVector("_Offset_Eyes").y;
        eyes.SetVector("_Offset_Eyes", new Vector2(0, 0.315f));

        Invoke("OpenEye", 0.2f);
    }

    public void OpenEye()
    {
        eyes.SetVector("_Offset_Eyes", new Vector2(0, originalValue));
    }
}
