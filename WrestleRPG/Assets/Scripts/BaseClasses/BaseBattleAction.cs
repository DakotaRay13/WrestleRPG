using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBattleAction : MonoBehaviour {

    public string ActName;      //Name of Action
    public string ActDesc;      //Description of Action

    public float ActPow;        //Action Power
    public float ActCost;       //Action Cost

                                //Action Animation

    public bool MustBeDown;     //CONDITION: Target must be down
    public bool CostsHP;        //CONDITION: Cost HP instead of SP

    public bool TargetSelf;     //Actor targets themself.

    public enum Type            //What type of move is this?
    {
        NEUTRAL,                //  -No Allignment
        CLEAN,                  //  -Clean move. Allign to "Face"
        DIRTY                   //  -Dirty Move. Allign to "Heel". Not nessisarily illegal.
    }

    public Type MoveType;

    public bool Illegal;        //Is move illegal? (Only used by CPU to stop use)

    public bool Finisher;       //Is this move a Finisher (Only used by CPU)

    public abstract IEnumerator PerformAction(BaseCharStateMachine Actor, BaseCharStateMachine Target);

    // Move To Target Functions
    public bool ActionStarted = false;
    public float animSpeed = 10f;

    public bool MoveToTarget(GameObject Actor, GameObject Target)
    {
        if (Vector3.Distance(Actor.transform.position, Target.transform.position) > 2)
        {
            Actor.transform.position = Vector3.MoveTowards(Actor.transform.position, Target.transform.position, animSpeed * Time.deltaTime);
            return true;
        }
        else return false; //Once true, Movement Finished
    }

    public bool MoveToPosition(GameObject Actor, Vector3 Target)
    {
        return Target != (Actor.transform.position = Vector3.MoveTowards(Actor.transform.position, Target, animSpeed * Time.deltaTime));
    }

    //If function Action Started is accidently set to True prematurly, sets to false.
    public void CheckIfTrueError()
    {
        if (ActionStarted == true) ActionStarted = false;
    }

    //if MustBeDown = true, call this action with character's possible targets
    public bool IsDownActionPossible(List<GameObject> PossibleTargets)
    {
        bool x = false;

        foreach(GameObject Target in PossibleTargets)
        {
            if (Target.GetComponent<BaseCharStateMachine>().Status_Down == true)
                x = true;
        }

        return x;
    }

    //If character cannot afford move, return false
    public bool IsCostOfActionPossible(BaseCharStateMachine Actor)
    {
        //Apply HP or SP as needed
        if (CostsHP == true)
        {
            //Is CurrentStat > ActCost?
            if (Actor.Stats.HP.Current > ActCost)
            {
                return true;
            }
            
            else
            {
                return false;
            }
        }

        else
        {
            //Is CurrentStat > ActCost?
            if (Actor.Stats.SP.Current >= ActCost)
            {
                return true;
            }

            else
            {
                return false;
            }
        }
    }

    //Returns true or False if there is a bonus to the momentum based on character Alignment
    public bool FaceHeelMomentumBonus(bool isFace)
    {
        if (isFace == true && MoveType == Type.CLEAN)
            return true;
        else if (isFace == false && MoveType == Type.DIRTY)
            return true;
        else return false;
    }
}