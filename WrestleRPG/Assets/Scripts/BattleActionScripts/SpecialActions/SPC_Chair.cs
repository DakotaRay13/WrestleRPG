using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPC_Chair : BaseBattleAction
{
    SPC_Chair()
    {
        ActName = "Steel Chair";
        ActDesc = "Strike your opponent with a Steel Chair";
        ActPow = 30f;
        ActCost = 5f;
        ActionStarted = false;
        MustBeDown = false;
        CostsHP = false;
        TargetSelf = false;
        MoveType = Type.DIRTY;
        Illegal = true;
        Finisher = false;
    }

    public override IEnumerator PerformAction(BaseCharStateMachine Actor, BaseCharStateMachine Target)
    {
        if (ActionStarted == true) yield break;
        ActionStarted = true;

        //Move Character to Target
        while (MoveToTarget(Actor.Wrestler, Target.Wrestler))
        {
            yield return null;
        }

        //Wait (Later, do Attack Animation here
        yield return new WaitForSeconds(0.5f);

        //Calculate Damage
        float Damage = Target.CalculateDamage(ActPow, Actor.Stats.ATK.Current, Target.Stats.DEF.Current);

        //Do Damage
        Target.DamageHP(Damage);

        //Damage WP
        Target.DamageWP(Damage);

        //Add momentum to both characters
        float Momentum = (Damage / (Target.Stats.HP.Base * 2)) * 100;
        Actor.AddMomentum(Momentum, MoveType);
        Target.AddMomentum(Momentum * 0.5f, Type.NEUTRAL);

        while (MoveToPosition(Actor.Wrestler, Actor.startPosition))
        {
            yield return null;
        }

        //Set BSM
        Actor.BSM.TheTurn = BattleStateMachine.TurnStates.REFFCHECK;

        //End Coroutine
        ActionStarted = false;
    }
}
