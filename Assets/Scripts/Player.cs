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
    public List<MonopolyNode> GetMonopolyNodes => myMonopolyNodes;

    // PLAYERINFO
    PlayerInfo myInfo;

    // AI
    int aiMoneySavity = 200;

    // RETURN SOME INFOS
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentnode; 
    public int ReadMoney => money;

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    // İNSANLAR İÇİN PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
    public static ShowHumanPanel OnShowHumanPanel;
    
    public void Initialize (MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
    {
        currentnode = startNode;
        money = startMoney;
        myInfo = info;
        myInfo.SetPlayerNameAndCash(name, money);
        myToken = token;
        myInfo.ActivateArrow(false);
    }

    public void SetMyCurrentNode(MonopolyNode newNode) // TUR SONA ERDİ
    {
        currentnode = newNode;

        // OYUNCU NODE'UN ÜZERİNE GELDİ
        newNode.PlayerLandedOnNode(this);

        // AI NODE'UN ÜZERİNE GELDİ
        if(playerType == PlayerType.AI) 
        {
            // EV İNŞA EDİLEBİLİNİR Mİ?
            CheckIfPlayerHasASet();

            // İPOTEKSİZ MÜLK MÜ?
            UnMortgageProperties();

            // KAYBEDİLMİŞ MÜLKLER İÇİN TİCARET YAPILABİLİNİR Mİ?
        }
    }

    public void CollectMoney (int amount)
    {
        money += amount;
        myInfo.SetPlayerCash(money);

        if(playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0;
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0;
            // UI GÖSTER
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn);
        }
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
        // SAHİPLİĞİ AYARLA
        myMonopolyNodes.Add(node);
        // TÜM NODE LARIN ÜCRETLERİNİ SIRALA
        SortPropertiesByPrice();
    }

    void SortPropertiesByPrice()
    {
        myMonopolyNodes = myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount, Player owner)
    {
        // KİRA İÇİN YETERLİ PARASI YOKSA
        if(money < rentAmount)
        {
            if(playerType == PlayerType.AI) // BOT İÇİN BORÇLAR OTOMATİK YÖNETİLİR
                HandleInsufficientFunds(rentAmount);
            else
            {   // İNSAN BORÇLU MAALESEF
                // TURU DEVREDIŞI BIRAK VE ZAR AT
                OnShowHumanPanel.Invoke(true, false, false);
            }
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
            if(playerType == PlayerType.AI) // BOT İÇİN BORÇLAR OTOMATİK YÖNETİLİR
                HandleInsufficientFunds(amount);
            /* else
            {   // İNSAN BORÇLU MAALESEF
                // TURU DEVREDIŞI BIRAK VE ZAR AT
                OnShowHumanPanel.Invoke(true, false, false);
            } */
        }
        money -= amount;

        // UI GÜNCELLE
        myInfo.SetPlayerCash(money);

        if(playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0;
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0;
            // UI GÖSTER
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn);
        }
    }
    
    //-------------------------------------------------- KODES --------------------------------------------------

    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        // OYUNCUNUN POZİSYONUNU TEKRAR AYARLA
        //myToken.transform.position = Board.instance.route[10].transform.position;
        //currentnode = Board.instance.route[10];
        Board.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard),this);
        GameManager.instance.ResetRolledADouble();
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

    // ---------------------------- SOKAK TAMİRLERİ ----------------------------
    public int[] CountHousesAndHotels()
    {
        int houses = 0; // CountHousesAndHotels[0]
        int hotels = 0; // CountHousesAndHotels[1]

        foreach (var node in myMonopolyNodes)
        {
            if(node.NumberOfHouses!=5)
                houses += node.NumberOfHouses;
            else
                hotels++;
        }

        int[] allBuildings = new int[]{houses,hotels};
        return allBuildings;
    }

    // ---------------------------- YETERSİZ PARAYI YÖNET ----------------------------
    public void HandleInsufficientFunds(int amountToPay)
    {
        int housesToSell = 0; // SATILABİLİR EVLER
        int allHouses = 0;
        int propertiesToMortgage = 0;
        int allPropertiesToMortgage = 0;

        // TÜM EVLERİN SAYISI
        foreach (var node in myMonopolyNodes)
        {
            allHouses += node.NumberOfHouses;
        }

        // ARSALARI GÖZDEN GEÇİR VE GEREKTİĞİ KADARINI SAT
        while(money < amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if(housesToSell > 0)
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    // DAHA FAZLA PARAYA İHTİYAÇ VAR mı?
                    if(money >= amountToPay)
                        return;
                }
            }
        }
        // İPOTEK
        foreach (var node in myMonopolyNodes)
        {
            allPropertiesToMortgage += (!node.IsMortgaged) ? 0 : 1;
        }

        // DAHADA ARSALARI GÖZDEN GEÇİR VE İHTİYAÇ KADARI KADAR İPOTEKLE
        while(money < amountToPay && allPropertiesToMortgage > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage = (!node.IsMortgaged) ? 1 : 0;
                if(propertiesToMortgage>0)
                {
                    CollectMoney(node.MortgageProperty());
                    allPropertiesToMortgage--;
                    // DAHA FAZLA PARAYA İHTİYAÇ VAR mı?
                    if(money >= amountToPay)
                        return;
                }
            }
        }
        // BU NOKTAYA GELMİŞSEN GEÇMİŞ OLSUN - İFLAS ETTİN
        Bankrupt();
    }

    // ---------------------------- İFLAS - OYUN BİTTİ ----------------------------
    void Bankrupt()
    {
        // OYUNCUYU OYUNDAN ÇIKAR

        // MessageSystem e BİR MESAJ GÖNDER
        OnUpdateMessage.Invoke(name + " <color = purple>iflas etti :(</color>");
        // OYUNCUNUN SAHİP OLDUĞU HER ŞEYİ TEMİZLE
        for (int i = myMonopolyNodes.Count-1; i >= 0; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }
        // OYUNCUYU SİL
        GameManager.instance.RemovePlayer(this);
    }

    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
    }

    // ---------------------------- İPOTEK KALDIRMA ----------------------------
    void UnMortgageProperties()
    {
        // BOT İÇİN
        foreach (var node in myMonopolyNodes)
        {
            if(node.IsMortgaged)
            {
                if(node.IsMortgaged)
                {
                    int cost = node.MortgageValue + (int)(node.MortgageValue * 0.1f); // %10 FAİZ
                    // İPOTEĞİ KALDIRMAK İÇİN PARAMIZ YETERLİ mi?
                    if(money >= aiMoneySavity + cost)
                    {
                        PayMoney(cost);
                        node.UnMortgageProperty();
                    }
                }
                
            }
        }
    }

    // ---------------------------- OYUNCUNUN MÜLK SETİNE SAHİP OLUP OLMADIĞINI KONTROL ET ----------------------------
    void CheckIfPlayerHasASet()
    {
        // HER BİR SETİ BİR KEZ ÇAĞIR
        // Bir ev eklemek istediğimde Setteki tüm arsalar ev eklemesini engellemek için geçici bir list oluşturdum
        List<MonopolyNode> processedSet = null;
        // KARŞILAŞTIR VE DEPOLA
        foreach (var node in myMonopolyNodes)
        {
            var(list, allSame) = Board.instance.PlayerHasAllNodesOfSet(node);
            if (!allSame)
                continue;

            List<MonopolyNode> nodeSets = list;

            if (nodeSets != null && nodeSets != processedSet)
            {   // Set içindeki herhangi bir arsa ipotekli mi?
                bool hasMortgagedNode = nodeSets.Any(node => node.IsMortgaged) ? true : false;
                if (!hasMortgagedNode)
                {
                    if(nodeSets[0].monopolyNodeType == MonopolyNodeType.Property)
                    {   // SET TAMAMLANDI VE EV İNŞA EDİLEBİLİR
                        BuildHouseOrHotelEvenly(nodeSets);
                        
                        processedSet = nodeSets;    // İŞLENEN SET BURADA GÜNCELLENİR
                    }
                }
            }
        }
    }

    // ---------------------------- MÜLK SETLERİNE EŞİT ŞEKİLDE EV YAP ----------------------------
    internal void BuildHouseOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)
    {
        int minHouses = int.MaxValue;
        int maxHouses = int.MinValue;

        // MÜLKLERDE(ARSALARDA) BULUNAN MIN VE MAX EV SAYILARINI AL
        foreach (var node in nodesToBuildOn)
        {
            int numOfHouses = node.NumberOfHouses;
            if(numOfHouses < minHouses)
                minHouses = numOfHouses;

            if(numOfHouses > maxHouses && numOfHouses < 5)
                maxHouses = numOfHouses;
        }
        // İZİN VERİLEN MAKSİMUM SAYIDA EV SATIN AL
        foreach (var node in nodesToBuildOn)
        {
            if(node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordHouse(node.houseCost))
            {
                node.BuildHouseOrHotel();
                PayMoney(node.houseCost);
                // SADECE BİR ARSAYA EV İNŞA ETMEK İÇİN DURDUR
                break;
            }
        }
    }

    internal void SellHouseEvenly(List<MonopolyNode> nodeToSellFrom)
    {
        int minHouses = int.MaxValue;
        bool houseSold = false;

        foreach (var node in nodeToSellFrom)
        {
            minHouses = Mathf.Min(minHouses, node.NumberOfHouses);
        }

        // EV SAT
        for (int i = nodeToSellFrom.Count-1; i >= 0 ; i--)
        {
            if(nodeToSellFrom[i].NumberOfHouses > minHouses)
            {
                CollectMoney(nodeToSellFrom[i].SellHouseOrHotel());
                houseSold = true;
                break;
            }
        }

        if(!houseSold)
            CollectMoney(nodeToSellFrom[nodeToSellFrom.Count - 1].SellHouseOrHotel());
        
    }
    // ---------------------------- TAKAS SİSTEMİ ----------------------------

    // ---------------------------- SETTE EKSİK OLAN MÜLKLERİ BUL ----------------------------

    // ---------------------------- EVLER VE OTELLER - KARŞILAYABİLME VE SAYMA ----------------------------
    public bool CanAffordHouse(int price)
    {
        if(playerType == PlayerType.AI) // BOTLAR İÇİN
            return (money - aiMoneySavity) >= price;

        // İNSANLAR İÇİN
        return money >= price;
    }

    public void ActivateSelector(bool active)
    {
        myInfo.ActivateArrow(active);
    }
}