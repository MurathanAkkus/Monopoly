using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;


public class ChanceField : MonoBehaviour
{
    public static ChanceField instance;

    [SerializeField] List<SCR_ChanceCards> cards = new List<SCR_ChanceCards>();
    [SerializeField] TMP_Text cardText;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime; // ... SANİYE SONRA KARTI GİZLE
    // [SerializeField] float moveDelay = 0.5f;
    [SerializeField] Button closeCardButton;

    List<SCR_ChanceCards> cardPool = new List<SCR_ChanceCards>();
    List<SCR_ChanceCards> usedCardPool = new List<SCR_ChanceCards>();


    // ŞİMDİKİ KART VE OYUNCU
    SCR_ChanceCards pickedCard;
    SCR_ChanceCards jailFreeCard;
    Player currentPlayer;

    // İNSANLAR İÇİN PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool enablePayToFree, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    void OnEnable()
    {
        MonopolyNode.OnDrawChanceCard += Drawcard;
    }

    void OnDisable()
    {
        MonopolyNode.OnDrawChanceCard -= Drawcard;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cardHolderBackground.SetActive(false);
        // TÜM KARTLARI HAVUZA EKLE
        cardPool.AddRange(cards);

        // TÜM KARTLARI KARIŞTIR
        ShuffleCards();
    }

    void ShuffleCards()
    {
        for (int i = 0; i < cardPool.Count; i++)
        {
            int index = Random.Range(0, cardPool.Count);
            SCR_ChanceCards tempCard = cardPool[index];
            cardPool[index] = cardPool[i];
            cardPool[i] = tempCard;
        }
    }

    void Drawcard(Player cardTaker)
    {
        // BİR KART ÇEK
        pickedCard = cardPool[0];
        cardPool.RemoveAt(0);

        if (pickedCard.jailFreeCard)
            jailFreeCard = pickedCard;
        else
            usedCardPool.Add(pickedCard);

        if (cardPool.Count == 0)
        {
            // BÜTÜN KARTLARI GERİ BIRAK
            cardPool.AddRange(usedCardPool);
            usedCardPool.Clear();
            // TÜM KARTLARI KARIŞTIR
            ShuffleCards();
        }
        // ŞİMDİKİ OYUNCU
        currentPlayer = cardTaker;
        // KARTI GÖSTER
        cardHolderBackground.SetActive(true);
        // TEXTİ DOLDUR
        cardText.text = pickedCard.textOnCard;

        bool isAi = currentPlayer.playerType == Player.PlayerType.AI;
        if(isAi)
            Invoke("ApplyCardEffect", showTime);

        closeCardButton.gameObject.SetActive(!isAi);
    }

    public void ApplyCardEffect()
    {
        bool isMoving = false;

        if (pickedCard.rewardMoney != 0)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penaltyMoney != 0 && !pickedCard.payToPlayer)
        {
            currentPlayer.PayMoney(pickedCard.penaltyMoney);
        }
        else if (pickedCard.moveToBoardIndex != -1)
        {
            isMoving = true;
            // STEPS TO GOAL
            int currentIndex = Board.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
            int lengthOfBoard = Board.instance.route.Count;
            int stepsToMove = 0;
            if (currentIndex < pickedCard.moveToBoardIndex) // GİDECEĞİ NODE ÖNÜNDE İSE
                stepsToMove = pickedCard.moveToBoardIndex - currentIndex;

            else if (currentIndex > pickedCard.moveToBoardIndex) // GİDECEĞİ NODE ARKASINDA İSE
                stepsToMove = lengthOfBoard - currentIndex + pickedCard.moveToBoardIndex;

            // START THE MOVE
            Board.instance.MovePlayerToken(stepsToMove, currentPlayer);
        }
        else if (pickedCard.payToPlayer)
        {
            int totalCollected = 0;
            List<Player> allPlayers = GameManager.instance.GetPlayers;

            foreach (var player in allPlayers)
            {
                if (player != currentPlayer)
                {
                    // İFLASI ÖNLEMEK
                    int amount = Mathf.Min(currentPlayer.ReadMoney, pickedCard.penaltyMoney);
                    player.CollectMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.PayMoney(totalCollected);
        }
        else if (pickedCard.streetRepairs)
        {
            int[] allBuildings = currentPlayer.CountHousesAndHotels();
            int totalCosts = pickedCard.streetRepairsHotelPrice * allBuildings[0] + pickedCard.streetRepairsHotelPrice * allBuildings[1]; // Her Ev için 40 + her Otel için 115
            currentPlayer.PayMoney(totalCosts);
        }
        else if (pickedCard.goToJail)
        {
            currentPlayer.GoToJail(Board.instance.route.IndexOf(currentPlayer.MyMonopolyNode));
            isMoving = true;
        }
        else if (pickedCard.jailFreeCard) // 
        {
            currentPlayer.AddChanceJailFreeCard();
        }
        else if (pickedCard.moveStepsBackwards != 0)
        {
            int steps = System.Math.Abs(pickedCard.moveStepsBackwards);
            Board.instance.MovePlayerToken(-steps, currentPlayer);
            isMoving = true;
        }
        else if (pickedCard.nextRailroad)
        {
            Board.instance.MovePlayerToken(MonopolyNodeType.Railroad, currentPlayer);
            isMoving = true;
        }
        else if (pickedCard.nextUtility)
        {
            Board.instance.MovePlayerToken(MonopolyNodeType.Utility, currentPlayer);
            isMoving = true;
        }
        cardHolderBackground.SetActive(false);
        ContinueGame(isMoving);
    }

    void ContinueGame(bool isMoving)
    {
        //Debug.Log("isMoving: " + isMoving);
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            // if(!isMoving && GameManager.instance.RolledADouble)
            //     GameManager.instance.RollDice();
            // else if(!isMoving && !GameManager.instance.RolledADouble)
            //     GameManager.instance.SwitchPlayer();
            if (!isMoving)
                GameManager.instance.Continue();
        }
        else // İNSAN OYUNCUNUN INPUT'LARI
        {
            if (!isMoving)
            {
                bool jail1 = currentPlayer.HasChanceFreeCard;
                bool jail2 = currentPlayer.HasCommunityFreeCard;
                bool showPayToGetOut = currentPlayer.IsInJail && !GameManager.instance.HasRolledDice;

                OnShowHumanPanel.Invoke(true, GameManager.instance.RolledADouble, !GameManager.instance.RolledADouble, showPayToGetOut, jail1, jail2);
            }
        }
    }

    public void AddBackJailFreeCard()
    {
        usedCardPool.Add(jailFreeCard);
        jailFreeCard = null;
    }
}
