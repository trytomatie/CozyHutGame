using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

public class PlayerSaveData : MonoBehaviour
{
    public PlayerCustomization customization;
    public List<ulong> discoverdItemIDs = new List<ulong>();
    [HideInInspector] public List<CraftingRecepie> discoveredRecipies = new List<CraftingRecepie>();

    private void Start()
    {
        DiscoverRecepies();
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
            Debug.Log("No Files found");
            return null;
        }
    }



    public void SavePlayerData()
    {
        PlayerSaveDataSerialized saveData = GetPlayerSaveData();
        List<string> playerFiles = FindSavedPlayerData();
        if (playerFiles == null || !playerFiles.Contains(customization.playerName))
        {
            Directory.CreateDirectory(DirectoryPath());
            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            string filePath = Path.Combine(DirectoryPath(), customization.playerName + ".json");
            File.WriteAllText(filePath, json);
            print($"Playerdata is saved to {filePath}");
            return;
        }
        print($"PlayerName Already Exists");

    }


    public void OverrideSavePlayerData()
    {
        PlayerSaveDataSerialized saveData = GetPlayerSaveData();
        List<string> playerFiles = FindSavedPlayerData();
        Directory.CreateDirectory(DirectoryPath());
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        string filePath = Path.Combine(DirectoryPath(), customization.playerName + ".json");
        File.WriteAllText(filePath, json);
        print($"Playerdata is saved to {filePath}");
    }

    public void LoadPlayerData(string fileName)
    {
        string filePath = Path.Combine(DirectoryPath(), fileName + ".json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PlayerSaveDataSerialized saveData = JsonConvert.DeserializeObject<PlayerSaveDataSerialized>(json);
            customization.torsoIndex = saveData.torsoIndex;
            customization.legIndex = saveData.legIndex;
            customization.feetIndex = saveData.feetIndex;
            customization.irisColorIndex = saveData.irisColorIndex;
            customization.pupilColorIndex = saveData.pupilColorIndex;
            customization.highlightColorIndex = saveData.highlightColorIndex;
            customization.eyelashColorIndex = saveData.eyelashColorIndex;
            customization.eyebrowColorIndex = saveData.eyebrowColorIndex;
            customization.skinColorIndex = saveData.skinColorIndex;
            customization.eyebrowIndex = saveData.eyebrowIndex;
            customization.mouthIndex = saveData.mouthIndex;
            customization.eyelashIndex = saveData.eyelashIndex;
            customization.highlightIndex = saveData.highlightIndex;
            discoverdItemIDs = saveData.discoverdItemIDs;
        }
        else
        {
            Debug.LogWarning($"File not found: {filePath}");
        }
    }

    public PlayerSaveDataSerialized GetPlayerSaveData()
    {
        return new PlayerSaveDataSerialized()
        {
            playerName = customization.playerName,
            torsoIndex = customization.torsoIndex,
            legIndex = customization.legIndex,
            feetIndex = customization.feetIndex,
            irisColorIndex = customization.irisColorIndex,
            highlightColorIndex = customization.highlightColorIndex,
            eyelashColorIndex = customization.eyelashColorIndex,
            eyebrowColorIndex = customization.eyelashColorIndex,
            skinColorIndex = customization.skinColorIndex,
            eyebrowIndex = customization.eyebrowIndex,
            mouthIndex = customization.mouthIndex,
            eyelashIndex = customization.eyelashIndex,
            highlightIndex = customization.highlightIndex,
            discoverdItemIDs = discoverdItemIDs
        };
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
    public int irisColorIndex;
    public int pupilColorIndex;
    public int highlightColorIndex;
    public int eyelashColorIndex;
    public int eyebrowColorIndex;
    public int skinColorIndex;
    public int eyebrowIndex;
    public int mouthIndex;
    public int eyelashIndex;
    public int highlightIndex;
    public List<ulong> discoverdItemIDs;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref torsoIndex);
        serializer.SerializeValue(ref legIndex);
        serializer.SerializeValue(ref feetIndex);
        serializer.SerializeValue(ref irisColorIndex);
        serializer.SerializeValue(ref pupilColorIndex);
        serializer.SerializeValue(ref highlightColorIndex);
        serializer.SerializeValue(ref eyelashColorIndex);
        serializer.SerializeValue(ref eyelashColorIndex);
        serializer.SerializeValue(ref eyebrowColorIndex);
        serializer.SerializeValue(ref skinColorIndex);
        serializer.SerializeValue(ref eyebrowIndex);
        serializer.SerializeValue(ref mouthIndex);
        serializer.SerializeValue(ref eyelashIndex);
        serializer.SerializeValue(ref highlightIndex);
    }
}
