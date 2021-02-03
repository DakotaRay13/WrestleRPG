using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReffSM : MonoBehaviour
{
    //Match Rules
    public enum Type
    {
        SINGLE,
        TAG,
        T_THREAT,
        F_FOURWAY,

        //Handicap, Player Disadvantage
        H_1V2,
        H_1V3,

        //Handicap, Player Advantage
        H_2V1,
        H_3V1
    }
    public Type MatchType;
    public bool DQ = true;

    //Refferee State
    public bool Down = false;

    //List of Special Moves for each match type
    public List<BaseBattleAction> SingleActions;
    public List<BaseBattleAction> TagActions;
    public List<BaseBattleAction> TThreatActions;
    public List<BaseBattleAction> FourWayActions;
    public List<BaseBattleAction> H1v2Actions;
    public List<BaseBattleAction> H1v3Actions;
    public List<BaseBattleAction> H2v1Actions;
    public List<BaseBattleAction> H3v1Actions;

    //Load the Characters into here
    public List<GameObject> PlayerChars;
    public List<GameObject> EnemyChars;

    //Default Characters if None in Lists
    public GameObject DefaultPlayerChar;
    public GameObject DefaultEnemyChar;

    ///////////////////////////////////////////////////////////////////////////////////////////

    //Set up the Rules for the match
    public void LoadRulesAtStart(BattleStateMachine BSM)
    {
        switch(MatchType)
        {
            case (Type.SINGLE):
                {
                    DQ = true;
                    NoCharCheck(1, 1);
                    break;
                }
            case (Type.TAG):
                {
                    DQ = true;
                    NoCharCheck(2, 2);
                    break;
                }
            case (Type.T_THREAT):
                {
                    DQ = false;
                    NoCharCheck(1, 2);
                    break;
                }
            case (Type.F_FOURWAY):
                {
                    DQ = false;
                    NoCharCheck(1, 3);
                    break;
                }
            case (Type.H_1V2):
                {
                    DQ = true;
                    NoCharCheck(1, 2);
                    break;
                }
            case (Type.H_1V3):
                {
                    DQ = true;
                    NoCharCheck(1, 3);
                    break;
                }
            case (Type.H_2V1):
                {
                    DQ = true;
                    NoCharCheck(2, 1);
                    break;
                }
            case (Type.H_3V1):
                {
                    DQ = true;
                    NoCharCheck(3, 1);
                    break;
                }
            default:
                {
                    Debug.Log("ReffSM.MatchType did not have a match type. \nSingles Match Loaded.");
                    DQ = true;
                    NoCharCheck(1, 1);
                    break;
                }
        }

        //Load Special Actions into BSM
        BSM.SpecialActions = GetSpecialActions();

        //Load Characters
        foreach(GameObject Char in PlayerChars)
        {
            BSM.PlayerChars.Add(Instantiate(Char));
        }

        foreach (GameObject Char in EnemyChars)
        {
            BSM.EnemyChars.Add(Instantiate(Char));
        }
    }

    //Assign Special Actions to the Battle State Machine
    public List<BaseBattleAction> GetSpecialActions()
    {
        switch(MatchType)
        {
            case (Type.SINGLE):
                {
                    return SingleActions;
                }
            case (Type.TAG):
                {
                    return TagActions;
                }
            case (Type.T_THREAT):
                {
                    return TThreatActions;
                }
            case (Type.F_FOURWAY):
                {
                    return FourWayActions;
                }
            case (Type.H_1V2):
                {
                    return H1v2Actions;
                }
            case (Type.H_1V3):
                {
                    return H1v3Actions;
                }
            case (Type.H_2V1):
                {
                    return H2v1Actions;
                }
            case (Type.H_3V1):
                {
                    return H3v1Actions;
                }
            default:
                {
                    Debug.Log("ReffSM.GetSpecialActions did not have a match type. \nSingles Match Actions Loaded.");
                    return SingleActions;
                }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////

    //Save characters
    public void SaveCharacters(GameObject inChar, List<GameObject> outChars)
    {
        outChars.Add(inChar);
    }

    public void ClearCharacters()
    {
        PlayerChars.Clear();
        EnemyChars.Clear();
    }
 
    ///////////////////////////////////////////////////////////////////////////////////////////
    
    //Check for no character error
    public void NoCharCheck(int player, int enemy)
    {
        if (PlayerChars.Count == 0)
        {
            for(int i = 0; i < player; i++)
            {
                PlayerChars.Add(DefaultPlayerChar);
            }
        }

        if(EnemyChars.Count == 0)
        {
            for(int i = 0; i < player; i++)
            {
                EnemyChars.Add(DefaultEnemyChar);
            }
        }
    }
}