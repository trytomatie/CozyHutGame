using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ResourceController : NetworkBehaviour
{

    [SerializeField] private MMF_Player damageFeedback;
    public GameObject stump;
    public GameObject brokenTree;
    public GameObject intactTree;

    public NetworkVariable<int> hp = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    // Start is called before the first frame update
    void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        hp.OnValueChanged += OnDeath;
    }

    void OnDeath(int previousValue, int newValue)
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

    [ClientRpc]
    public void PlayFeedbackClientRpc(int rnd)
    {
        damageFeedback.StopFeedbacks();
        MMF_FloatingText floatingText = damageFeedback.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = "+"+ rnd + " Wood";
        damageFeedback.PlayFeedbacks();
    }
}
