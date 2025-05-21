using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Board gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] int currentPlayer;

    [Header("Global Game Settings")]
    [SerializeField] int maxTurnsInJail = 3; // KODESTE KAÇ TUR KALACAĞINI AYARLAR
    [SerializeField] int startMoney = 1500;
    [SerializeField] int goMoney = 500;
    [SerializeField] float secondsBetweenTurns = 3;

    [Header("Player Info")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel; // PlayerInfo Prefab'larının ebeveyn olarak kabul edilmesi için
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();

    [Header("GameOver / WinInfo")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text winnerNameText;

    [Header("Dice")]
    [SerializeField] Dice dice1;
    [SerializeField] Dice dice2;

    // ATILAN ZAR
    List<int> rolledDice = new List<int>();
    bool rolledADouble;

    public bool RolledADouble => rolledADouble;
    public void ResetRolledADouble() => rolledADouble = false;
    int doubleRollCount;
    bool hasRolledDice;
    public bool HasRolledDice => hasRolledDice;

    // VERGİ HAVUZU
    int taxPoll = 0;

    // ATILAN ZAR


    // PARA ALMAK İÇİN GEÇ
    public int GetGoMoney => goMoney;
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayers => playerList;
    public Player GetCurrentPlayer => playerList[currentPlayer];

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    // İNSANLAR İÇİN PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    // DEBUG
    [Header("Debug")]
    public bool DebugRoll = false;
    [SerializeField] int rolledDice1;
    [SerializeField] int rolledDice2;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentPlayer = Random.Range(0, playerList.Count);
        gameOverPanel.SetActive(false);
        Initialize();
        CameraSwitcher.instance.SwitchToTopDown();

        StartCoroutine(StartGame());
        OnUpdateMessage.Invoke("<b>Hoşgeldiniz");
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(3f);
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
            // RollDice();
            RollPysicalDice();
        else // İNSAN INPUTLARI İÇİN UI GÖSTER
            OnShowHumanPanel.Invoke(true, false, false, false, false);
    }

    void Initialize()
    {
        if (GameSettings.settingsList.Count == 0)
        {
            Debug.LogError("Ana Menüden oyun başlatıldı!");
            return;
        }
        
        foreach (var setting in GameSettings.settingsList)
        {
            Player p1 = new Player();
            p1.name = setting.playerName;
            p1.playerType = (Player.PlayerType)setting.selectedType;


            playerList.Add(p1);

            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            if (infoObject == null)
                Debug.Log("Test: infoObject = null");
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            if (info == null)
                Debug.Log("Test: info = null");

            GameObject newToken = Instantiate(playerTokenList[setting.selectedColor], gameBoard.route[0].transform.position, Quaternion.identity);
            p1.Initialize(gameBoard.route[0], startMoney, info, newToken);
        }

        // for (int i = 0; i < playerList.Count; i++)
        // {   // BÜTÜN OYUNCULARI OLUŞTUR
        //     GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
        //     PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

        //     // RASTGELE TOKEN
        //     int randIndex = Random.Range(0, playerTokenList.Count);

        //     // BAŞLANGIÇ
        //     GameObject newToken = Instantiate(playerTokenList[randIndex], gameBoard.route[0].transform.position, Quaternion.identity);

        //     playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        // }

        playerList[currentPlayer].ActivateSelector(true);

        bool jail1 = playerList[currentPlayer].HasChanceFreeCard;
        bool jail2 = playerList[currentPlayer].HasCommunityFreeCard;

        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        else
            OnShowHumanPanel.Invoke(false, false, false, jail1, jail2);
    }

    public void RollPysicalDice()
    {
        CheckForJailFree();
        rolledDice.Clear();
        dice1.RollDice();
        dice2.RollDice();
        CameraSwitcher.instance.SwitchToDice();

        // UI GÖSTER VEYA GİZLE
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            bool jail1 = playerList[currentPlayer].HasChanceFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityFreeCard;

            OnShowHumanPanel.Invoke(true, false, false, jail1, jail2);
        }
    }

    void CheckForJailFree()
    {
        if (playerList[currentPlayer].IsInJail && playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            if (playerList[currentPlayer].HasChanceFreeCard)
                playerList[currentPlayer].UseCommunityJailFreeCard();
            else if (playerList[currentPlayer].HasCommunityFreeCard)
                playerList[currentPlayer].UseCommunityJailFreeCard();
        }
    }

    public void ReportDiceRolled(int diceValue)
    {
        rolledDice.Add(diceValue);
        if (rolledDice.Count == 2)
        {
            RollDice();
        }
    }

    void RollDice()  // INSAN VEYA AI TARAFINDAN ZAR ATMA BUTONUNA BAS
    {
        bool allowedToMove = true;
        hasRolledDice = true;

        // // SON ATILAN ZARI SIFIRLA
        // rolledDice = new int[2];

        // if (!DebugRoll)
        // {   // ZAR AT VE SAKLA
        //     rolledDice[0] = Random.Range(1, 7);
        //     rolledDice[1] = Random.Range(1, 7);
        // }
        // if (DebugRoll)
        // {   // DEBUG
        //     rolledDice[0] = rolledDice1;
        //     rolledDice[1] = rolledDice2;
        // }
        Debug.Log("Zarlar atildi: " + rolledDice[0] + " & " + rolledDice[1]);

        // ÇİFT Mİ?
        rolledADouble = rolledDice[0] == rolledDice[1]; // if(rolledDice[0] == rolledDice[1]) rolledADouble = true;

        // ARD ARDA 3 DEFA ÇİFT ATARSA -> KODESE -> TURU SONLANDIR

        // ZATEN HAPİSTE Mİ?
        if (playerList[currentPlayer].IsInJail)
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();

            if (rolledADouble)
            {
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>kodesten çıkabilir</color>, çünkü <b>çift</b> zar attı");
                doubleRollCount++;

                // OYUNCUYU HAREKET ETTİR

            }
            else if (playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                // YETERİNCE BURADA DURDU
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>kodesten çıkabilir</color>");
                // AYRILMASINA İZİN VERİLDİ
            }
            else
            {
                allowedToMove = false;
            }
        }
        else // KODESTE DEĞİLSE
        {
            // ÇİFT ZARLARI RESETLE
            if (!rolledADouble)
                doubleRollCount = 0;
            else
            {
                doubleRollCount++;
                if (doubleRollCount >= 3)
                {
                    // KODESE HAREKET ETTİR
                    int indexOnBoard = Board.instance.route.IndexOf(playerList[currentPlayer].MyMonopolyNode);
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    OnUpdateMessage?.Invoke(playerList[currentPlayer].name + " <b>3 defa cift</b> zar atti ve <b><color=red>kodese gitmesi</color></b> gerekiyor!");
                    rolledADouble = false; // RESET

                    return;
                }
            }
        }

        // HAPİSTEN ÇIKABİLİR Mİ?

        // İZİN VERİLİRSE İLERLE
        if (allowedToMove)
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " " + rolledDice[0] + " & " + rolledDice[1] + " attı");
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            // OYUNCU DEĞİŞTİRİLEBİLİR
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " " + rolledDice[0] + " & " + rolledDice[1] + " attı.<br><b><color=red>Kodeste</color></b> kalmalı!");
            StartCoroutine(DelayBetweenSwitchPlayer());
        }
    }
    IEnumerator DelayBeforeMove(int rolledDice)
    {
        CameraSwitcher.instance.SwitchToPlayer(playerList[currentPlayer].MyToken.transform);
        yield return new WaitForSeconds(secondsBetweenTurns);

        // İLERLEMEYE İZİN VERİLİRSE
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);

        // İLERLEMEYE İZİN VERİLMEZSE
    }

    IEnumerator DelayBetweenSwitchPlayer()
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        SwitchPlayer();
    }

    public void SwitchPlayer()
    {
        CameraSwitcher.instance.SwitchToTopDown();
        currentPlayer++;

        // 
        hasRolledDice = false;
        // ÇİFT Mi ATILDI?
        doubleRollCount = 0;

        // OYUNCU FAZLA MI?
        if (currentPlayer >= playerList.Count)
            currentPlayer = 0;

        DeactivateArrows();
        playerList[currentPlayer].ActivateSelector(true);
        // KODES KONTROL


        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)  // OYUNCU AI MI?
        {
            // RollDice();
            RollPysicalDice();

            OnShowHumanPanel.Invoke(false, false, false, false, false);
        }
        else // OYUNCU INSAN MI? - UI GÖSTER
        {
            bool jail1 = playerList[currentPlayer].HasChanceFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        }



    }

    public List<int> LastRolledDice => rolledDice;

    public void AddTaxToPool(int amount)
    {
        taxPoll += amount;
    }

    public int GetTaxPool()
    {
        // GEÇİCİ OLARAK VERGİ HAVUZUNU DEPOLAR
        int currentTaxCollected = taxPoll;
        // HAVUZU RESETLER
        taxPoll = 0;
        // GEÇİCİ VERGİYİ GÖNDERİR
        return currentTaxCollected;
    }

    // ------------------------------------------------------- OYUN BİTTİ -------------------------------------------------------
    public void RemovePlayer(Player player)
    {
        playerList.Remove(player);
        // OYUNUN BİTTİĞİNİ KONTROL ET
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {   // KAZANAN OYUNCU
            string str = playerList[0].name + " OYUNU KAZANDI!";
            Debug.Log(str);
            OnUpdateMessage.Invoke(str);
            // OYUN DÖNGÜSÜNÜ DURDUR

            //UI GÖSTER
            gameOverPanel.SetActive(true);
            winnerNameText.text = playerList[0].name;
        }
    }

    // ------------------------------------------------------- UI ÖGELERİ --------------------------------------------------------
    void DeactivateArrows()
    {
        foreach (var player in playerList)
        {
            player.ActivateSelector(false);
        }
    }

    // ------------------------------------------------------- OYUNA DEVAM ETME GÖREVLERİ ----------------------------------------
    public void Continue()
    {
        //Debug.Log(gameObject);
        if (playerList.Count > 1)
            Invoke("ContinueGame", SecondsBetweenTurns);
    }

    void ContinueGame()
    {
        if (RolledADouble)  // SON ATILAN ZARLAR ÇİFT GELDİYSE
        {   // TEKRAR AT
            // RollDice();     
            RollPysicalDice();
        }
        else                // ÇİFT GELMEDİYSE
            if (playerList.Count > 1)
            SwitchPlayer(); // OYUNCU DEĞİŞTİR 
    }

    // ------------------------------------------------------- İNSAN - İNSANIN İFLASI ----------------------------------------------------
    public void HumanBankrupt()
    {
        playerList[currentPlayer].Bankrupt();
        Continue();
    }

    // ------------------------------------------------------- İNSAN - HAPİSTEN ÇIKMA KARTI BUTONLARI ------------------------------------
    public void Jail1CardButtonEvent() // ŞANS KARTI
    {
        playerList[currentPlayer].UseChanceJailFreeCar();
    }
    public void Jail2CardButtonEvent() // KAMU KURULUŞU KARTI
    {
        playerList[currentPlayer].UseCommunityJailFreeCard();
    }
    
}