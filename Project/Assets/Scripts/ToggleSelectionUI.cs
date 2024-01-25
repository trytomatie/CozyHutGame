using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleSelectionUI : MonoBehaviour
{
    public UnityEvent onToggleOn;

   
    private void OnEnable()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(ToggleOn);
        CheckToggleSelection();

    }

    public void CheckToggleSelection()
    {
        Toggle toggle = GetComponent<Toggle>();
        Animator anim = toggle.GetComponent<Animator>();
        if(toggle.isOn)
        {
            anim.SetBool("Selected", true);
        }
        else
        {
            anim.SetBool("Selected", false);
            anim.SetTrigger("Normal");
        }
    }
    // Just there to not break old Unity Events
    public void CheckToggleSelection(Toggle toggle)
    {
        CheckToggleSelection();
    }

    public virtual void ToggleOn(bool value)
    {
        if (value)
        {
            onToggleOn.Invoke();
        }
    }
}
