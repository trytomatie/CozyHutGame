using MalbersAnimations.Controller;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_CraftingStation : Interactable
{
    // What needs to happen?
    // Open Crafting Menu
    // Disable Player Movment
    // Activate Mouse
    // Player Crafting Anim (Maybe)

    // On Exit Interaction
    // Disable Crafting Menu
    // Enable Player Movement
    // Hide Cursor
    public override void LocalInteraction(GameObject source)
    {
        base.LocalInteraction(source);
    }
}
