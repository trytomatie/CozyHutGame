using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using static Item;

[CreateAssetMenu(menuName = "Quests/Quest_CutTree")]
public class Quest_CutTree : Quest
{
    public Quest_CutTree()
    {
        // Setup Quest
        questData = new QuestData();
        questData.questId = 1;
        questData.questName = "Getting Wood!";
        questData.questProgress = new int[] { 0, 0 };
        questData.questProgressCap = new int[] { 1, 1 };
    }

    public override bool CheckQuestConditions() 
    {

        if (GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().equipmentInventory.GetAmmountOfItem(condtionItems[0].itemId) > 0) 
        {
            questData.questProgress[0] = 1;
        }

        questData.questProgress[1] = StatisticsAPI.GetResourceStatistic_ResourceDestroyed(new ulong[] { 2, 3,4 });

        if (questData.questProgress[1] >= questData.questProgressCap[1])
        {
            return true;
        }
        return false;
    }

    public override string[] GetQuestDescription()
    {
        string[] result = new string[]
        {
           $"{questData.questProgress[0]} / {questData.questProgressCap[0]} Equip the Axe",
           $"{questData.questProgress[1]} / {questData.questProgressCap[1]} Trees Felled",
           $" ",
           $"Reward: - {rewardAmounts[0]} {reward[0].itemName}"
        };
        return result;

    }

}