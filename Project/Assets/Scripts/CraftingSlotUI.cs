using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSlotUI : MonoBehaviour
{
    public Image sprite;
    public TextMeshProUGUI text;
    public CraftingRecepie craftingRecipieInfo;

    public MEvent selectEvent;
    public void SetUpCraftingSlot(CraftingRecepie recpie)
    {
        sprite.sprite = recpie.sprite;
        text.text = recpie.recepieName;
        craftingRecipieInfo = recpie;
    }

    public void SelectCraftingSlot()
    {
        selectEvent.Invoke(gameObject);
    }
}
