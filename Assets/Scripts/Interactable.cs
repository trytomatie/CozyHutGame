using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    public float heightOffset = 0.5f;
    public string interactionText = "Interact";
    public virtual void Interact(GameObject source)
    {

    }
}
