using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SA_StandardStrike : BaseBattleAction
{
    SA_StandardStrike()
    {
        ActName = "Strike";
        ActDesc = "A simple strike to the chest";
        ActPow = 10f;
        ActCost = 0f;
        ActionStarted = false;
        MustBeDown = false;
        CostsHP = false;
        TargetSelf = false;
        MoveType = Type.NEUTRAL;
        Illegal = false;
        Finisher = false;
    }
    
    public override IEnumerator PerformAction(BaseCharStateMachine Actor, BaseCharStateMachine Target)
    {
        if (ActionStarted == true) yield break;
        ActionStarted = true;

        //Move Character to Target
        while(MoveToTarget(Actor.Wrestler, Target.Wrestler))
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
        Target.AddMomentum(Momentum * 0.5f, MoveType);

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
