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

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;
    
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
        SortPropertiesByPrice();
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

    internal void PayMoney(int amount)
    {
        // YETERLİ PARASI YOKSA
        if(money < amount)
        {
            // 
        }
        money -= amount;

        // UI GÜNCELLE
        myInfo.SetPlayerCash(money);
    }
    
    //-------------------------------------------------- KODES --------------------------------------------------

    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        // OYUNCUNUN POZİSYONUNU TEKRAR AYARLA
        //myToken.transform.position = Board.instance.route[10].transform.position;
        //currentnode = Board.instance.route[10];
        Board.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard),this);
    }

    public void SetOutOfJail()
    {
        isInJail = false;

        // KODESTE TURLARI RESETLE
        numTurnsInJail = 0;
    }

    int CalculateDistanceFromJail(int indexOnBoard)
    {
        int result = 0;
        int indexOfJail = 10;

        if (indexOnBoard > indexOfJail)
            result = -(indexOnBoard - indexOfJail);
        else
            result = indexOfJail - indexOnBoard;
        
        return result;
    }

    public int NumTurnsInJail => numTurnsInJail;

    public void IncreaseNumTurnsInJail()
    {
        numTurnsInJail++;
    }

    // ---------------------------- YETERSİZ PARAYI YÖNET HANDLE INSUFFICIENT FUNDS 

    // ---------------------------- İFLAS - OYUN BİTTİ ----------------------------

    // ---------------------------- OYUNCUNUN MÜLK SETİNE SAHİP OLUP OLMADIĞINI KONTROL ET ----------------------------

    // ---------------------------- MÜLK SETLERİNE EŞİT ŞEKİLDE EV YAP ----------------------------

    // ---------------------------- TAKAS SİSTEMİ ----------------------------

    // ---------------------------- SETTE EKSİK OLAN MÜLKLERİ BUL ----------------------------

    // ---------------------------- EVLER VE OTELLER - KARŞILAYABİLME VE SAYMA ----------------------------
    
}