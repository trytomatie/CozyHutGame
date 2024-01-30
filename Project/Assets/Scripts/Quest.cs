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
        SystemMessageManagerUI.ShowSystemMessage("Quest completed!");
        if(reward.Length > 0)
        {
            SystemMessageManagerUI.ShowSystemMessage($"You received {rewardAmounts[0]} {reward[0].itemName}");
            AwardItems(GameManager.GetLocalPlayer().GetComponent<NetworkPlayerInit>().inventory);
        }
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
            int i = 0;
            foreach(Vector2Int progress in questData.questProgress)
            {
                this.questProgress[i].x = progress.x;
                i++;
            }
        }
    }
}