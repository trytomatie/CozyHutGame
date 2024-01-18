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

    public static int GetBuildingsStatistic_BuildingsBuilt(ulong id) 
    { 
        if(Instance.buildingsBuilt.ContainsKey(id))
        {
            return Instance.buildingsBuilt[id];
        }
        return 0;
    }
}

