using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class tst_MainMenu : MonoBehaviour
{
    public Dropdown MatchSelect;
    public ReffSM Refferee;
    public GameObject HTPText;
    public GameObject ChangeLogText;

    //Character List (Choose or Generate Characters Later)
    public GameObject tst_PlayerChar;
    public GameObject tst_EnemyChar;

    public void Awake()
    {
        MatchSelect = GameObject.Find("MatchTypeMenu").GetComponent<Dropdown>();
        HTPText = GameObject.Find("HTPText");
        ChangeLogText = GameObject.Find("ChangeLogText");
        HTPText.SetActive(false);
        ChangeLogText.SetActive(false);
    }

    public void ShowHTP()
    {
        //Deactivate ChangeLog
        if(ChangeLogText.activeSelf == true)
        {
            ChangeLogText.SetActive(false);
        }

        //Activate HTPText
        if (HTPText.activeSelf == false)
        {
            HTPText.SetActive(true);
        }
    }

    public void ShowChangeLog()
    {
        //Deactivate HTPText
        if (HTPText.activeSelf == true)
        {
            HTPText.SetActive(false);
        }

        //Activate ChangeLog
        if (ChangeLogText.activeSelf == false)
        {
            ChangeLogText.SetActive(true);
        }
    }

    //Function to start the match
    public void StartMatch()
    {
        //Clear previous Characters from Refferee
        Refferee.ClearCharacters();

        //Set Match Type from Dropdown
        switch (MatchSelect.value)
        {
            case (0): //Single
                {
                    Refferee.MatchType = ReffSM.Type.SINGLE;

                    //Save Chars to Refferee
                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }
            case (1): //Tag
                {
                    Refferee.MatchType = ReffSM.Type.TAG;

                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }
            case (2): //Triple Threat
                {
                    Refferee.MatchType = ReffSM.Type.T_THREAT;

                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }
            case (3): //Fatal Four Way
                {
                    Refferee.MatchType = ReffSM.Type.F_FOURWAY;

                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }
            case (4): //Handicap 1 v 2
                {
                    Refferee.MatchType = ReffSM.Type.H_1V2;

                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }
            case (5): //Handicap 1 v 3
                {
                    Refferee.MatchType = ReffSM.Type.H_1V3;

                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }
            case (6): //Handicap 2 v 1
                {
                    Refferee.MatchType = ReffSM.Type.H_2V1;

                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }
            case (7): //Handicap 3 v 1
                {
                    Refferee.MatchType = ReffSM.Type.H_3V1;

                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }
            default:
                {
                    Debug.Log("Match Select did not have a match type. \nSingles Match Loaded.");
                    Refferee.MatchType = ReffSM.Type.SINGLE;

                    //Save Chars to Refferee
                    Refferee.SaveCharacters(tst_PlayerChar, Refferee.PlayerChars);
                    Refferee.SaveCharacters(tst_EnemyChar, Refferee.EnemyChars);
                    break;
                }

        }

        //Load the tst_BattleSystem scene
        SceneManager.LoadScene("tst_Battle");
    }

    //Function to exit the game
    public void ExitGame()
    {
        Application.Quit();
    }
}
