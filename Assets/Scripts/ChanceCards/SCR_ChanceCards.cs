using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Chance Card", menuName = "Monopoly/Cards/Chance")]
public class SCR_ChanceCards : ScriptableObject
{
    public string textOnCard; // Kart Açıklaması
    public int rewardMoney; // PARA KAZANMA
    public int penaltyMoney; // PARA KAYBETME
    public int moveToBoardIndex = -1;
    public bool payToPlayer;

    [Header("MoveToLocations")]
    public bool nextRailroad;
    public bool nextUtility;
    public int moveStepsBackwards;
    
    [Header("Jail")]
    public bool goToJail;
    public bool jailFreeCard;

    [Header("Street Repairs")]
    public bool streetRepairs;
    public int streetRepairsHousePrice = 25;
    public int streetRepairsHotelPrice = 100;
}
