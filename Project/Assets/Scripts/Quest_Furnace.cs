using Newtonsoft.Json;
using UnityEngine;
using static Item;

[CreateAssetMenu(menuName = "Quests/Quest_Furnace")]
public class Quest_Furnace : Quest
{
    public Quest_Furnace()
    {
        // Setup Quest
        questData = new QuestData();
        questData.questId = 4;
        questData.questName = "They are Minerals!";
        questData.questProgress = new Vector2Int[] 
        { 
            new Vector2Int(0, 1), // Built Furnace
            new Vector2Int(0, 1), // Interacted with Furnace
        };
    }

    public override bool CheckQuestConditions()
    {
        if(StatisticsAPI.GetBuildingsStatistic_BuildingsBuilt(12) >= 1)
        {
            questData.questProgress[0].x = 1;
        }   
        if(GameUI.Instance.GetComponent<Animator>().GetInteger("Ui_State") == 5) 
        {
            questData.questProgress[1].x = 1;
        }
        if (questData.questProgress[1].x >= questData.questProgress[1].y)
        {
            return true;
        }
        return false;
    }

    public override string[] GetQuestDescription()
    {
        return new string[]
        {
           $"{questData.questProgress[0].x} / {questData.questProgress[0].y} Build furnace",
           $"{questData.questProgress[1].x} / {questData.questProgress[1].y} Interact with furnace"
        };
    }

}