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
        questData.questProgress = new int[] { 0, 0 };
        questData.questProgressCap = new int[] { 1, 3 };
    }

    public override bool CheckQuestConditions()
    {
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
           $"{questData.questProgress[0]} / {questData.questProgressCap[0]} Build furnace",
           $"{questData.questProgress[1]} / {questData.questProgressCap[1]} Interact with furnace"
        };
    }

}