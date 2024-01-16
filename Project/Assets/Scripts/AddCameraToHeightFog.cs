using AtmosphericHeightFog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HeightFogOverride))]
public class AddCameraToHeightFog : MonoBehaviour
{
    private HeightFogOverride heightFog;
    // Start is called before the first frame update
    void Start()
    {
        heightFog = GetComponent<HeightFogOverride>();
        heightFog.mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void AddCameraToHeightFogGlobal(GameObject go)
    {

    }
}
