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
    public int[] questProgress;
    public int[] questProgressCap;
    [JsonIgnore]
    public string questName;

    // constructor
    public QuestData(int questId, int[] questProgress, int[] questProgressCap, string questName)
    {
        this.questId = questId;
        this.questProgress = questProgress;
        this.questProgressCap = questProgressCap;
        this.questName = questName;
    }

    // Setup Questdata from SaveFile
    public void SetupDataFromSaveFile(QuestData saveQuestData)
    {
        if(this.questId == saveQuestData.questId)
        {
            this.questProgress = saveQuestData.questProgress;
            Debug.Log($"Quest {questName} Progress: {questProgress[0]} / {questProgressCap[0]}");
        }
    }
}