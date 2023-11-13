using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationParameterController : MonoBehaviour
{
    public string selectedParameter; 
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetParameter(string value)
    {
        selectedParameter = value;
    }
    public void SetAnimatorBool(bool value)
    {
        anim.SetBool(selectedParameter, value);
    }
    public void SetAnimatorInteger(int value)
    {
        anim.SetInteger(selectedParameter, value);
    }
    public void SetAnimatorFloat(float value)
    {
        anim.SetFloat(selectedParameter, value);
    }
}
