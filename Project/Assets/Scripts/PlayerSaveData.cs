using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static Item;

public class PlayerSaveData : MonoBehaviour
{
    public PlayerCustomization customization;
    public Container playerInventory;
    public Container equipmentInventory;
    public List<ulong> discoverdItemIDs = new List<ulong>();
    [HideInInspector] public List<CraftingRecepie> discoveredRecipies = new List<CraftingRecepie>();

    private void Start()
    {
        DiscoverRecepies();
        ValidatePlayerData();
    }
    public virtual void DiscoverItem(Item item)
    {
        if (item == null) return;
        if(!discoverdItemIDs.Contains(item.itemId))
        {
            discoverdItemIDs.Add(item.itemId);
            DiscoverRecepies();
        }
    }

    public bool CheckIfItemHasBeenDiscovered(Item item)
    {
        if (discoverdItemIDs.Contains(item.itemId))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual void DiscoverRecepies()
    {
        discoveredRecipies = new List<CraftingRecepie>();
        foreach (CraftingRecepie cr in CraftingRecepiesManager.Instance.craftingRecepies)
        {
            int count = cr.requiredItems.Length;
            foreach (Item item in cr.requiredItems)
            {
                if(CheckIfItemHasBeenDiscovered(item))
                {
                    count--;
                }
            }
            if(count <= 0)
            {
                discoveredRecipies.Add(cr);
            }
        }
    }
    #region Saving
    public string DirectoryPath()
    {
        return Path.Combine(Application.persistentDataPath, "CozyPlayerData");
    }

    public string DirectoryTempSaveDataPath()
    {
        return Path.Combine(DirectoryPath(), "TempSaveData");
    }
    public List<string> FindSavedPlayerData()
    {
        if (Directory.Exists(DirectoryPath()))
        {
            List<string> playerNames = new List<string>();
            string[] files = Directory.GetFiles(DirectoryPath());
            foreach (var file in files)
            {
                playerNames.Add(Path.GetFileNameWithoutExtension(file));
            }
            return playerNames;
        }
        else
        {
            Debug.Log("No Files found");;
            return null;
        }
    }

    public void CreatePlayerData()
    {
        PlayerSaveDataSerialized saveData = GetPlayerSaveData();
        List<string> playerFiles = FindSavedPlayerData();
        if (playerFiles == null || !playerFiles.Contains(customization.playerName))
        {
            CreateDirectories();
            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            string filePath = Path.Combine(DirectoryPath(), customization.playerName + ".json");
            File.WriteAllText(filePath, json);
            print($"Playerdata is saved to {filePath}");
            return;
        }
        print($"PlayerName Already Exists");
    }

    public void CreatePlayerData(PlayerCustomization playerCustomization)
    {
        QuestManager.Instance.CurrentQuestIndex = 0;
        customization = playerCustomization;
        CreatePlayerData();
    }

    private void CreateDirectories()
    {
        Directory.CreateDirectory(DirectoryPath());
        Directory.CreateDirectory(Path.Combine(DirectoryPath(), "TempSaveData"));
    }

    public void SavePlayerData()
    {
        PlayerSaveDataSerialized saveData = GetPlayerSaveData();
        List<string> playerFiles = FindSavedPlayerData();
        CreateDirectories();
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        string filePath = Path.Combine(DirectoryPath(), customization.playerName + ".json");
        string tempFilePath = Path.Combine(DirectoryTempSaveDataPath(), $"{customization.playerName}_TEMP.json");
        File.WriteAllText(tempFilePath, json); // Write to temp File in case of Crash
        File.Replace(tempFilePath, filePath,null); // Replace Temp File (Atomic Replacement)
        print($"Playerdata is saved to {filePath}, TempFile Saved to {tempFilePath}");
    }

    public void DeletePlayerData(string fileName)
    {
        string filePath = Path.Combine(DirectoryPath(), fileName + ".json");
        File.Delete(filePath);
    }

    public void ValidatePlayerData()
    {
        foreach(string file in FindSavedPlayerData())
        {
            string filePath = Path.Combine(DirectoryPath(), file + ".json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    PlayerSaveDataSerialized saveData = JsonConvert.DeserializeObject<PlayerSaveDataSerialized>(json);
                }
                catch(JsonException e)
                {
                    Debug.LogError($"File: {filePath} is invalid and will be deleted!");
                    DeletePlayerData(file);

                }
            }
        }
    }

    public void LoadPlayerData(string fileName)
    {
        string filePath = Path.Combine(DirectoryPath(), fileName + ".json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PlayerSaveDataSerialized saveData = JsonConvert.DeserializeObject<PlayerSaveDataSerialized>(json);
            customization.playerName = saveData.playerName;
            customization.torsoIndex = saveData.torsoIndex;
            customization.legIndex = saveData.legIndex;
            customization.feetIndex = saveData.feetIndex;
            customization.irisColor = saveData.irisColor;
            customization.pupilColor = saveData.pupilColor;
            customization.highlightColor = saveData.highlightColor;
            customization.eyelashColor = saveData.eyelashColor;
            customization.eyebrowColor = saveData.eyebrowColor;
            customization.skinColorIndex = saveData.skinColor;
            customization.eyebrowIndex = saveData.eyebrowIndex;
            customization.mouthIndex = saveData.mouthIndex;
            customization.eyelashIndex = saveData.eyelashIndex;
            customization.highlightIndex = saveData.highlightIndex;
            customization.hairIndex = saveData.hairIndex;
            customization.hairColorIndex = saveData.hairColorIndex;
            discoverdItemIDs = saveData.discoverdItemIDs;

            LoadQuestData(saveData);
            if (saveData.inventory != null && playerInventory != null && saveData.inventory.Length > 0)
            {
                playerInventory.items = saveData.inventory;
            }
            if (saveData.equipmentInventory != null && equipmentInventory != null && saveData.equipmentInventory.Length > 0)
            {
                equipmentInventory.items = saveData.equipmentInventory;
            }
        }
        else
        {
            Debug.LogWarning($"File not found: {filePath}");
        }
    }

    // Load QuestData into QuestManager
    public void LoadQuestData(PlayerSaveDataSerialized saveData)
    {
        if(saveData.questData == null ||  saveData.questData.Length <= 0)
        {
            Debug.LogWarning($"No QuestData found for {saveData.playerName}");
            return;
        }
        for (int i = 0; i < saveData.questData.Length; i++)
        {
            foreach(Quest quest in QuestManager.Instance.quests)
            {
                if(quest.questData.questId == saveData.questData[i].questId)
                {
                    QuestManager.Instance.quests[i].questData.SetupDataFromSaveFile(saveData.questData[i]);
                }
            }
        }
        QuestManager.Instance.CurrentQuestIndex = saveData.currentQuestIndex;
    }

    public PlayerSaveDataSerialized GetPlayerSaveData()
    {

        PlayerSaveDataSerialized data = new PlayerSaveDataSerialized()
        {
            playerName = customization.playerName,
            torsoIndex = customization.torsoIndex,
            legIndex = customization.legIndex,
            feetIndex = customization.feetIndex,
            irisColor = customization.irisColor,
            highlightColor = customization.highlightColor,
            eyelashColor = customization.eyelashColor,
            eyebrowColor = customization.eyelashColor,
            skinColor = customization.skinColorIndex,
            eyebrowIndex = customization.eyebrowIndex,
            mouthIndex = customization.mouthIndex,
            eyelashIndex = customization.eyelashIndex,
            highlightIndex = customization.highlightIndex,
            hairIndex = customization.hairIndex,
            hairColorIndex = customization.hairColorIndex,
            discoverdItemIDs = discoverdItemIDs,
        };
        if(QuestManager.Instance != null)
        {
            data.currentQuestIndex = QuestManager.Instance.CurrentQuestIndex;
            data.questData = QuestManager.Instance.quests.Select(x => x.questData).ToArray();
        }
        if(playerInventory != null)
        {
            data.inventory = playerInventory.items;
        }
        if(equipmentInventory != null)
        {
            data.equipmentInventory = equipmentInventory.items;
        }

        return data;
    }

    #endregion
}

[Serializable]
public struct PlayerSaveDataSerialized : INetworkSerializable
{
    public string playerName;
    public int torsoIndex;
    public int legIndex;
    public int feetIndex;
    public SerializeableColor irisColor;
    public SerializeableColor pupilColor;
    public SerializeableColor highlightColor;
    public SerializeableColor eyelashColor;
    public SerializeableColor eyebrowColor;
    public int skinColor;
    public int eyebrowIndex;
    public int mouthIndex;
    public int eyelashIndex;
    public int highlightIndex;
    public int hairIndex;
    public int hairColorIndex;
    // No Network Sync for these Atributes
    public List<ulong> discoverdItemIDs; 
    public ItemData[] inventory;
    public ItemData[] equipmentInventory;
    public int currentQuestIndex;
    public QuestData[] questData;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref torsoIndex);
        serializer.SerializeValue(ref legIndex);
        serializer.SerializeValue(ref feetIndex);
        serializer.SerializeValue(ref irisColor);
        serializer.SerializeValue(ref pupilColor);
        serializer.SerializeValue(ref highlightColor);
        serializer.SerializeValue(ref eyelashColor);
        serializer.SerializeValue(ref eyelashColor);
        serializer.SerializeValue(ref eyebrowColor);
        serializer.SerializeValue(ref skinColor);
        serializer.SerializeValue(ref eyebrowIndex);
        serializer.SerializeValue(ref mouthIndex);
        serializer.SerializeValue(ref eyelashIndex);
        serializer.SerializeValue(ref highlightIndex);
        serializer.SerializeValue(ref hairIndex);
        serializer.SerializeValue(ref hairColorIndex);
    }
}

public struct SerializeableColor : INetworkSerializable, IEquatable<SerializeableColor>
{
    public float r;
    public float g;
    public float b;
    public float a;

    public SerializeableColor(float r,float g,float b,float a)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public bool Equals(SerializeableColor other)
    {
        throw new NotImplementedException();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref r);
        serializer.SerializeValue(ref g);
        serializer.SerializeValue(ref b);
        serializer.SerializeValue(ref a);
    }

    public static implicit operator Color(SerializeableColor serializableColor)
    {
        return new Color(serializableColor.r, serializableColor.g, serializableColor.b, serializableColor.a);
    }

    public static implicit operator SerializeableColor(Color color)
    {
        return new SerializeableColor(color.r, color.g, color.b, color.a);
    }
}
