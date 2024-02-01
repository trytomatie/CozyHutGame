using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingSelectionUI : MonoBehaviour
{
    public GameObject buildingSelectionSlotPrefab;

    public Transform buildingSelection;
    public Transform craftingSelection;
    public Transform furnitureSelection;

    // Start is called before the first frame update
    void Start()
    {
        AssignBuildingSlots();
        SwitchActiveSelection(0);
    }

    private void AssignBuildingSlots()
    {
        foreach(BuildingObject bo in BuildingObjectManager.Instance.buildingObjects)
        {
            GameObject go = Instantiate(buildingSelectionSlotPrefab);
            if(bo.sprite != null)
            {
                go.transform.GetChild(0).GetComponent<Image>().sprite = bo.sprite;
            }
            go.GetComponentInChildren<BuildingSlotUI>().buildingData = bo;
            switch (bo.buildingCategory)
            {
                case BuildingObject.BuildingCategory.Building:
                    go.transform.parent = buildingSelection;
                    break;
                case BuildingObject.BuildingCategory.Crafting:
                    go.transform.parent = craftingSelection;
                    break;
                case BuildingObject.BuildingCategory.Furniture:
                    go.transform.parent = furnitureSelection;
                    break;
            }
            go.transform.localScale = Vector3.one;
        }
    }

    public void SwitchActiveSelection(int i)
    {
        buildingSelection.gameObject.SetActive(false);
        craftingSelection.gameObject.SetActive(false);
        furnitureSelection.gameObject.SetActive(false);
        switch (i)
        {
            case 0:
                buildingSelection.gameObject.SetActive(true);
                break;
            case 1:
                craftingSelection.gameObject.SetActive(true);
                break;
            case 2:
                furnitureSelection.gameObject.SetActive(true);
                break;
        }
    }
}
