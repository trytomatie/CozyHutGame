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
        questData.questProgress = new int[] { 0, 0 };
        questData.questProgressCap = new int[] { 3, 3 };
    }

    public override bool CheckQuestConditions()
    {
        // Check if quest is completed by checking if the player has the required items
        questData.questProgress[0] = GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().inventory.GetAmmountOfItem(condtionItems[0].itemId);
        questData.questProgress[1] = GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().inventory.GetAmmountOfItem(condtionItems[1].itemId);
        if (questData.questProgress[0] >= questData.questProgressCap[0] && questData.questProgress[1] >= questData.questProgressCap[1])
        {
            return true;
        }
        return false;
    }

    public override string[] GetQuestDescription()
    {
        string[] result = new string[]
        {
            $"{questData.questProgress[0]} / {questData.questProgressCap[0]} Sticks Collected",
            $"{questData.questProgress[1]} / {questData.questProgressCap[1]} Stones Collected",
            $" ",
            $"Reward: - {rewardAmounts[0]} {reward[0].itemName}"
        };
        return result;
    }

}