using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

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

    public void CollectMoney (int amount)
    {
        money += amount;
        myInfo.SetPlayerCash(money);
    }
    
    internal bool CanAffordNode (int price)
    {   // NODE'U KARŞILAYABİLİR Mİ?
        return price <= money;
    }

    public void BuyProperty(MonopolyNode node)
    {
        money -= node.price;
        node.SetOwner(this);
        // UI GÜNCELLE
        myInfo.SetPlayerCash(money);
        // SAHİPLİK
        myMonopolyNodes.Add(node);
        // TÜM NODE'LARIN ÜCRETLERİNİ SIRALA

    }
    void SortPropertiesByPrice()
    {
        myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount, Player owner)
    {
        // KİRA İÇİN YETERLİ PARASI YOKSA
        if(money < rentAmount)
        {
            // 
        }
        money -= rentAmount;
        owner.CollectMoney(rentAmount);

        // UI GÜNCELLE
        myInfo.SetPlayerCash(money);
    }
}
