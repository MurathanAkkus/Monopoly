using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Community Card", menuName = "Monopoly/Cards/Community")]
public class SCR_CommunityCard : ScriptableObject
{
    public string textOnCard; // Açıklama
    public int rewardMoney; // PARA KAZANMA
    public int penaltyMoney; // PARA KAYBETME
    public int moveToBoardIndex = -1;
    public bool collectFromPlayer;
    
    [Header("Jail")]
    public bool goToJail;
    public bool jailFreeCard;

    [Header("Street Repairs")]
    public bool streetRepairs;
    public int streetRepairsHousePrice = 40;
    public int streetRepairsHotelPrice = 115;
}
