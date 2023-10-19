using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ResourceController : NetworkBehaviour
{

    [SerializeField] private MMF_Player damageFeedback;

    public NetworkVariable<int> hp = new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        hp.OnValueChanged += OnDeath;
    }

    protected virtual void OnDeath(int previousValue, int newValue)
    {
        if(newValue <= 0)
        {

        }
    }

    [ServerRpc (RequireOwnership =false)]
    public void PlayFeedbackServerRpc(int rnd)
    {
        damageFeedback.StopFeedbacks();
        MMF_FloatingText floatingText = damageFeedback.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = "+"+ rnd + " Wood";
        damageFeedback.PlayFeedbacks();
    }
}
