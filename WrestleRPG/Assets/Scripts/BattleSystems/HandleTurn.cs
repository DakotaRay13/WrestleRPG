using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandleTurn
{
    public string Atk_Name;
    public BaseCharStateMachine Actor;          //Who is ATTACKING

    public string Tgt_Name;
    public BaseCharStateMachine Target;         //Who is the TARGET

    public BaseBattleAction Action;             //What is happening
}
