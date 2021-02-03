using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT_BattleCry : BaseBattleAction
{
    TNT_BattleCry()
    {
        ActName = "Battle Cry";
        ActDesc = "A loud cry to boost your ATK";
        ActPow = 3f;
        ActCost = 5f;
        ActionStarted = false;
        MustBeDown = false;
        CostsHP = true;
        TargetSelf = true;
        MoveType = Type.CLEAN;
        Illegal = false;
        Finisher = false;
    }

    public override IEnumerator PerformAction(BaseCharStateMachine Actor, BaseCharStateMachine Target)
    {
        if (ActionStarted == true) yield break;
        ActionStarted = true;

        //Wait (Later, do Attack Animation here
        yield return new WaitForSeconds(1.5f);

        //Buff the ATK
        Actor.BuffStat(Actor.Stats.ATK, Actor.Stats.CHR, ActPow);

        if(Actor.Stats.ATK.Current == Actor.Stats.ATK.Base * 2)
        {
            Actor.BSM.DisplayNote("Attack is Maxed Out.");
        }
        else Actor.BSM.DisplayNote("Attack has Increased.");

        yield return new WaitForSeconds(1.5f);

        //Set BSM
        Actor.BSM.TheTurn = BattleStateMachine.TurnStates.DONE;

        //End Coroutine
        ActionStarted = false;
    }
}
