using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPC_Pin : BaseBattleAction
{
    SPC_Pin()
    {
        ActName = "Pin";
        ActDesc = "Pin your opponent. A three count wins the match.";
        ActPow = 0f;
        ActCost = 0f;
        ActionStarted = false;
        MustBeDown = true;
        CostsHP = false;
        TargetSelf = false;
        MoveType = Type.NEUTRAL;
        Illegal = false;
        Finisher = false;
    }

    //*************************************************************************************************

    private int PinCount = 0;
    private List<BaseCharStateMachine> EnemiesUp = new List<BaseCharStateMachine>(); //Enemies who are standing
    private bool Breakup = false; //Set to true if there is a breakup
    private BaseCharStateMachine PinBreaker;

    float ActorCount0, ActorCount1, ActorCount2; //Modifiers for chances on each count. 0 = ActorWP%, 1 = ActorWP% * 0.75, 2 = ActorWP% * 0.50.

    public override IEnumerator PerformAction(BaseCharStateMachine Actor, BaseCharStateMachine Target)
    {
        if (ActionStarted == true) yield break;
        ActionStarted = true;

        //Move Character to Target
        while (MoveToTarget(Actor.Wrestler, Target.Wrestler))
        {
            yield return null;
        }

        PinCount = 0;
        //Set up ActorCount
        ActorCount0 = 0.0f;
        ActorCount1 = 0.1f;
        ActorCount2 = 0.2f;

        //The Pin Action!
        //Count 1
        yield return new WaitForSecondsRealtime(1);

        if(Target.TryPinEscape(ActorCount0) == false)
        {
            PinCount++;
            Actor.BSM.DisplayNote(PinCount.ToString());

            //Count 2
            yield return new WaitForSecondsRealtime(1);
            if (Target.TryPinEscape(ActorCount1) == false)
            {
                PinCount++;
                Actor.BSM.DisplayNote(PinCount.ToString());
                yield return new WaitForSecondsRealtime(1f);

                //Try for breakup//
                //Reset Breakup //
                Breakup = false;
                EnemiesUp.Clear();

                if (Actor.type == "PLAYER")
                {
                    foreach(GameObject Enemy in Actor.BSM.EnemyChars)
                    {
                        if(Enemy.GetComponent<EnemyStateMachine>().Status_Down == false)
                        {
                            EnemiesUp.Add(Enemy.GetComponent<EnemyStateMachine>());
                        }
                    }
                }
                if (Actor.type == "ENEMY")
                {
                    foreach(GameObject Enemy in Actor.PossibleTargets)
                    {
                        if (Enemy.GetComponent<BaseCharStateMachine>().Status_Down == false)
                        {
                            EnemiesUp.Add(Enemy.GetComponent<BaseCharStateMachine>());
                        }
                    }
                }

                if (EnemiesUp.Count > 0)
                {
                    foreach (BaseCharStateMachine Enemy in EnemiesUp)
                    {
                        if (Enemy.TryPinBreak())
                        {
                            PinBreaker = Enemy;
                            Breakup = true;
                            break;
                        }
                    }
                }

                if (Breakup == false)
                {
                    //Count 3
                    if (Target.TryPinEscape(ActorCount2) == false)
                    {
                        PinCount++;
                        Actor.BSM.DisplayNote(PinCount.ToString());
                    }
                }

            }
        }

        //If PinCount is 3, Actor wins the match.
        if(PinCount == 3)
        {
            yield return new WaitForSeconds(1f);
            Actor.BSM.FinishMatch(Actor, Target, "Pinfall");
        }

        else
        {
            //Was there a breakup?
            if(Breakup == true)
            {
                Actor.BSM.DisplayNote(PinBreaker.Stats.Name + " broke up the pin!");
                while (MoveToTarget(PinBreaker.Wrestler, Target.Wrestler))
                {
                    yield return null;
                }
                //Wait (Later, do Attack Animation here
                yield return new WaitForSeconds(0.5f);

                //Give PinBreaker some momentum
                PinBreaker.AddMomentum(5f, MoveType);

                //Move both back
                while (MoveToPosition(Actor.Wrestler, Actor.startPosition) || MoveToPosition(PinBreaker.Wrestler, PinBreaker.startPosition))
                {
                    yield return null;
                }
            }

            // There was a Kickout (PinCount != 3 and Breakup is False)
            else
            {
                Actor.BSM.DisplayNote(Target.Stats.Name + " kicked out at " + PinCount.ToString());

                while (MoveToPosition(Actor.Wrestler, Actor.startPosition))
                {
                    yield return null;
                }
            }
            //Set BSM
            Actor.BSM.TheTurn = BattleStateMachine.TurnStates.DONE;
        }

        //Give Actor and target Momentum
        Actor.AddMomentum(3f, MoveType);
        Target.AddMomentum(5f, MoveType);

        //End Coroutine
        ActionStarted = false;
    }

    
}

