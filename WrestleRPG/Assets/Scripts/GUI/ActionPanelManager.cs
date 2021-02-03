using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPanelManager : MonoBehaviour
{
    //Primary Panels
    //Mainly use these for activating/deactivating diffrent parts.
    public GameObject ActionPanel;
    public GameObject ActMenuPanel;

    //Action Panel Buttons
    //These are set in stone
    public GameObject[] ActionPanelOptions = new GameObject[6];

    //Act Menu Buttons
    //These will be filled in each turn
    public GameObject ButtonPrefab;
    public List<GameObject> ActButtons = new List<GameObject>();

    //Info Panel contents
    public Text Info;

    public void Start()
    {
        DeactivateAllPanel();
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    //Activate the Action Panel

    public void ActivateActionPanel()
    {
        ActionPanel.SetActive(true);

        //Set Attack Button as active
        ActionPanel.transform.GetChild(0).gameObject.GetComponent<Button>().Select();

        //Deactivate Grapple, Taunt, Special, and Finisher as needed (Attack and Defend should ALWAYS have an action
        CharStats Character = GameObject.Find("BattleStateMachine").GetComponent<BattleStateMachine>().CurrentChar.Stats;

        //Grapple Button
        if (Character.GrappleActions.Count <= 0)
            DeactivateButton(ActionPanel.transform.GetChild(1).gameObject);
        else ActivateButton(ActionPanel.transform.GetChild(1).gameObject);

        //Taunt Button
        if (Character.TauntActions.Count <= 0)
            DeactivateButton(ActionPanel.transform.GetChild(2).gameObject);
        else ActivateButton(ActionPanel.transform.GetChild(2).gameObject);

        //Special Button
        if (GameObject.Find("BattleStateMachine").GetComponent<BattleStateMachine>().SpecialActions.Count <= 0)
            DeactivateButton(ActionPanel.transform.GetChild(4).gameObject);
        else ActivateButton(ActionPanel.transform.GetChild(4).gameObject);

        //Finisher Button
        if (Character.FinisherActions.Count <= 0 || 
            GameObject.Find("BattleStateMachine").GetComponent<BattleStateMachine>().CurrentChar.Momentum.Current 
            < GameObject.Find("BattleStateMachine").GetComponent<BattleStateMachine>().CurrentChar.Momentum.Base)
        {
            DeactivateButton(ActionPanel.transform.GetChild(5).gameObject);
        }
        else ActivateButton(ActionPanel.transform.GetChild(5).gameObject);
    }

    //Activate the Act Menu Panel

    public void ActivateActMenu()
    {
        ActMenuPanel.SetActive(true);
    }

    //Deactivate the Action Panel and ActMenuPanel

    public void DeactivateAllPanel()
    {
        ActionPanel.SetActive(false);
        ActMenuPanel.SetActive(false);
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    //Fill the action menu with the accompanying character actions
    public void FillActMenu(List<BaseBattleAction> Actions)
    {
        //First, empty the List
        EmptyActMenu();

        //If actions are empty, disable menu. Else, continue
        if (Actions.Count <= 0) ActMenuPanel.SetActive(false);
        else
        {
            //Fill the buttons
            int i = 0;
            foreach (BaseBattleAction Action in Actions)
            {
                //If action was set to true prematurly, set to false
                Action.CheckIfTrueError();

                //Instantiate the Button Prefab
                ActButtons.Add(Instantiate(ButtonPrefab, ActMenuPanel.transform));

                //Fill the Text Areas
                ActButtons[i].transform.GetChild(0).gameObject.GetComponent<Text>().text = Action.ActName;

                if (Action.ActCost <= 0f) ActButtons[i].transform.GetChild(1).gameObject.GetComponent<Text>().text = "";
                else
                {
                    ActButtons[i].transform.GetChild(1).gameObject.GetComponent<Text>().text = Action.ActCost.ToString();
                    if (Action.CostsHP == true) ActButtons[i].transform.GetChild(1).gameObject.GetComponent<Text>().text += " HP";
                    else ActButtons[i].transform.GetChild(1).gameObject.GetComponent<Text>().text += " SP";
                }
                
                //Set the Button To perform the action
                ActButtons[i].GetComponent<Button>().onClick.AddListener(delegate { setAction(Instantiate(Action)); });

                //Check if move is possible. If it is not, deactivate button.
                if (Action.MustBeDown == true)
                    if (Action.IsDownActionPossible(GameObject.Find("BattleStateMachine").GetComponent<BattleStateMachine>().EnemyChars) == false)
                        DeactivateActButton(ActButtons[i]);

                if(Action.IsCostOfActionPossible(GameObject.Find("BattleStateMachine").GetComponent<BattleStateMachine>().CurrentChar) == false)
                    DeactivateActButton(ActButtons[i]);

                i++;
            }
        }
    }

    //Empty the Action Menu
    public void EmptyActMenu()
    {
        ActButtons.Clear();

        //Destroy All buttons in ActMenuPanel
        foreach(Transform Child in ActMenuPanel.transform)
        {
            GameObject.Destroy(Child.gameObject);
        }
    }

    //When action selected from menu, set the action
    void setAction(BaseBattleAction Action)
    {
        BattleStateMachine BSM = GameObject.Find("BattleStateMachine").GetComponent<BattleStateMachine>();
        
        //Set ThisTurn Action
        BSM.ThisTurn.Actor = BSM.CurrentChar;
        BSM.ThisTurn.Atk_Name = BSM.CurrentChar.Stats.Name;
        BSM.ThisTurn.Action = Action;

        //Choose Target
        BSM.CurrentChar.TurnState = BaseCharStateMachine.CharTurnState.CHOOSETARGET;
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public void ActivateButton(GameObject X)
    {
        X.GetComponent<Button>().interactable = true;
        X.transform.GetChild(0).gameObject.GetComponent<Text>().color = Color.white;
    }
    public void DeactivateButton(GameObject X)
    {
        X.GetComponent<Button>().interactable = false;
        X.transform.GetChild(0).gameObject.GetComponent<Text>().color = Color.gray;
    }
    public void DeactivateActButton(GameObject X)
    {
        X.GetComponent<Button>().interactable = false;
        X.transform.GetChild(0).gameObject.GetComponent<Text>().color = Color.gray;
        X.transform.GetChild(1).gameObject.GetComponent<Text>().color = Color.gray;
    }
}
