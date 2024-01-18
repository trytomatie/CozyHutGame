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
    public static QuestManager Instance;



    private void Start()
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
        // Check the quest conditions of the current quest
        if (Instance.quests[Instance.currentQuestIndex].CheckQuestConditions())
        {
            Instance.quests[Instance.currentQuestIndex].CompleteQuest();
            if (Instance.currentQuestIndex < Instance.quests.Length)
            {
                Instance.currentQuestIndex++;
            }
        }
        Instance.UpdateQuestUI();
    }

    private void UpdateQuestUI()
    {
        string questName = Instance.quests[Instance.currentQuestIndex].questData.questName;
        string[] questDescription = Instance.quests[Instance.currentQuestIndex].GetQuestDescription();

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
        set => currentQuestIndex = value; 
    }

}

