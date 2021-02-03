using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : BaseCharStateMachine
{
    public CharPanelStats PanelStats;
    public GameObject TargetArrow;

    int currentTarget = 0;

    // Use this for initialization
    void Start()
    {
        type = "PLAYER";

        TargetArrow = GameObject.Find("tst_TargetArrow");

        //Initialize state
        TurnState = CharTurnState.WAIT;

        //Reset Momentum
        ResetMomentum();
    }

    // Update is called once per frame
    void Update()
    {
        //What this character is doing now
        switch (TurnState)
        {
            case (CharTurnState.WAIT):
                {
                    //Idle State
                    break;
                }
            case (CharTurnState.GETACTION):
                {
                    TargetArrow.transform.position = new Vector3(0, -5, 0);

                    //Place Target Arrow over This Character
                    if (BSM.PlayerChars.Count > 1)
                    {
                        TargetArrow.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 2, this.transform.position.z);
                    }
                    //Make sure Status_Defend is off
                    if(Status_Defend == true) Status_Defend = false;

                    //Activate Panel and wait. Later, use this to fill Grapple and Special Menus
                    BSM.APM.ActivateActionPanel();

                    TurnState = CharTurnState.WAIT;
                    break;
                }
            case (CharTurnState.CHOOSETARGET):
                {
                    BSM.APM.DeactivateAllPanel();
                    ChooseTarget();
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

        //Check for Updated UI
        UpdateStatsPanel();
    }
    
    // Choose A Target //
    /////////////////////
    public void ChooseTarget()
    {
        //If Action targets self, Move on
        if(BSM.ThisTurn.Action.TargetSelf == true)
        {
            TargetArrow.transform.position = new Vector3(0, -5, 0);
            BSM.ThisTurn.Target = this;
            BSM.ThisTurn.Tgt_Name = this.Stats.Name;
            BSM.TheTurn = BattleStateMachine.TurnStates.PERFORMACTION;
            TurnState = CharTurnState.WAIT;
        }
        // For when there is only one enemy
        else if (BSM.EnemyChars.Count == 1)
        {
            TargetArrow.transform.position = new Vector3(0, -5, 0);
            BSM.ThisTurn.Target = BSM.EnemyChars[0].GetComponent<EnemyStateMachine>();
            BSM.ThisTurn.Tgt_Name = BSM.ThisTurn.Target.Stats.Name;
            BSM.TheTurn = BattleStateMachine.TurnStates.PERFORMACTION;
            TurnState = CharTurnState.WAIT;
        }
        else
        {
            //Temporary -> Use Mouse to choose.
            //Goal      -> Use Directional Buttons to choose and have camera look at Target
            RaycastHit Selector;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If target must be down and currentTarget is Standing, Swap Targets
            while(BSM.ThisTurn.Action.MustBeDown == true && BSM.EnemyChars[currentTarget].GetComponent<BaseCharStateMachine>().Status_Down == false)
            {
                currentTarget++;
            }

            TargetArrow.transform.position = new Vector3(BSM.EnemyChars[currentTarget].transform.position.x, 
                BSM.EnemyChars[currentTarget].transform.position.y + 2, BSM.EnemyChars[currentTarget].transform.position.z);

            if (Physics.Raycast(ray, out Selector, 100.0f))
            {
                for (int i = 0; i < BSM.EnemyChars.Count; i++)
                    if (Selector.transform.gameObject == BSM.EnemyChars[i])
                    {
                        if (((BSM.ThisTurn.Action.MustBeDown == true && BSM.EnemyChars[i].GetComponent<BaseCharStateMachine>().Status_Down == true) ||
                            BSM.ThisTurn.Action.MustBeDown == false))
                        {
                            currentTarget = i;
                            TargetArrow.transform.position = new Vector3(BSM.EnemyChars[currentTarget].transform.position.x,
                                BSM.EnemyChars[currentTarget].transform.position.y + 2, BSM.EnemyChars[currentTarget].transform.position.z);
                        }
                    }
            }

            // confirm target
            if (Input.GetMouseButtonDown(0))
            {
                TargetArrow.transform.position = new Vector3(0, -5, 0);
                BSM.ThisTurn.Target = BSM.EnemyChars[currentTarget].GetComponent<EnemyStateMachine>();
                BSM.ThisTurn.Tgt_Name = BSM.ThisTurn.Target.Stats.Name;
                BSM.TheTurn = BattleStateMachine.TurnStates.PERFORMACTION;
                TurnState = CharTurnState.WAIT;
            }
        }
    }

    //REPLACE THIS LATER WHEN KEY CONTROLS ARE IMPLAMENTED//
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

    //Try to get up from a pin [REPLACE WITH QTE LATER]
    public override bool TryPinEscape(float ActorCount)
    {
        int KickoutChance = Mathf.RoundToInt(((Stats.WP.Current / Stats.WP.Base) + (Turns_Down * 0.10f) + ActorCount) * 100);
        if (KickoutChance > 100) KickoutChance = 100;

        int RNGesus = Random.Range(0, 101);

        Debug.Log("Kickout Chance - " + KickoutChance);
        Debug.Log("RNG - " + RNGesus);
        
        if (RNGesus <= KickoutChance) return true;
        else return false;
    }

    //Check if Stat Panel needs to be updated. If stat changes, show in UI
    public void UpdateStatsPanel()
    {
        //Check HP
        if(PanelStats.PlayerHP.text != Stats.HP.Current.ToString() + "/" + Stats.HP.Base.ToString())
        {
            //Update HP text and bar
            PanelStats.PlayerHP.text = Stats.HP.Current.ToString() + "/" + Stats.HP.Base.ToString();
            PanelStats.PlayerHPBar.transform.localScale = new Vector3(Mathf.Clamp(Stats.HP.Current / Stats.HP.Base, 0, 1), 
                PanelStats.PlayerHPBar.transform.localScale.y, PanelStats.PlayerHPBar.transform.localScale.z);
        }

        //Check WP
        if (PanelStats.PlayerWP.text != Stats.WP.Current.ToString() + "/" + Stats.WP.Base.ToString())
        {
            //Update WP text and bar
            PanelStats.PlayerWP.text = Stats.WP.Current.ToString() + "/" + Stats.WP.Base.ToString();
            PanelStats.PlayerWPBar.transform.localScale = new Vector3(Mathf.Clamp(Stats.WP.Current / Stats.WP.Base, 0, 1), 
                PanelStats.PlayerWPBar.transform.localScale.y, PanelStats.PlayerWPBar.transform.localScale.z);
        }

        //Check SP
        if (PanelStats.PlayerSP.text != Stats.SP.Current.ToString() + "/" + Stats.SP.Base.ToString())
        {
            //Update SP text and bar
            PanelStats.PlayerSP.text = Stats.SP.Current.ToString() + "/" + Stats.SP.Base.ToString();
            PanelStats.PlayerSPBar.transform.localScale = new Vector3(Mathf.Clamp(Stats.SP.Current / Stats.SP.Base, 0, 1), 
                PanelStats.PlayerSPBar.transform.localScale.y, PanelStats.PlayerSPBar.transform.localScale.z);
        }

        //Check Momentum
        if(PanelStats.MomentumBar.transform.localScale != new Vector3(Mathf.Clamp(Momentum.Current / Momentum.Base, 0, 1),
                PanelStats.MomentumBar.transform.localScale.y, PanelStats.MomentumBar.transform.localScale.z))
        {
            PanelStats.MomentumBar.transform.localScale = new Vector3(Mathf.Clamp(Momentum.Current / Momentum.Base, 0, 1),
                PanelStats.MomentumBar.transform.localScale.y, PanelStats.MomentumBar.transform.localScale.z);
        }

        //Check that the color is white when not full
        if (Momentum.Current != Momentum.Base && PanelStats.MomentumBar.color != Color.white)
            PanelStats.MomentumBar.color = Color.white;

        //Check that the color is red when full
        if (Momentum.Current == Momentum.Base && PanelStats.MomentumBar.color != Color.red)
            PanelStats.MomentumBar.color = Color.red;

        
    }

    public override void AddOrReplaceSpecialAction(BaseBattleAction Action)
    {
        Stats.SpecialAction = Action;
    }
}
