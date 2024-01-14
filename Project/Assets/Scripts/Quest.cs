using Newtonsoft.Json;
using UnityEngine;
using static Item;

public class Quest : ScriptableObject
{
    public QuestData questData;
    public Item[] condtionItems;
    public Item[] reward;
    public int[] rewardAmounts;

    public Quest()
    {
        // Setup Quest
        questData = new QuestData();
    }
    public virtual bool CheckQuestConditions()
    {
        return true;
    }

    public virtual string[] GetQuestDescription()
    {
        return new string[] { "No Objective" };
    }

    public virtual void CompleteQuest()
    {
        Debug.Log("Quest Completed");
    }

    public virtual void AwardItems(Container targetContainer)
    {
        for (int i = 0; i < reward.Length; i++)
        {
            targetContainer.AddItemServerRpc(new Item.ItemData(reward[i].itemId, rewardAmounts[i]));
        }
    }
}

public struct QuestData
{
    public int questId;
    public Vector2Int[] questProgress;
    [JsonIgnore]
    public string questName;

    // constructor
    public QuestData(int questId, ItemData[] reward, int[] rewardAmounts, Vector2Int[] questProgress, string questName, string questDescription)
    {
        this.questId = questId;
        this.questProgress = questProgress;
        this.questName = questName;
    }

    // Setup Questdata from SaveFile
    public void SetupDataFromSaveFile(QuestData questData)
    {
        if(this.questId == questData.questId)
        {
            this.questProgress = questData.questProgress;
        }
    }
}