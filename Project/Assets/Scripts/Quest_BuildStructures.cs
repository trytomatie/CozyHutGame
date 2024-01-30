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
        questData.questProgress = new int[] { 0, 0 };
        questData.questProgressCap = new int[] { 1, 3 };
    }

    public override bool CheckQuestConditions()
    {

        if(GameUI.Instance.GetComponent<Animator>().GetInteger("Ui_State") == 3) 
        {
            questData.questProgress[0] = 1;
        }

        questData.questProgress[1] = Mathf.Clamp(StatisticsAPI.GetBuildingStatistic_TotalBuildingsBuilt(),0,3);

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
           $"{questData.questProgress[0]} / {questData.questProgressCap[0]} Equip the Hammer and Open the Building Menu",
           $"{questData.questProgress[1]}  /  {questData.questProgressCap[1]} Structures Built",
           $" ",
           $"Reward: - {rewardAmounts[0]} {reward[0].itemName}"
        };
        return result;

    }

}