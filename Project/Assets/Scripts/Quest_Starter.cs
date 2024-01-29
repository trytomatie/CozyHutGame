using Newtonsoft.Json;
using UnityEngine;
using static Item;

[CreateAssetMenu(menuName = "Quests/Quest_Starter")]
public class Quest_Starter : Quest
{
    public Quest_Starter()
    {
        // Setup Quest
        questData = new QuestData();
        questData.questId = 0;
        questData.questName = "STARting out";
        questData.questProgress = new Vector2Int[] 
        { 
            new Vector2Int(0, 3), // Sticks
            new Vector2Int(0, 3)  // Stones
        };
    }

    public override bool CheckQuestConditions()
    {
        // Check if quest is completed by checking if the player has the required items
        questData.questProgress[0].x = GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().inventory.GetAmmountOfItem(condtionItems[0].itemId);
        questData.questProgress[1].x = GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().inventory.GetAmmountOfItem(condtionItems[1].itemId);
        if (questData.questProgress[0].x >= questData.questProgress[0].y && questData.questProgress[1].x >= questData.questProgress[1].y)
        {
            return true;
        }
        return false;
    }

    public override void CompleteQuest()
    {
        Debug.Log("Quest Completed");
    }

    public override string[] GetQuestDescription()
    {
        string[] result = new string[]
        {
            $"{questData.questProgress[0].x} / {questData.questProgress[0].y} Sticks Collected",
            $"{questData.questProgress[1].x} / {questData.questProgress[1].y} Stones Collected",
            $" ",
            $"Reward: - {rewardAmounts[0]} {reward[0].itemName}"
        };
        return result;
    }

}