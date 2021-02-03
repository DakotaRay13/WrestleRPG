using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT_Rest : BaseBattleAction
{
    TNT_Rest()
    {
        ActName = "Rest";
        ActDesc = "Take a breath. Recover SP.";
        ActPow = 0f;
        ActCost = 0f;
        ActionStarted = false;
        MustBeDown = false;
        CostsHP = false;
        TargetSelf = true;
        MoveType = Type.NEUTRAL;
        Illegal = false;
        Finisher = false;
    }

    public override IEnumerator PerformAction(BaseCharStateMachine Actor, BaseCharStateMachine Target)
    {
        if (ActionStarted == true) yield break;
        ActionStarted = true;

        yield return new WaitForSeconds(2f);
        
        //Add SP
        Actor.Stats.SP.Current += 10f;
        Actor.ZeroOrMax(Actor.Stats.SP);
        
        //Set BSM
        Actor.BSM.TheTurn = BattleStateMachine.TurnStates.DONE;

        //End Coroutine
        ActionStarted = false;
    }
}
