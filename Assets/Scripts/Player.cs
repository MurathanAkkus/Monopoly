using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEditor.Experimental.GraphView;
using TMPro;
using UnityEngine.UI;

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
    public string name; // OYUNCUNUN İSMİ
    int money; // OYUNCU BAKİYESİ

    MonopolyNode currentnode;
    bool isInJail;
    int numTurnsInJail;
    [SerializeField] GameObject myToken;
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>();
    public List<MonopolyNode> GetMonopolyNodes => myMonopolyNodes;

    bool hasChanceJailFreeCard, hasCommunityJailFreeCard;
    public bool HasChanceFreeCard => hasChanceJailFreeCard;
    public bool HasCommunityFreeCard => hasCommunityJailFreeCard;

    // PLAYERINFO
    PlayerInfo myInfo;

    // AI
    int aiMoneySavity = 200;

    // AI DURUMLARI (STATES)
    public enum AiStates
    {
        IDLE,
        TRADING
    }

    public AiStates aiState;

    // BAZI PLAYER BİLGİLERİ GET EDİLİR
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentnode;
    public int ReadMoney => money;

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    // İNSANLAR İÇİN PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool enablePayToFree, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    public void Initialize(MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
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
        if (playerType == PlayerType.AI)
        {
            // EV İNŞA EDİLEBİLİNİR Mİ?
            CheckIfPlayerHasASet();

            // İPOTEKSİZ MÜLK MÜ?
            UnMortgageProperties();

            // AI SETİNDEKİ EKSİK MÜLKLERİ ARAYACAK
            // TradingSystem.instance.FindMissingProperty(this);
        }
        else
            UiShowPanel.instance.ShowBuyButton(this);
    }

    public void CollectMoney(int amount)
    {
        money += amount;
        myInfo.SetPlayerCash(money);

        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = GameManager.instance.RolledADouble && ReadMoney >= 0;
            bool showPayToGetOut = IsInJail && !GameManager.instance.HasRolledDice;

            // UI GÖSTER
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, showPayToGetOut, hasChanceJailFreeCard, hasCommunityJailFreeCard);
        }
    }

    internal bool CanAffordNode(int price)
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
        OnUpdateMessage.Invoke(name + " " + node.name + " kartını satın aldı.");
        // TÜM NODE LARIN ÜCRETLERİNİ SIRALA
        SortPropertiesByPrice();
    }

    void SortPropertiesByPrice()
    {
        myMonopolyNodes = myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount, Player owner, MonopolyNode node)
    {
        // KİRA İÇİN YETERLİ PARASI YOKSA
        if (money < rentAmount)
        {
            if (playerType == PlayerType.AI) // BOT İÇİN BORÇLAR OTOMATİK YÖNETİLİR
                HandleInsufficientFunds(rentAmount);
            else
            {   // İNSAN BORÇLU MAALESEF
                // TURU DEVREDIŞI BIRAK VE ZAR AT
                OnShowHumanPanel.Invoke(true, false, true, false, hasChanceJailFreeCard, hasCommunityJailFreeCard);
            }
        }
        
        money -= rentAmount;
        owner.CollectMoney(rentAmount);
        if(node.monopolyNodeType == MonopolyNodeType.Property)
            OnUpdateMessage.Invoke(name + $" , {node.name} kartının sahibine " + rentAmount + "M kira ödedi.");
        else if(node.monopolyNodeType == MonopolyNodeType.Railroad)
            OnUpdateMessage.Invoke(name + $" , {node.name} kartının sahibine " + rentAmount + "M ulaşım ücreti ödedi.");
        else if (node.monopolyNodeType == MonopolyNodeType.Utility)
            OnUpdateMessage.Invoke(name + $" , {node.name} kartının sahibine " + rentAmount + "M fatura ödedi.");
        else if (node.monopolyNodeType == MonopolyNodeType.Property)

                    // UI GÜNCELLE
                    myInfo.SetPlayerCash(money);
    }

    internal void PayMoney(int amount)
    {
        // YETERLİ PARASI YOKSA
        if (money < amount)
        {
            if (playerType == PlayerType.AI) // BOT İÇİN BORÇLAR OTOMATİK YÖNETİLİR
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

        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = (GameManager.instance.RolledADouble || !GameManager.instance.HasRolledDice) && ReadMoney >= 0;
            bool showPayToGetOut = IsInJail && !GameManager.instance.HasRolledDice;

            // UI GÖSTER
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, showPayToGetOut, hasChanceJailFreeCard, hasCommunityJailFreeCard);
        }
    }

    //-------------------------------------------------- KODES --------------------------------------------------

    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        // OYUNCUNUN POZİSYONUNU TEKRAR AYARLA
        Board.instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);
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
            if (node.NumberOfHouses != 5)
                houses += node.NumberOfHouses;
            else
                hotels++;
        }

        int[] allBuildings = new int[] { houses, hotels };
        return allBuildings;
    }

    // ---------------------------- YETERSİZ PARAYI YÖNET ----------------------------
    public void HandleInsufficientFunds(int amountToPay)
    {
        int housesToSell; // SATILABİLİR EVLER
        int allHouses = 0;
        int propertiesToMortgage;
        int allPropertiesToMortgage = 0;

        // TÜM EVLERİN SAYISI
        foreach (var node in myMonopolyNodes)
        {
            allHouses += node.NumberOfHouses;
        }

        // ARSALARI GÖZDEN GEÇİR VE GEREKTİĞİ KADARINI SAT
        while (money < amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    // DAHA FAZLA PARAYA İHTİYAÇ VAR mı?
                    if (money >= amountToPay)
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
        while (money < amountToPay && allPropertiesToMortgage > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage = (!node.IsMortgaged) ? 1 : 0;
                if (propertiesToMortgage > 0)
                {
                    CollectMoney(node.MortgageProperty());
                    allPropertiesToMortgage--;
                    // DAHA FAZLA PARAYA İHTİYAÇ VAR mı?
                    if (money >= amountToPay)
                        return;
                }
            }
        }

        if (playerType == PlayerType.AI) // AI BU NOKTAYA GELMİŞSE GEÇMİŞ OLSUN - İFLAS ETTİ
            Bankrupt();
    }

    // ---------------------------- İFLAS - OYUN BİTTİ ----------------------------
    internal void Bankrupt()
    {
        // OYUNCUYU OYUNDAN ÇIKAR
        // MessageSystem e BİR MESAJ GÖNDER
        OnUpdateMessage.Invoke(name + " <color=purple>iflas etti :(</color>");
        // OYUNCUNUN SAHİP OLDUĞU HER ŞEYİ TEMİZLE
        for (int i = myMonopolyNodes.Count - 1; i >= 0; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }
        myMonopolyNodes.Clear();

        if (hasChanceJailFreeCard)
            ChanceField.instance.AddBackJailFreeCard();
        if (hasCommunityJailFreeCard)
            CommunityChest.instance.AddBackJailFreeCard();

        if (myToken != null)
            GameObject.Destroy(myToken);

        if (myInfo != null)
        {
            TMP_Text nameText = myInfo.GetNameText;
            if (nameText != null)
                nameText.text = $"<s>{nameText.text}</s>";
        }

        ActivateSelector(false);
        // OYUNCUYU SİL
        GameManager.instance.RemovePlayer(this);
    }

    // ---------------------------- İPOTEK KALDIRMA ----------------------------
    void UnMortgageProperties()
    {
        // BOT İÇİN
        foreach (var node in myMonopolyNodes)
        {
            if (node.IsMortgaged)
            {
                if (node.IsMortgaged)
                {
                    int cost = node.MortgageValue + (int)(node.MortgageValue / 10); // %10 FAİZ
                    // İPOTEĞİ KALDIRMAK İÇİN PARAMIZ YETERLİ mi?
                    if (money >= aiMoneySavity + cost)
                    {
                        PayMoney(cost);
                        node.UnMortgageProperty(cost);
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
            var (list, allSame) = Board.instance.PlayerHasAllNodesOfSet(node);
            if (!allSame)
                continue;

            List<MonopolyNode> nodeSets = list;

            if (nodeSets != null && nodeSets != processedSet)
            {   // Set içindeki herhangi bir arsa ipotekli mi?
                bool hasMortgagedNode = nodeSets.Any(node => node.IsMortgaged) ? true : false;
                if (!hasMortgagedNode)
                {
                    if (nodeSets[0].monopolyNodeType == MonopolyNodeType.Property)
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
            if (numOfHouses < minHouses)
                minHouses = numOfHouses;

            if (numOfHouses > maxHouses && numOfHouses < 5)
                maxHouses = numOfHouses;
        }
        // İZİN VERİLEN MAKSİMUM SAYIDA EV SATIN AL
        foreach (var node in nodesToBuildOn)
        {
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordHouse(node.houseCost))
            {
                node.BuildHouseOrHotel();
                PayMoney(node.houseCost);
                if (node.NumberOfHouses < 5)
                    OnUpdateMessage.Invoke($"{name}, {node.name} arsasına <u>{node.NumberOfHouses}. evini</u> inşa etti.");
                else
                    OnUpdateMessage.Invoke($"{name}, {node.name} arsasına bir otel inşa etti.");
                // SADECE BİR ARSAYA EV İNŞA ETMEK İÇİN DURDUR
                break;
            }
        }
    }

    internal void SellHouseOrHotel(List<MonopolyNode> nodeToSellFrom)
    {
        int minHouses = int.MaxValue;
        bool houseSold = false;

        foreach (var node in nodeToSellFrom)
        {
            minHouses = Mathf.Min(minHouses, node.NumberOfHouses);
        }

        // EV SAT
        for (int i = nodeToSellFrom.Count - 1; i >= 0; i--)
        {
            if (nodeToSellFrom[i].NumberOfHouses > minHouses)
            {
                CollectMoney(nodeToSellFrom[i].SellHouseOrHotel());
                houseSold = true;
                break;
            }
        }

        if (!houseSold)
            CollectMoney(nodeToSellFrom[nodeToSellFrom.Count - 1].SellHouseOrHotel());
    }

    // ---------------------------- EVLER VE OTELLER - KARŞILAYABİLME VE SAYMA ----------------------------
    public bool CanAffordHouse(int price)
    {
        if (playerType == PlayerType.AI) // BOTLAR İÇİN
            return (money - aiMoneySavity) >= price;

        // İNSANLAR İÇİN
        return money >= price;
    }

    // ---------------------------- SELECTOR --------------------------------------------------------------
    public void ActivateSelector(bool active)
    {
        myInfo.ActivateArrow(active);
    }

    // ---------------------------- TİCARET SİSTEMİ ------------------------------------------------------

    // ---------------------------- NODE EKLEME - ÇIKARMA ------------------------------------------------
    public void AddNode(MonopolyNode node)
    {
        myMonopolyNodes.Add(node);
        // ÜCRETİNE GÖRE NODEları SIRALA
        SortPropertiesByPrice();
    }

    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
        // ÜCRETLERİNE GÖRE NODEları SIRALA
        SortPropertiesByPrice();
    }

    // ---------------------------- DURUM MAKİNELERİ - STATE MACHINE -------------------------------------
    public void ChangeState(AiStates state)
    {
        if (playerType == PlayerType.HUMAN)
            return;

        aiState = state;
        switch (aiState)
        {
            case AiStates.IDLE: // OYUN DEVAM EDERKEN BEKLER
                {
                    // OYUN DEVAM ET
                    //ContinueGame();
                    GameManager.instance.Continue();
                }
                break;
            case AiStates.TRADING:
                {
                    // DEVAM EDENE KADAR BEKLE
                    TradingSystem.instance.FindMissingProperty(this);
                }
                break;
        }
    }

    // ---------------------------- public void AddChanceJail --------------------------------------------
    public void AddChanceJailFreeCard()
    {
        hasChanceJailFreeCard = true;
    }

    public void AddCommunityJailFreeCard()
    {
        hasCommunityJailFreeCard = true;
    }

    public void UseChanceJailFreeCard() // jail1
    {
        if (!isInJail)
            return;
        
        SetOutOfJail();
        hasChanceJailFreeCard = false;
        CommunityChest.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke($"{name} <u>kodesten çıkış</u> kartını kullandı.");
    }
    public void UseCommunityJailFreeCard() // jail2
    {
        if (!isInJail)
            return;
        SetOutOfJail();
        hasCommunityJailFreeCard = false;
        ChanceField.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke($"{name} <u>kodesten çıkış</u> kartını kullandı.");
    }

    public void PayToFree()
    {
        if (!isInJail)
            return;

        // YETERLİ PARASI VARSA
        if (money >= 50)
        {
            PayMoney(50);
            GameManager.instance.AddTaxToPool(50);
            SetOutOfJail();
            OnUpdateMessage.Invoke($"{name} <u>kodesten çıkmak için 50M ödedi.");
        }
        else
            OnUpdateMessage?.Invoke($"{name} kodesten çıkmak için yeterli paraya sahip değil!");
    }
}