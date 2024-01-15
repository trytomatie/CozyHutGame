using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefiningRecipieSlotUI : MonoBehaviour
{
    public Image craftingRecipieImage;
    public Image[] craftingItemsImages;
    public TextMeshProUGUI[] craftingItemsTexts;

    public void Setup(RefiningRecipie recipie)
    {
        craftingRecipieImage.sprite = recipie.sprite;

        // Hide all images and texts
        foreach(Image i in craftingItemsImages)
        {
            i.gameObject.SetActive(false);
        }
        foreach(TextMeshProUGUI t in craftingItemsTexts)
        {
            t.gameObject.SetActive(false);
        }

        // Show only the required items
        for(int i = 0; i < recipie.requiredItems.Length; i++)
        {
            craftingItemsImages[i].gameObject.SetActive(true);
            craftingItemsTexts[i].gameObject.SetActive(true);
            craftingItemsImages[i].sprite = recipie.requiredItems[i].sprite;
            craftingItemsTexts[i].text = $"{ recipie.requieredItemsCount[i]}x";
        }
    }
}
