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
        questData.questProgress = new Vector2Int[] 
        { 
            new Vector2Int(0, 1), // Equip Axe
            new Vector2Int(0, 1), // Cut down Tree
        };
    }

    public override bool CheckQuestConditions()
    {

        if (GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().equipmentInventory.GetAmmountOfItem(condtionItems[0].itemId) > 0) 
        {
            questData.questProgress[0].x = 1;
        }

        questData.questProgress[1].x = StatisticsAPI.GetResourceStatistic_ResourceDestroyed(new ulong[] { 2, 3,4 });

        if (questData.questProgress[1].x >= questData.questProgress[1].y)
        {
            return true;
        }
        return false;
    }

    public override string[] GetQuestDescription()
    {
        string[] result = new string[]
        {
           $"{questData.questProgress[0].x} / {questData.questProgress[0].y} Equip the Axe",
           $"{questData.questProgress[1].x} / {questData.questProgress[1].y} Trees Felled",
           $" ",
           $"Reward: - {rewardAmounts[0]} {reward[0].itemName}"
        };
        return result;

    }

}