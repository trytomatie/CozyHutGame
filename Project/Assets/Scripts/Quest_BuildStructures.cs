using Newtonsoft.Json;
using UnityEngine;
using static Item;

[CreateAssetMenu(menuName = "Quests/Quest_BuildStructures")]
public class Quest_BuildStructures : Quest
{
    public Quest_BuildStructures()
    {
        // Setup Quest
        questData = new QuestData();
        questData.questId = 2;
        questData.questName = "Time to Build";
        questData.questProgress = new Vector2Int[] 
        { 
            new Vector2Int(0, 1), // Open The Buildmenu
            new Vector2Int(0, 3), // Build 3 Structures
        };
    }

    public override bool CheckQuestConditions()
    {

        if(GameUI.Instance.GetComponent<Animator>().GetInteger("Ui_State") == 3) 
        {
            questData.questProgress[0].x = 1;
        }

        questData.questProgress[1].x = Mathf.Clamp(StatisticsAPI.GetBuildingStatistic_TotalBuildingsBuilt(),0,3);

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
           $"{questData.questProgress[0].x} / {questData.questProgress[0].y} Equip the Hammer and Open the Building Menu",
           $"{questData.questProgress[1].x} / {questData.questProgress[1].y} Structures Built",
           $" ",
           $"Reward: - {rewardAmounts[0]} {reward[0].itemName}"
        };
        return result;

    }

}