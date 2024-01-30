using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


// Manages all quests for the player
public class QuestManager : MonoBehaviour
{
    public Quest[] quests;
    private int currentQuestIndex = 0;
    private static QuestManager instance;

    private bool questCompleted = false;


    private void Awake()
    {
        // singlton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public static void CheckQuestConditions()
    {
        if(Instance.quests[Instance.CurrentQuestIndex] == null || Instance.questCompleted)
        {
            return;
        }
        // Check the quest conditions of the current quest
        if (Instance.quests[Instance.CurrentQuestIndex].CheckQuestConditions())
        {
            Instance.questCompleted = true;
            Instance.quests[Instance.CurrentQuestIndex].CompleteQuest();
            if (Instance.CurrentQuestIndex < Instance.quests.Length)
            {
                Instance.CurrentQuestIndex++;
            }
            Instance.questCompleted = false;
        }
        Instance.UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        string questName = Instance.quests[Instance.CurrentQuestIndex].questData.questName;
        string[] questDescription = Instance.quests[Instance.CurrentQuestIndex].GetQuestDescription();

        GameUI.Instance.questName.text = questName;
        GameUI.Instance.questDescription.text = "";
        foreach (string description in questDescription)
        {
            GameUI.Instance.questDescription.text += description + "\n";
        }
    }

    public int CurrentQuestIndex 
    { 
        get => currentQuestIndex;
        set 
        {
            currentQuestIndex = value;
        }
    }
    public static QuestManager Instance { get => instance; set => instance = value; }
}

