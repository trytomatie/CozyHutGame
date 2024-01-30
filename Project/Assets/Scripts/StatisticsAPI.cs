using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class StatisticsAPI : MonoBehaviour
{
    // Singleton pattern
    public static StatisticsAPI Instance;

    // Building Stats
    public Dictionary<ulong, int> buildingsBuilt = new Dictionary<ulong, int>();
    public Dictionary<ulong, int> resorucesDestroyed = new Dictionary<ulong, int>();

    public void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public static void AddBuildingBuilt(ulong buildingId)
    {
        if (Instance.buildingsBuilt.ContainsKey(buildingId))
        {
            Instance.buildingsBuilt[buildingId]++;
        }
        else
        {
            Instance.buildingsBuilt.Add(buildingId, 1);
        }
    }

    public static void AddResourceDestroyed(ulong resourceId)
    {
        if (Instance.resorucesDestroyed.ContainsKey(resourceId))
        {
            Instance.resorucesDestroyed[resourceId]++;
        }
        else
        {
            Instance.resorucesDestroyed.Add(resourceId, 1);
        }
    }

    public static int GetResourceStatistic_ResourceDestroyed(ulong id)
    {
        if (Instance.resorucesDestroyed.ContainsKey(id))
        {
            return Instance.resorucesDestroyed[id];
        }
        return 0;
    }

    public static int GetResourceStatistic_ResourceDestroyed(ulong[] id)
    {
        int total = 0;
        foreach (ulong value in id)
        {
            total += GetResourceStatistic_ResourceDestroyed(value);
        }
        return total;
    }

    public static int GetBuildingsStatistic_BuildingsBuilt(ulong id) 
    { 
        if(Instance.buildingsBuilt.ContainsKey(id))
        {
            return Instance.buildingsBuilt[id];
        }
        return 0;
    }

    public static int GetBuildingStatistic_TotalBuildingsBuilt()
    {
        int total = 0;
        foreach (int value in Instance.buildingsBuilt.Values)
        {
            total += value;
        }
        return total;
    }
}

