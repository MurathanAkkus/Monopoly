using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class CommunityChest : MonoBehaviour
{
    public static CommunityChest instance;
    [SerializeField] List<SCR_CommunityCard> cards = new List<SCR_CommunityCard>();
    [SerializeField] TMP_Text cardText;
    [SerializeField] GameObject cardHolderBackground;
    [SerializeField] float showTime; // ... SANİYE SONRA KARTI GİZLE
    // [SerializeField] float moveDelay = 0.5f;
    [SerializeField] Button closeCardButton;
    [Space]

    List<SCR_CommunityCard> cardPool = new List<SCR_CommunityCard>();
    List<SCR_CommunityCard> usedCardPool = new List<SCR_CommunityCard>();


    // ŞİMDİKİ KART VE OYUNCU
    SCR_CommunityCard pickedCard;
    SCR_CommunityCard jailFreeCard;
    Player currentPlayer;

    // İNSANLAR İÇİN PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool enablePayToFree, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    void OnEnable()
    {
        MonopolyNode.OnDrawCommunityCard += Drawcard;
    }

    void OnDisable()
    {
        MonopolyNode.OnDrawCommunityCard -= Drawcard;
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
            SCR_CommunityCard tempCard = cardPool[index];
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
        // EĞER BİR AI İSE, BUTONU DEAKTİF ET
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            closeCardButton.interactable = false;
            Invoke("ApplyCardEffect", showTime);
        }
        else
            closeCardButton.interactable = true;

    }

    public void ApplyCardEffect()
    {
        bool isMoving = false;

        if (pickedCard.rewardMoney != 0 && !pickedCard.collectFromPlayer)
        {
            currentPlayer.CollectMoney(pickedCard.rewardMoney);
        }
        else if (pickedCard.penaltyMoney != 0)
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
        else if (pickedCard.collectFromPlayer)
        {
            int totalCollected = 0;
            List<Player> allPlayers = GameManager.instance.GetPlayers;

            foreach (var player in allPlayers)
            {
                if (player != currentPlayer)
                {
                    // İFLASI ÖNLEMEK
                    int amount = Mathf.Min(player.ReadMoney, pickedCard.rewardMoney);
                    player.PayMoney(amount);
                    totalCollected += amount;
                }
            }
            currentPlayer.CollectMoney(totalCollected);
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
        else if (pickedCard.jailFreeCard)
        {
            currentPlayer.AddCommunityJailFreeCard();
        }
        cardHolderBackground.SetActive(false);
        ContinueGame(isMoving);
    }

    void ContinueGame(bool isMoving)
    {
        //Debug.Log("isMoving: " + isMoving);
        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            if (!isMoving && GameManager.instance.RolledADouble)
                GameManager.instance.RollPysicalDice();
            else if (!isMoving && !GameManager.instance.RolledADouble)
                GameManager.instance.SwitchPlayer();
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