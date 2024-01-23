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
        questData.questId = 0;
        questData.questName = "Finding new Lands.";
        questData.questProgress = new Vector2Int[] 
        { 
            new Vector2Int(0, 1), // Reached Main Island
        };
    }

    public override bool CheckQuestConditions()
    {
        if (questData.questProgress[0].x >= questData.questProgress[0].y)
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
        return new string[]
        {
           $"{questData.questProgress[0].x} / {questData.questProgress[0].y} Main Island Reached",
        };
    }

}