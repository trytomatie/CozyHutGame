using Newtonsoft.Json;
using UnityEngine;
using static Item;

[CreateAssetMenu(menuName = "Quests/Quest_MainIsland")]
public class Quest_MainIsland : Quest
{
    public Quest_MainIsland()
    {
        // Setup Quest
        questData = new QuestData();
        questData.questId = 3;
        questData.questName = "Finding new Lands.";
        questData.questProgress = new int[] { 0};
        questData.questProgressCap = new int[] { 1};
    }

    public override bool CheckQuestConditions()
    {
        if (questData.questProgress[0] >= questData.questProgressCap[0])
        {
            return true;
        }
        return false;
    }

    public override string[] GetQuestDescription()
    {
        return new string[]
        {
           $"{questData.questProgress[0]} / {questData.questProgressCap[0]} Main Island Reached",
           $" ",
           $"Reward: - {rewardAmounts[0]} {reward[0].itemName}"
        };
    }

}