using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VFXController : NetworkBehaviour
{
    public GameObject[] vfx;

    public PlayerController pc;
    // Start is called before the first frame update
    void Start()
    {
           
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerVFX(int i)
    {
        Instantiate(vfx[i],vfx[i].transform.position,vfx[i].transform.rotation);
        if(IsOwner)
        {
            pc.SpawnHitBox(transform.position + transform.forward * 1, 3);
        }

    }

    public void TriggerVFX(int i,float size)
    {
        Instantiate(vfx[i], vfx[i].transform.position, vfx[i].transform.rotation);
        SpawnVFXServerRpc(i);
        if (IsOwner)
        {
            pc.SpawnHitBox(transform.position + transform.forward * 1, size);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnVFXServerRpc(int i)
    {
        SpawnVFXClientRpc(i);
    }

    [ClientRpc]
    public void SpawnVFXClientRpc(int i)
    {
        if (IsOwner) 
            return;
        Instantiate(vfx[i], vfx[i].transform.position, vfx[i].transform.rotation);
    }
}
