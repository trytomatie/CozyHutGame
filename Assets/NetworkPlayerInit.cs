using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerInit : NetworkBehaviour
{
    public MonoBehaviour[] componentsToDisable;
    public GameObject[] objectsToDelete;
    public GameObject[] objectsToActivate;
    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner)
        {
            foreach(MonoBehaviour component in componentsToDisable)
            {
                component.enabled = false;
            }

            foreach(GameObject go in objectsToDelete)
            {
                Destroy(go);
            }
        }
        else
        {
            foreach (GameObject go in objectsToActivate)
            {
                go.SetActive(true);
            }
        }
    }

    private void Update()
    {

    }

    [ClientRpc]
    public void TeleportClientRpc(Vector3 postion, ClientRpcParams clientRpcParams = default)
    {
        transform.position = postion;
    }


}
