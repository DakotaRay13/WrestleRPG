using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for the Character State Machines in the Battle System

public abstract class BaseCharStateMachine : MonoBehaviour
{
    //State Machine type
    public string type;

    //Wrestler's Character File
    public GameObject Wrestler;
    public CharStats Stats;

    //Damage Modifiers for Wrestler's Diffrent Parts
    //Modify at start for Strengths / Weaknesses
    public float DMGMOD_MAX = 2.0f;
    public float DMGMOD_Head = 1.0f;
    public float DMGMOD_Torso = 1.0f;
    public float DMGMOD_Arms = 1.0f;
    public float DMGMOD_Legs = 1.0f;

    //This character's momentum towards their finisher
    public Stats Momentum = new Stats(100f);

    //How many strikes this character has
    public int DQ_Strikes = 0;

    //Battle State Machine
    public BattleStateMachine BSM;

    //Character States
    public enum CharTurnState
    {
        GETACTION,
        CHOOSETARGET,
        WAIT,
        DOWN,
        WIN,
        LOSS
    }
    
    public CharTurnState TurnState;

    //Starting Position of this character
    public Vector3 startPosition;

    // Action Speed Counter //
    public float ASC = 0f;

    //Possible Targets//
    public List<GameObject> PossibleTargets = new List<GameObject>();

    // Status Effects //
    ////////////////////

    //Down Status Variables
    public bool Status_Down = false;
    public int Turns_Down = 0;
    public float StartWP = 0f;

    //Defending Status
    public bool Status_Defend = false;

    //Endure Status
    //If HP goes to 0 while defending, this turns false and HP = 1
    public bool Status_Endure = true;

    public IEnumerator GetUpFail()
    {
        Debug.Log(Stats.Name + " is Down");
        BSM.DisplayNote(Stats.Name + " is Down!");

        yield return new WaitForSeconds(2f);
        
        //Incrament Turns Down and go to the next turn
        Turns_Down++;
        BSM.TheTurn = BattleStateMachine.TurnStates.WHOSNEXT;
    }

    public IEnumerator GetUpSuccess()
    {
        Debug.Log(Stats.Name + " got up");
        BSM.DisplayNote(Stats.Name + " got up!");

        yield return new WaitForSeconds(2f); //ADD GET UP ANIMATION LATER//

        Status_Down = false;
        Stats.HP.Current = Stats.WP.Current;
        Turns_Down = 0;
        BSM.TheTurn = BattleStateMachine.TurnStates.GETACTION;
    }

    public abstract bool TryPinEscape(float ActorCount);

    //This character tries to break up the pin. [CHANGE TO ABSTRACT LATER?]
    public bool TryPinBreak()
    {
        //CurrentWP% + Current Spd
        float Chance = (Stats.WP.Current / Stats.WP.Base) + (Stats.SPD.Current * 0.01f);

        float RNGesus = Random.Range(0, 1f);

        if (RNGesus <= Chance) return true;
        else return false;
    }

    //Debuff a character stat.
    public void DebuffStat(Stats TargetStat, Stats AssistStat, float BuffPow)
    {
        float Debuff;

        //Calculate the debuff
        Debuff = BuffPow + (AssistStat.Current / 5f);

        //Do Damage to TargetStat.Current
        TargetStat.Current -= Debuff;

        //Zero out if needed
        if (TargetStat.Current <= 0f) TargetStat.Current = 0f;
    }

    //Buff a character stat
    public void BuffStat(Stats TargetStat, Stats AssistStat, float BuffPow)
    {
        float Buff;

        //Calculate the buff
        Buff = BuffPow + (AssistStat.Current / 5f);

        //Apply the Buff
        TargetStat.Current += Buff;

        //Max out is *2 Stat
        if(TargetStat.Current > TargetStat.Base * 2)
        {
            TargetStat.Current = TargetStat.Base * 2;
        }
        Debug.Log(TargetStat.Current);
    }

    //Reset a stat
    public void ResetStat(Stats TargetStat)
    {
        TargetStat.Current = TargetStat.Base;
    }

    //Do Damage to this Character
    public void DamageHP(float Damage)
    {
        //If the character can endure, Endure the attack
        if (Damage > Stats.HP.Current && Status_Defend && Status_Endure)
        {
            EndureDamage();
        }
        else
        {
            //Do Damage to TargetStat.Current
            Stats.HP.Current -= Damage;

            //Zero out if needed
            ZeroOrMax(Stats.HP);
        }

        //Show damage Text
        if (BSM.FloatingText)
            ShowDamageText(Damage);
        else Debug.Log("No Damage Text");
    }

    //Damage the WP
    public void DamageWP(float Damage)
    {
        float WP_Shield = 0.5f;
        float WP_Damage;

        //If the character is defending, return
        if (Status_Defend == true) return;

        //If the character is standing, WP is shielded. Else, WP takes full force of attack.
        if (Status_Down == false)
        {
            WP_Damage = Mathf.Round(Damage * WP_Shield);
        }

        else
        {
            WP_Damage = Mathf.Round(Damage);
        }

        //Damage the WP
        Stats.WP.Current -= WP_Damage;

        //Minimum WP should be 1
        ZeroOrMax(Stats.WP);
        if (Stats.WP.Current == 0f) Stats.WP.Current = 1f;
    }

    //Function to Calculate Damage
    public float CalculateDamage(float ActPow, float ATK, float DEF)
    {
        float Damage = ActPow * (ATK / (ATK + DEF));

        if (Status_Defend) Damage = Damage / 2;

        return Mathf.Round(Damage);
    }

    //Function to repair stat if below 0
    public void ZeroOrMax(Stats Stat)
    {
        if (Stat.Current <= 0f) Stat.Current = 0f;
        else if (Stat.Current > Stat.Base) Stat.Current = Stat.Base;
    }

    //Function for Enduring a turn
    public void EndureDamage()
    {
        //Set HP.Current to 1, endure and defend to false, then return true
        BSM.DisplayNote(Stats.Name + " endured the Hit!");
        Stats.HP.Current = 1f;
        Status_Endure = false;
    }

    public void TakeCostOfAction(BaseBattleAction Action)
    {
        if (Action.CostsHP == true)
        {
            Debug.Log("Cost taken for " + Action.ActName);
            Stats.HP.Current -= Action.ActCost;
        }
        else
        {
            Stats.SP.Current -= Action.ActCost;
        }
    }

    public void AddMomentum(float X, BaseBattleAction.Type MoveType)
    {
        float BonusModifier = 2f;
        
        //Assign Bonuses
        switch(MoveType)
        {
            case (BaseBattleAction.Type.NEUTRAL):
                {
                    //No bonuses on neutral actions.
                    break;
                }
            case (BaseBattleAction.Type.CLEAN):
                {
                    //If character is Face, apply bonus.
                    if (Stats.Face == true)
                    {
                        X = X * BonusModifier;
                    }
                        
                    break;
                }
            case (BaseBattleAction.Type.DIRTY):
                {
                    //If character is Heel, apply bonus.
                    if (Stats.Face == false)
                    {
                        X = X * BonusModifier;
                    }

                    break;
                }
        }

        //If character is defending, Apply Bonus Modifier
        if (Status_Defend) X = X * BonusModifier;

        //Add Momentum
        Momentum.Current += X;
        ZeroOrMax(Momentum);
    }

    //Reset Momentum to 0
    public void ResetMomentum()
    {
        Momentum.Current = 0f;
    }

    //Adds or replaces character's special action.
    //Enemy version also replaces in available actions
    public abstract void AddOrReplaceSpecialAction(BaseBattleAction Action);

    public void ShowDamageText(float Damage)
    {
        var TextOBJ = Instantiate(BSM.FloatingText, this.transform.position, Quaternion.identity, transform);
        TextOBJ.GetComponent<TextMesh>().text = Damage.ToString();
    }
}
