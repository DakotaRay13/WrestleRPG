using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : BaseCharStateMachine
{
    public List<BaseBattleAction> AvailableActions = new List<BaseBattleAction>();
    // Use this for initialization
    void Start()
    {
        type = "ENEMY";
        //Initialize State
        TurnState = CharTurnState.WAIT;

        //Reset Momentum
        ResetMomentum();
    }

    // Update is called once per frame
    void Update()
    {
        switch (TurnState)
        {
            case (CharTurnState.WAIT):
                {
                    // Idle State
                    break;
                }
            case (CharTurnState.GETACTION):
                {
                    ChooseAction();
                    TurnState = CharTurnState.CHOOSETARGET;
                    break;
                }
            case (CharTurnState.CHOOSETARGET):
                {
                    GetTargets();

                    if (PossibleTargets.Count > 0)
                    {
                        ChooseTarget();

                        BSM.ThisTurn.Tgt_Name = BSM.ThisTurn.Target.Stats.Name;
                        BSM.TheTurn = BattleStateMachine.TurnStates.PERFORMACTION;
                        TurnState = CharTurnState.WAIT;
                    }
                    else
                    {
                        Destroy(GameObject.Find(BSM.ThisTurn.Action.name));
                        TurnState = CharTurnState.GETACTION;
                    }

                    break;
                }
            case (CharTurnState.DOWN):
                {
                    bool GUp = TryGetUp();
                    if (GUp)
                    {
                        StartCoroutine(GetUpSuccess());
                        TurnState = CharTurnState.WAIT;
                    }

                    else
                    {
                        StartCoroutine(GetUpFail());
                        TurnState = CharTurnState.WAIT;
                    }
                    break;
                }
            case (CharTurnState.WIN):
                {
                    break;
                }
            case (CharTurnState.LOSS):
                {
                    break;
                }
        }
    }

    //Fills all of the enemies actions in so they could be chosen
    public void FillActions()
    {
        AvailableActions.Clear();

        if (Stats.AttackAction != null)
            AvailableActions.Add(Stats.AttackAction);

        foreach (BaseBattleAction Action in Stats.GrappleActions)
        {
            if(IsActionPossible(Action))
                AvailableActions.Add(Action);
        }

        foreach (BaseBattleAction Action in Stats.TauntActions)
        {
            if (IsActionPossible(Action))
                AvailableActions.Add(Action);
        }

        foreach (BaseBattleAction Action in BSM.SpecialActions)
        {
            if (IsActionPossible(Action))
                AvailableActions.Add(Action);
        }

        if (Stats.SpecialAction != null)
        {
            if (IsActionPossible(Stats.SpecialAction))
                AvailableActions.Add(Stats.SpecialAction);
        }

        foreach (BaseBattleAction Action in Stats.FinisherActions)
        {
            if (IsActionPossible(Action))
                AvailableActions.Add(Action);
        }
    }

    //Randomly chooses an action from AvailableActions
    public void ChooseAction()
    {
        FillActions();

        BSM.ThisTurn.Actor = this;
        BSM.ThisTurn.Atk_Name = Stats.Name;

        int RNGesus = Random.Range(0, AvailableActions.Count);

        //Check if Possible
        //if (IsActionPossible(AvailableActions[RNGesus]) == true)
        //{
        BSM.ThisTurn.Action = Instantiate(AvailableActions[RNGesus]);
        //}
        //else
        //    ChooseAction();
    }
    
    //Gets the possible targets
    public void GetTargets()
    {
        //Empty Targets
        PossibleTargets.Clear();

        //Fill Possible Targets
        foreach(GameObject Char in BSM.PlayerChars)
            if((BSM.ThisTurn.Action.MustBeDown == true && Char.GetComponent<BaseCharStateMachine>().Status_Down == true) || BSM.ThisTurn.Action.MustBeDown == false)
                PossibleTargets.Add(Char);

        //If Free-For-All match, Fill in EnemyChars other than this one
        if(BSM.Refferee.MatchType == ReffSM.Type.T_THREAT || BSM.Refferee.MatchType == ReffSM.Type.F_FOURWAY)
            foreach(GameObject Char in BSM.EnemyChars)
                if (Char != Wrestler)
                    if ((BSM.ThisTurn.Action.MustBeDown == true && Char.GetComponent<BaseCharStateMachine>().Status_Down == true) || BSM.ThisTurn.Action.MustBeDown == false)
                        PossibleTargets.Add(Char);
    }
    
    //Chooses a target based on Player Characters and Match Type
    public void ChooseTarget()
    {
        int RNGesus;

        //If there is only one player...
        if (PossibleTargets.Count == 1)
        {
            BSM.ThisTurn.Target = PossibleTargets[0].GetComponent<PlayerStateMachine>();
        }
        
        else
        {
            RNGesus = Random.Range(0, PossibleTargets.Count);
            BSM.ThisTurn.Target = PossibleTargets[RNGesus].GetComponent<BaseCharStateMachine>();
        }
    }

    //When is down on their own turn, try to get up.
    public bool TryGetUp()
    {
        Debug.Log(StartWP + ", " + Turns_Down);

        int MaxRNG = 100;
        int GUpChance = Mathf.RoundToInt(((StartWP / Stats.WP.Base) + (Turns_Down * 0.1f)) * 100);

        //Make sure Maximum Chance is 100
        if (GUpChance > MaxRNG) GUpChance = MaxRNG;

        //Try Get Up
        int RNGesus = Random.Range(0, MaxRNG + 1);

        Debug.Log("MaxRNG: " + MaxRNG + ", GUpChance: " + GUpChance + ", RNGesus: " + RNGesus);

        if (RNGesus <= GUpChance)
            return true;
        else
            return false;
    }

    //Try to get up from a pin
    public override bool TryPinEscape(float ActorCount)
    {
        int KickoutChance = Mathf.RoundToInt(((Stats.WP.Current / Stats.WP.Base) + (Turns_Down * 0.10f) + ActorCount) * 100);
        if (KickoutChance > 100) KickoutChance = 100;

        int RNGesus = Random.Range(0, 101);

        Debug.Log("Kickout Chance - " + KickoutChance.ToString("0"));
        Debug.Log("RNG - " + RNGesus.ToString("0"));

        if (RNGesus <= KickoutChance) return true;
        else return false;
    }

    public bool IsActionPossible(BaseBattleAction Action)
    {
        //Check if move is possible. If it is not, Do not add.
        if (Action.MustBeDown == true)
        {
            //Get Possible Targets
            if (BSM.ThisTurn.Action != null)
            {
                GetTargets();
            }
            else
            {
                Debug.Log("There be dragons");
            }

            if (Action.IsDownActionPossible(PossibleTargets) == false)
                return false;
        }

        if (Action.IsCostOfActionPossible(this) == false)
        {
            Debug.Log(Action.ActName + " is not possible. (Cost)");
            return false;
        }
            

        //Check if Action can get character disqualified
        if (Action.Illegal == true && DQ_Strikes >= 2)
        {
            Debug.Log(Action.ActName + " is not possible. (Illegal)");
            return false;
        }
            

        //Is action a finisher?
        if (Action.Finisher == true && Momentum.Current != Momentum.Base)
        {
            Debug.Log(Action.ActName + " is not possible. (Finisher)");
            return false;
        }

        Debug.Log(Action.ActName + " is possible.");
        return true;
    }

    //Replace action in Special Action and AvailableActions
    public override void AddOrReplaceSpecialAction(BaseBattleAction Action)
    {
        if(Stats.SpecialAction != null)
            AvailableActions.Remove(Stats.SpecialAction);

        Stats.SpecialAction = Action;
        AvailableActions.Add(Stats.SpecialAction);
    }
}
