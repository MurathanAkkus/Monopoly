using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public enum PlayerType
    {
        HUMAN,
        AI
    }

    // HUMAN
    public PlayerType playerType;
    public string name;
    int money;

    MonopolyNode currentnode;
    bool isInJail;
    int numTurnsInJail;
    [SerializeField] GameObject myToken;
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode> ();

    // PLAYERINFO
    PlayerInfo myInfo;

    // AI
    int aiMoneySavity = 200;

    // RETURN SOME INFOS
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentnode; 
    

    public void Initialize (MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
    {
        currentnode = startNode;
        money = startMoney;
        myInfo = info;
        myInfo.SetPlayerNameAndCash(name, money);
        myToken = token;
    }

    public void SetMyCurrentNode(MonopolyNode newNode) // TUR SONA ERDİ
    {
        currentnode = newNode;

        // OYUNCU NODE'UN ÜZERİNE GELDİ
        newNode.PlayerLandedOnNode(this);

        // AI NODE'UN ÜZERİNE GELDİ

            // EV İNŞA EDİLEBİLİNİR Mİ?

            // İPOTEKSİZ MÜLK MÜ?

            // KAYBEDİLMİŞ MÜLKLER İÇİN TİCARET YAPILABİLİNİR Mİ?
    }
}
