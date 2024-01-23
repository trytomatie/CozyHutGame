using Newtonsoft.Json;
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
        if (other.gameObject.GetComponent<NetworkObject>().NetworkObjectId == NetworkManager.Singleton.LocalClientId)
        {
            Quest_MainIsland quest = QuestManager.Instance.quests[2] as Quest_MainIsland;
            if(QuestManager.Instance.CurrentQuestIndex == 2)
            {
                quest.questData.questProgress[0].x = 1;
                QuestManager.CheckQuestConditions();
                gameObject.SetActive(false);
                return;
            }
            if(QuestManager.Instance.CurrentQuestIndex > 2)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
