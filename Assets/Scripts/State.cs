using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class State : NetworkBehaviour
{
    public enum StateName { Empty,Controlling,Interacting,Throwing,
        Flee,
        Aiming,
        Visible,
        Invisible,
        LampOn,
        LampOff,
        Dragging,
        Dashing,
        Chasing,
        Attacking,
        Hurt,
        Edgeing,
        Dead,
        LockOn,
        Staggered,
        Invalid,
        LookForDraggables,
        Alerted,
        Idle,
        Approaching,
        Circling,
        Taunting,
        ApproachForAttack,
        AttackRecovery,
        Shielding,
        TransitioningLevel,
        Cutszene
    }
    public StateName stateName;

    /// <summary>
    /// Gets called when state is entered
    /// </summary>
    /// <param name="source"></param>
    public virtual void EnterState(GameObject source)
    {

    }

    /// <summary>
    /// Gets called when current state is this state
    /// </summary>
    public virtual void UpdateState(GameObject source)
    {

    }

    /// <summary>
    /// Gets called for the current state and checks Transitions to other state from this state
    /// </summary>
    /// <returns></returns>
    public virtual StateName Transition(GameObject source)
    {
        return stateName;
    }

    /// <summary>
    /// Gets called when exiting state
    /// </summary>
    public virtual void ExitState(GameObject source)
    {

    }

    /// <summary>
    /// Gets called everytime regadless of current state
    /// </summary>
    /// <returns></returns>
    public virtual StateName AnyTransition(GameObject source)
    {
        return StateName.Empty;
    }
}
