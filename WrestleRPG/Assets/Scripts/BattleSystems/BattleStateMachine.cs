using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleStateMachine : MonoBehaviour
{
    // Battle States //
    ///////////////////

    public enum TurnStates
    {
        WAIT,
        WHOSNEXT,
        GETACTION,
        PERFORMACTION,
        REFFCHECK,
        DONE,
        ISDOWNED,
        MATCHFINISHED
    }
    public TurnStates TheTurn;

    // UI Objects //
    ////////////////

    //Manages the Action Panel
    public ActionPanelManager APM;

    //The Character Panels
    public GameObject[] StatsPanels = new GameObject[3];

    //The notification panel on top of the screen
    public GameObject NotificationPanel;

    //Floating text that shows the damage
    public GameObject FloatingText;

    // Player and Enemy State Machines //
    /////////////////////////////////////
    public List<GameObject> PlayerChars = new List<GameObject>();
    public List<GameObject> EnemyChars  = new List<GameObject>();

    public List<BaseCharStateMachine> AllChars = new List<BaseCharStateMachine>();

    // Turn Queue //
    ////////////////

    public Queue<BaseCharStateMachine> TurnQueue = new Queue<BaseCharStateMachine>();
    public BaseCharStateMachine CurrentChar;
    const float TIMER = 200f;

    // This Turn (to be reset at the end of the turn) //

    public HandleTurn ThisTurn;

    // Refferee //
    public ReffSM Refferee;
    public GameObject ReffObj;

    public List<BaseBattleAction> SpecialActions = new List<BaseBattleAction>();

    //Reason for MatchFinish
    public string WinReason;

    /**********************************************************************************************************/

    //Put BattleState Machine Constructor here with info inside

    /**********************************************************************************************************/
    // Use this for initialization
    void Start()
    {
        //Initialize States
        TheTurn = TurnStates.WHOSNEXT;

        ReffObj = Instantiate(ReffObj);
        Refferee = ReffObj.GetComponent<ReffSM>();

        //Load Match Rules
        Refferee.LoadRulesAtStart(this);

        //Assign Character Positions
        AssignPositions();

        //Assign State Machines
        AssignStateMachines();

        //Add State Machines to list
        AddSMsToList();

        //Set up UI
        AssignStatPanels();
    }

    // Update is called once per frame
    void Update()
    {
        switch (TheTurn)
        {
            case (TurnStates.WAIT):
                {
                    //idle state
                    break;
                }
            case (TurnStates.WHOSNEXT):
                {
                    //Disable NotePanel
                    DeactivateNote();

                    CurrentChar = GetNextTurn();

                    if (CurrentChar.Status_Down == true)
                        TheTurn = TurnStates.ISDOWNED;
                    else TheTurn = TurnStates.GETACTION;
                    
                    break;
                }
            case (TurnStates.GETACTION):
                {
                    CurrentChar.TurnState = BaseCharStateMachine.CharTurnState.GETACTION;
                    TheTurn = TurnStates.WAIT;
                    break;
                }
            case (TurnStates.PERFORMACTION):
                {
                    //Take cost from currentchar once
                    if(ThisTurn.Action.ActionStarted == false)
                    {
                        CurrentChar.TakeCostOfAction(ThisTurn.Action);
                        DisplayNote();
                    }
                    
                    // Perform Chosen Action
                    StartCoroutine(ThisTurn.Action.PerformAction(ThisTurn.Actor, ThisTurn.Target));
                    
                    break;
                }
            case (TurnStates.REFFCHECK):
                {
                    CheckForDQ();
                    break;
                }
            case (TurnStates.DONE):
                {
                    // If Character was Downed this turn, change their state and CurrentTurn gets another turn
                    if(ThisTurn.Target.Status_Down == false && ThisTurn.Target.Stats.HP.Current == 0f)
                    {
                        DisplayNote(ThisTurn.Target.Stats.Name + " is Down!");

                        ThisTurn.Target.Status_Down = true;
                        if (ThisTurn.Target.Status_Defend == true) ThisTurn.Target.Status_Defend = false;
                        ThisTurn.Target.StartWP = ThisTurn.Target.Stats.WP.Current;
                        TheTurn = TurnStates.GETACTION;
                    }

                    // Else, End turn
                    else TheTurn = TurnStates.WHOSNEXT;

                    Destroy(GameObject.Find(ThisTurn.Action.name));
                    break;
                }
            case (TurnStates.ISDOWNED):
                {
                    // This character tries to get up.
                    CurrentChar.TurnState = BaseCharStateMachine.CharTurnState.DOWN;

                    //Wait for update from Character State Maching
                    TheTurn = TurnStates.WAIT;
                    break;
                }

            case (TurnStates.MATCHFINISHED):
                {
                    //For now, just check who won. Add character specific stuff later.
                    bool pWin = false;

                    foreach(BaseCharStateMachine Character in AllChars)
                    {
                        if (Character.TurnState == BaseCharStateMachine.CharTurnState.WIN && Character.type == "PLAYER")
                        {
                            pWin = true;
                        }
                    }

                    //Create Win Message
                    string WinMessage = PlayerChars[0].GetComponent<PlayerStateMachine>().Stats.Name;

                    for(int x = 1; x < PlayerChars.Count; x++)
                    {
                        if(x < PlayerChars.Count - 1)
                            WinMessage = WinMessage +  " and " + PlayerChars[x].GetComponent<PlayerStateMachine>().Stats.Name;
                        else
                            WinMessage = WinMessage + ", " + PlayerChars[x].GetComponent<PlayerStateMachine>().Stats.Name;
                    }

                    //If the player won, have that appear
                    if (pWin)
                    {
                        WinMessage += " Win";
                    }
                    else WinMessage += " Lose";

                    //Display Win Message
                    DisplayNote(WinMessage + " by " + WinReason + "!");

                    //Wait for scene transition
                    StartCoroutine(EndOfMatch());
                    TheTurn = TurnStates.WAIT;

                    break;
                }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////

    //Assign state machines to the characters
    void AssignStateMachines()
    {
        foreach(GameObject Char in PlayerChars)
        {
            Char.tag = "Player";
            Char.AddComponent<PlayerStateMachine>();
            Char.GetComponent<PlayerStateMachine>().Wrestler = Char;
            Char.GetComponent<PlayerStateMachine>().BSM = this;
            Char.GetComponent<PlayerStateMachine>().startPosition = Char.transform.position;
            Char.GetComponent<PlayerStateMachine>().Stats = Char.GetComponent<CharStats>();
        }

        foreach(GameObject Char in EnemyChars)
        {
            Char.tag = "Enemy";
            Char.AddComponent<EnemyStateMachine>();
            Char.GetComponent<EnemyStateMachine>().Wrestler = Char;
            Char.GetComponent<EnemyStateMachine>().BSM = this;
            Char.GetComponent<EnemyStateMachine>().startPosition = Char.transform.position;
            Char.GetComponent<EnemyStateMachine>().Stats = Char.GetComponent<CharStats>();
        }
    }
    
    //Assign each character to their positions
    void AssignPositions()
    {
        // Possible Character Positions //
        //////////////////////////////////

        Vector3 Position1 = new Vector3(-3.24f, 0f, -3.24f);
        Vector3 Position2 = new Vector3(3.24f, 0f, 3.24f);
        Vector3 Position3 = new Vector3(-3.24f, 0f, 3.24f);
        Vector3 Position4 = new Vector3(3.24f, 0f, -3.24f);
        Vector3 ReffereePos = new Vector3(0f, 0f, 5.96f);

        Vector3[] Positions = new Vector3[4];

        // Position Map //
        /////////
        //  R  //
        // 3 2 //
        // 1 4 //
        /////////
        
        //Arrange Characters by Match Type
        if(Refferee.MatchType == ReffSM.Type.SINGLE)
        {
            //Player Positions
            PlayerChars[0].transform.position = Position1;

            //Enemy Positions
            EnemyChars[0].transform.position = Position2;

            //Refferee Position
            ReffObj.transform.position = Position3;
        }
        else if (Refferee.MatchType == ReffSM.Type.TAG)
        {
            //Player Positions
            PlayerChars[0].transform.position = Position1;
            PlayerChars[1].transform.position = Position3;

            //Enemy Positions
            EnemyChars[0].transform.position = Position2;
            EnemyChars[1].transform.position = Position4;

            //Refferee Position
            ReffObj.transform.position = ReffereePos;
        }
        else if (Refferee.MatchType == ReffSM.Type.T_THREAT || Refferee.MatchType == ReffSM.Type.H_1V2)
        {
            //Player Positions
            PlayerChars[0].transform.position = Position1;

            //Enemy Positions
            EnemyChars[0].transform.position = Position2;
            EnemyChars[1].transform.position = Position4;

            //Refferee Position
            ReffObj.transform.position = Position3;
        }
        else if (Refferee.MatchType == ReffSM.Type.F_FOURWAY || Refferee.MatchType == ReffSM.Type.H_1V3)
        {
            //Player Positions
            PlayerChars[0].transform.position = Position1;

            //Enemy Positions
            EnemyChars[0].transform.position = Position2;
            EnemyChars[1].transform.position = Position3;
            EnemyChars[2].transform.position = Position4;

            //Refferee Position
            ReffObj.transform.position = ReffereePos;
        }
        else if (Refferee.MatchType == ReffSM.Type.H_2V1)
        {
            //Player Positions
            PlayerChars[0].transform.position = Position1;
            PlayerChars[1].transform.position = Position3;

            //Enemy Positions
            EnemyChars[0].transform.position = Position2;

            //Refferee Position
            ReffObj.transform.position = Position4;
        }
        else if (Refferee.MatchType == ReffSM.Type.H_3V1)
        {
            //Player Positions
            PlayerChars[0].transform.position = Position1;
            PlayerChars[1].transform.position = Position3;
            PlayerChars[2].transform.position = Position4;

            //Enemy Positions
            EnemyChars[0].transform.position = Position2;

            //Refferee Position
            ReffObj.transform.position = ReffereePos;
        }
    }

    //Add the state machines to each list
    void AddSMsToList()
    {
        foreach (GameObject Char in PlayerChars)
            AllChars.Add(Char.GetComponent<PlayerStateMachine>());
        foreach (GameObject Char in EnemyChars)
            AllChars.Add(Char.GetComponent<EnemyStateMachine>());
    }

    //Assign the Stats Panels to Player Chars
    public void AssignStatPanels()
    {
        for(int i = 0; i < PlayerChars.Count; i++)
        {
            //Turn on the Stat Panel
            StatsPanels[i].SetActive(true);

            PlayerChars[i].GetComponent<PlayerStateMachine>().PanelStats = StatsPanels[i].GetComponent<CharPanelStats>();
            PlayerChars[i].GetComponent<PlayerStateMachine>().PanelStats.GetComponent<CharPanelStats>().PlayerName.text = PlayerChars[i].GetComponent<PlayerStateMachine>().Stats.Name;
            PlayerChars[i].GetComponent<PlayerStateMachine>().UpdateStatsPanel();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////
    //Action Menu Functions
    
    public void ClickAttackButton()
    {
        //If action was set to true prematurly, set to false
        CurrentChar.Stats.AttackAction.CheckIfTrueError();

        //Set to do the Attack Action
        ThisTurn.Action = Instantiate(CurrentChar.Stats.AttackAction);
        ThisTurn.Actor = CurrentChar;
        ThisTurn.Atk_Name = CurrentChar.Stats.Name;
        CurrentChar.TurnState = BaseCharStateMachine.CharTurnState.CHOOSETARGET;
    }

    public void ClickGrappleButton()
    {
        APM.ActivateActMenu();
        APM.FillActMenu(CurrentChar.Stats.GrappleActions);
    }

    public void ClickTauntButton()
    {
        APM.ActivateActMenu();
        APM.FillActMenu(CurrentChar.Stats.TauntActions);
    }

    public void ClickDefendButton()
    {
        //Turn on the defend status
        CurrentChar.Status_Defend = true;

        //Deactivate the UI
        APM.DeactivateAllPanel();

        //Move Target Arrow
        PlayerChars[0].GetComponent<PlayerStateMachine>().TargetArrow.transform.position = new Vector3(0, -5f, 0);

        //Immediatly go on to next turn
        TheTurn = TurnStates.WHOSNEXT;
    }

    public void ClickSpecialButton()
    {
        //Add character's action to the menu
        if(CurrentChar.Stats.SpecialAction != null)
            SpecialActions.Add(CurrentChar.Stats.SpecialAction);

        //Activate and fill menu
        APM.ActivateActMenu();
        APM.FillActMenu(SpecialActions);

        //Remove character's action from SpecialActions
        if (CurrentChar.Stats.SpecialAction != null)
            SpecialActions.Remove(CurrentChar.Stats.SpecialAction);
    }

    public void ClickFinishButton()
    {
        APM.ActivateActMenu();
        APM.FillActMenu(CurrentChar.Stats.FinisherActions);
    }

    //Function for canceling turn during Choose Target
    public void CancelPlayerAction()
    {
        Debug.Log("Cancel Pressed");

        //Check if PlayerChar
        if(CurrentChar.type == "PLAYER")
        {
            //Is player choosing a target?
            if(CurrentChar.TurnState == BaseCharStateMachine.CharTurnState.CHOOSETARGET)
            {
                //Set player to GetAction
                CurrentChar.TurnState = BaseCharStateMachine.CharTurnState.GETACTION;
                Debug.Log("Player Action Canceled");
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////

    //If the Turn Queue is empty, fill up the Turn Queue
    BaseCharStateMachine GetNextTurn()
    {
        while (TurnQueue.Count <= 0)
        {
            foreach (BaseCharStateMachine SM in AllChars)
            {
                SM.ASC += SM.Stats.SPD.Current;

                if (SM.ASC >= TIMER)
                {
                    SM.ASC = 0;
                    TurnQueue.Enqueue(SM);
                }
            }
        }

        return TurnQueue.Dequeue();
    }

    ///////////////////////////////////////////////////////////////////////////////////////

    //Display Notifications
    public void DisplayNote()
    {
        NotificationPanel.SetActive(true);

        NotificationPanel.GetComponentInChildren<Text>().text = ThisTurn.Atk_Name + " hits " + ThisTurn.Tgt_Name + " with a " + ThisTurn.Action.ActName;
    }

    //Display Specific Notifications
    public void DisplayNote(string Message)
    {
        NotificationPanel.SetActive(true);

        NotificationPanel.GetComponentInChildren<Text>().text = Message;
    }

    //Deactivates the Notification Panel
    public void DeactivateNote()
    {
        NotificationPanel.SetActive(false);
    }

    ///////////////////////////////////////////////////////////////////////////////////////

    public void CheckForDQ()
    {
        //Check if match has DQs
        if(Refferee.DQ == true)
        {
            //Check if Refferee is down
            if (Refferee.Down == false)
            {
                StartCoroutine(DQStrikeChar());
                TheTurn = TurnStates.WAIT;
            }
            else TheTurn = TurnStates.DONE;
        }
        else TheTurn = TurnStates.DONE;
    }

    public IEnumerator DQStrikeChar()
    {
        //Incrament strikes
        CurrentChar.DQ_Strikes++;

        //Write Note
        string ReffMessage = CurrentChar.Stats.Name + "! That is strike " + CurrentChar.DQ_Strikes.ToString() + "!";

        //If Strikes = 3, Ring the Bell
        if(CurrentChar.DQ_Strikes >= 3)
        {
            ReffMessage += " Ring the Bell!";
        }

        //Display Note
        DisplayNote(ReffMessage);

        yield return new WaitForSeconds(2);

        if (CurrentChar.DQ_Strikes >= 3)
        {
            FinishMatch(ThisTurn.Target, ThisTurn.Actor, "Disqualification");
        }
        else
        {
            TheTurn = TurnStates.DONE;
        }
    }

    //Function for finishing the match
    public void FinishMatch(BaseCharStateMachine Winner, BaseCharStateMachine Loser, string WinReason)
    {
        Winner.TurnState = BaseCharStateMachine.CharTurnState.WIN;
        Loser.TurnState = BaseCharStateMachine.CharTurnState.LOSS;
        TheTurn = TurnStates.MATCHFINISHED;
        this.WinReason = WinReason;
    }

    public IEnumerator EndOfMatch()
    {
        yield return new WaitForSeconds(10f);

        SceneManager.LoadScene("tst_Finished");
    }
}