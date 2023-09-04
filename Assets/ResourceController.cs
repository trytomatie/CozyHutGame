using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ResourceController : NetworkBehaviour
{

    [SerializeField] private MMF_Player damageFeedback; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ClientRpc]
    public void PlayFeedbackClientRpc()
    {
        damageFeedback.StopFeedbacks();
        MMF_FloatingText floatingText = damageFeedback.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = "+"+Random.Range(12, 24) + " Wood";
        damageFeedback.PlayFeedbacks();
    }
}
