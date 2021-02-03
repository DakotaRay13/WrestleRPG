using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT_NotWorthMyTime : BaseBattleAction
{
    TNT_NotWorthMyTime()
    {
        ActName = "Not Worth My Time";
        ActDesc = "Shrug off your opponent. Boost ATK, but drops Defense.";
        ActPow = 4f;
        ActCost = 5f;
        ActionStarted = false;
        MustBeDown = false;
        CostsHP = false;
        TargetSelf = true;
        MoveType = Type.DIRTY;
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

        if (Actor.Stats.ATK.Current == Actor.Stats.ATK.Base * 2)
        {
            Actor.BSM.DisplayNote("Attack is Maxed Out.");
        }
        else Actor.BSM.DisplayNote("Attack has Increased.");

        yield return new WaitForSeconds(1.5f);

        //Debuff the Defense
        Actor.DebuffStat(Actor.Stats.DEF, Actor.Stats.CHR, ActPow);

        if (Actor.Stats.DEF.Current == 0)
        {
            Actor.BSM.DisplayNote("Defense has bottomed out.");
        }
        else Actor.BSM.DisplayNote("Defense has decreased.");

        yield return new WaitForSeconds(1.5f);

        //Set BSM
        Actor.BSM.TheTurn = BattleStateMachine.TurnStates.DONE;

        //End Coroutine
        ActionStarted = false;
    }
}
