using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FIN_Powerbomb : BaseBattleAction
{
    FIN_Powerbomb()
    {
        ActName = "Powerbomb";
        ActDesc = "Pick up your opponent and slam them down into the canvas as hard as you can!";
        ActPow = 40f;
        ActCost = 0f;
        ActionStarted = false;
        MustBeDown = false;
        CostsHP = false;
        TargetSelf = false;
        MoveType = Type.NEUTRAL;
        Illegal = false;
        Finisher = true;
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

        //Add momentum to Target
        float Momentum = (Damage / (Target.Stats.HP.Base * 2)) * 100;
        Target.AddMomentum(Momentum * 0.5f, MoveType);

        //Reset Actor's momentum
        Actor.ResetMomentum();

        //If target is down, reset turns down
        if(Target.Status_Down == true)
        {
            Target.Turns_Down = 0;
        }

        while (MoveToPosition(Actor.Wrestler, Actor.startPosition))
        {
            yield return null;
        }

        //Set BSM
        Actor.BSM.TheTurn = BattleStateMachine.TurnStates.DONE;

        //End Coroutine
        ActionStarted = false;
    }
}
