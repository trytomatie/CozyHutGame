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
        questData.questName = "Just Have Fun!";
        questData.questProgress = new int[] { 0, 0 };
        questData.questProgressCap = new int[] { 1, 3 };
    }

    public override bool CheckQuestConditions()
    {
        return false;
        if(StatisticsAPI.GetBuildingsStatistic_BuildingsBuilt(12) >= 1)
        {
            questData.questProgress[0] = 1;
        }   
        if(GameUI.Instance.GetComponent<Animator>().GetInteger("Ui_State") == 5) 
        {
            questData.questProgress[1] = 1;
        }
        if (questData.questProgress[1] >= questData.questProgressCap[1])
        {
            return true;
        }
        return false;
    }

    public override string[] GetQuestDescription()
    {
        return new string[]
        {
           $"1/1 Have fun, that's it!",
        };
    }

}