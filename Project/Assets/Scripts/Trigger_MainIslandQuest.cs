using Newtonsoft.Json;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static Item;

public class Trigger_MainIslandQuest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player has entered the trigger
        // If so, update the quest progress
        // And check if the quest is completed
        if (other.gameObject.GetComponent<NetworkPlayerInit>() != null)
        {
            NetworkPlayerInit player = other.gameObject.GetComponent<NetworkPlayerInit>();
            if(player.gameObject == GameManager.GetLocalPlayer())
            {
                Quest quest = QuestManager.Instance.quests[QuestManager.Instance.CurrentQuestIndex];
                bool isMainislandQuest = quest.GetType().Equals(typeof(Quest_MainIsland));
                if (isMainislandQuest)
                {
                    quest.questData.questProgress[0].x = 1;
                    QuestManager.CheckQuestConditions();
                    gameObject.SetActive(false);
                    return;
                }
            }
        }
    }
}
