using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ResourceController_Tree : ResourceController
{
    public GameObject stump;
    public GameObject brokenTree;
    public GameObject intactTree;


    protected override void OnDeath(int previousValue, int newValue)
    {
        if(newValue <= 0)
        {
            intactTree.SetActive(false);
            brokenTree.SetActive(true);
            stump.SetActive(true);
            GetComponent<Collider>().enabled = false;
           // brokenTree.GetComponent<Rigidbody>().AddExplosionForce(10,transform.position + new Vector3(0,5,1),3,2,ForceMode.Impulse);
        }
    }

}
