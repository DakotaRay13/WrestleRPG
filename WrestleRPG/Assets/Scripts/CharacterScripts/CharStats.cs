using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharStats : MonoBehaviour
{
    // Personal Info //
    public string Name;
    public int Lvl;
    public bool Face;

    ///////////////////////////////////////////

    // Health Points //
    public Stats HP = new Stats(50f);

    // Will Power //
    public Stats WP = new Stats(50f);

    // Stamina Points //
    public Stats SP = new Stats(30f);

    ///////////////////////////////////////////

    // Attack //
    public Stats ATK = new Stats(10f);

    // Defense //
    public Stats DEF = new Stats(10f);

    // Speed //
    public Stats SPD = new Stats(10f);

    // Charisma //
    public Stats CHR = new Stats(10f);

    // Technical //
    public Stats TECH = new Stats(10f);

    // Luck //
    public Stats LUCK = new Stats(10f);

    ///////////////////////////////////////////

    //Actions will go here
    public BaseBattleAction AttackAction;

    //Grapple Actions
    public List<BaseBattleAction> GrappleActions;

    //Taunt Actions
    public List<BaseBattleAction> TauntActions;

    //Special Action (IE if character brings an Item/Weapon to ring)
    public BaseBattleAction SpecialAction;

    //Finisher
    public List<BaseBattleAction> FinisherActions;
}

public class Stats
{
    public float Base, Current;

    public Stats(float Stat)
    {
        Base = Stat;
        Current = Base;
    }
}
