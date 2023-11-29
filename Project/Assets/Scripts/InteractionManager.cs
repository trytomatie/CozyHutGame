using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public List<Interactable> interactablesInRange = new List<Interactable>();
    public Interactable currentInteractionTarget;
    public float minInteractionDistance = 3;

    public event Action<Interactable> InteractableSet;
    public event Action<Interactable> InteractableLost;

    private static InteractionManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        CurrentInteractionTarget = CheckInteractableInRange();
    }

    public Interactable CheckInteractableInRange()
    {
        float closestDistance = float.MaxValue;
        float currentDistance;
        Interactable closestInteractable = null;
        foreach(Interactable interactable in interactablesInRange)
        {
            currentDistance = Vector3.Distance(interactable.transform.position, transform.position);
            if (currentDistance < closestDistance && currentDistance > minInteractionDistance)
            {
                closestInteractable = interactable;
                closestDistance = currentDistance;
            }
        }
        return closestInteractable;
    }

    public void Interact(GameObject soruce)
    {
        if(CurrentInteractionTarget != null )
        {
            print("Hey");
            CurrentInteractionTarget.Interact();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Interactable>() != null)
        {
            interactablesInRange.Add(other.GetComponent<Interactable>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Interactable>() != null && interactablesInRange.Contains(other.GetComponent<Interactable>()))
        {
            interactablesInRange.Remove(other.GetComponent<Interactable>());
        }
    }

    public Interactable CurrentInteractionTarget
    {
        get => currentInteractionTarget;
        set
        { 
            if(value != currentInteractionTarget)
            {
                if(value != null)
                {
                    currentInteractionTarget = value;
                    InteractableSet.Invoke(currentInteractionTarget);
                }
                else
                {
                    currentInteractionTarget = value;
                    InteractableLost.Invoke(currentInteractionTarget);
                }

            }

    
        }
    }

    public static InteractionManager Instance
    {
        get => instance;
    }
}
