using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPC_GetWeaponFromRing : BaseBattleAction
{
    //List of possible weapons under the ring
    public List<BaseBattleAction> Weapons;

    SPC_GetWeaponFromRing()
    {
            ActName = "Get Weapon from Ring";
            ActDesc = "A simple strike to the chest";
            ActPow = 10f;
            ActCost = 0f;
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

        Actor.BSM.DisplayNote(Actor.Stats.Name + " looked under the ring...");

        int RNGesus = Random.Range(0, Weapons.Count);

        //Add Weapon to Actor's Special Action
        Actor.AddOrReplaceSpecialAction(Weapons[RNGesus]);

        yield return new WaitForSeconds(1.5f);

        Actor.BSM.DisplayNote(Actor.Stats.Name + " found a " + Weapons[RNGesus].ActName);

        Actor.AddMomentum(5f, MoveType);

        yield return new WaitForSeconds(1.5f);

        //Set BSM
        Actor.BSM.TheTurn = BattleStateMachine.TurnStates.DONE;

        //End Coroutine
        ActionStarted = false;
    }
}
