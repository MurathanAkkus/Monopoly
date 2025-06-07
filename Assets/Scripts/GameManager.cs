using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] float secondsBetweenTurns = 3f;

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
    [Space]

    [SerializeField] Button payToFreeButton;
    [SerializeField] Button jailFreeCardButton1;
    [SerializeField] Button jailFreeCardButton2;
    [Space]

    // ATILAN ZAR
    List<int> rolledDice = new List<int>();
    bool rolledADouble;

    public bool RolledADouble => rolledADouble;
    public void ResetRolledADouble() => rolledADouble = false;

    int doubleRollCount;

    bool hasRolledDice = false;
    public bool HasRolledDice => hasRolledDice;

    bool isBusy;
    public bool IsBusy => isBusy;

    public void SetBusy(bool busy)
    {
        isBusy = busy;
    }

    // VERGİ HAVUZU
    int taxPoll = 0;

    // PARA ALMAK İÇİN GEÇ
    public int GetGoMoney => goMoney;

    public float SecondsBetweenTurns => secondsBetweenTurns;

    public List<Player> GetPlayers => playerList;
    public Player GetCurrentPlayer => playerList[currentPlayer];

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    // İNSANLAR İÇİN PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool enablePayToFree, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Initialize();
        OnUpdateMessage.Invoke("<b>Hoşgeldiniz");

        gameOverPanel.SetActive(false);
        CameraSwitcher.instance.SwitchToTopDown();

        StartCoroutine(WaitUntilSceneIsReadyThenStartGame());
        StartGame();
    }

    IEnumerator WaitUntilSceneIsReadyThenStartGame()
    {
        // GAMEMANAGER VE KAMERALARI BEKLER
        yield return new WaitUntil(() => CameraSwitcher.instance != null && GameManager.instance != null);

        yield return new WaitForEndOfFrame(); // UI LAYOUTu BEKLER

        StartGame();
    }

    void StartGame()
    {
        RollPysicalDice();
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
            OnShowHumanPanel.Invoke(true, false, false, false, false, false);
    }

    void Initialize()
    {
        if (GameSettings.settingsList.Count == 0)
        {
            Debug.LogError("Oyuncu Yok!");
            return;
        }

        // OYUNCU LİSTESİNİ KARIŞTIR
        var shuffledSettings = GameSettings.settingsList.OrderBy(_ => Random.value).ToList();

        foreach (var setting in shuffledSettings)
        {
            // Player NESNESİNİ OLUŞTUR VE BİLGİLERİNİ AYARLA
            Player player = new Player
            {
                name = setting.playerName,
                playerType = (Player.PlayerType)setting.selectedType
            };

            // OYUNCU LİSTESİ UI AYARLA
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            if (infoObject == null)
            {
                Debug.LogWarning("infoObject null!");
                continue;
            }

            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            if (info == null)
            {
                Debug.LogWarning("PlayerInfo component yok!");
                continue;
            }

            GameObject newToken = Instantiate(playerTokenList[setting.selectedColor], gameBoard.route[0].transform.position, Quaternion.identity);

            player.Initialize(gameBoard.route[0], startMoney, info, newToken);

            playerList.Add(player);

            // UIdaki İSMİNİ SEÇİLEN RENGE GÖRE AYARLA
            Color playerColor = newToken.GetComponentInChildren<Renderer>().material.color;
            info.SetPlayerNameColor(playerColor);
        }
        currentPlayer = 0;        // İLK OYUNCU LİSTENİN EN BAŞINDAKİ
        playerList[currentPlayer].ActivateSelector(true);

        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
            Player.OnShowHumanPanel?.Invoke(true, true, false, false, false, false);
        else
            Player.OnShowHumanPanel?.Invoke(false, false, false, false, false, false);
    }

    public void RollPysicalDice()
    {
        if (isBusy)
            return;
        SetBusy(true);
        CheckForJailFree();
        rolledDice.Clear();
        OnUpdateMessage.Invoke($"<b>{playerList[currentPlayer].name}</b> zar atıyor");
        dice1.RollDice();
        dice2.RollDice();
    
        CameraSwitcher.instance.SwitchToDice();

        // UI GÖSTER VEYA GİZLE
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            bool jail1 = playerList[currentPlayer].HasChanceFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityFreeCard;
            bool showPayToGetOut = playerList[currentPlayer].IsInJail && !HasRolledDice;

            OnShowHumanPanel.Invoke(true, false, false, showPayToGetOut, jail1, jail2);
        }
    }

    void CheckForJailFree()
    {
        if (playerList[currentPlayer].IsInJail && playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            if (playerList[currentPlayer].HasChanceFreeCard)
                playerList[currentPlayer].UseChanceJailFreeCard();
            else if (playerList[currentPlayer].HasCommunityFreeCard)
                playerList[currentPlayer].UseCommunityJailFreeCard();
            else
                playerList[currentPlayer].PayToFree();
        }
    }

    public void ReportDiceRolled(int diceValue)
    {
        rolledDice.Add(diceValue);
        if (rolledDice.Count == 2)
            RollDice();
    }

    void RollDice()  // INSAN VEYA AI TARAFINDAN ZAR ATMA BUTONUNA BAS
    {
        bool allowedToMove = true;
        hasRolledDice = true;

        // ÇİFT Mİ? ARD ARDA 3 DEFA ÇİFT ATARSA -> KODESE -> TURU SONLANDIR
        rolledADouble = rolledDice[0] == rolledDice[1]; // if(rolledDice[0] == rolledDice[1]) rolledADouble = true;

        if (playerList[currentPlayer].IsInJail) // ZATEN HAPİSTE Mİ?
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();

            if (rolledADouble)
            {
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke($"<b>{playerList[currentPlayer].name}</b> <color=green>kodesten çıktı</color>, çünkü <b>çift</b> zar attı");
                doubleRollCount++;
                // OYUNCUYU HAREKET ETTİR
            }
            else if (playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                // YETERİNCE BURADA DURDU - 3 TUR
                OnUpdateMessage.Invoke($"<b>{playerList[currentPlayer].name}</b> 3 tur boyunca çift atamadı. Bu yüzden 50M ödeyerek çıkmak zorunda.");

                if (playerList[currentPlayer].ReadMoney >= 50)
                {
                    playerList[currentPlayer].PayMoney(50);
                    AddTaxToPool(50);

                    // HAREKETE İZİN VERİLDİ
                    playerList[currentPlayer].SetOutOfJail();
                }
                else
                {
                    OnUpdateMessage.Invoke($"{playerList[currentPlayer].name} kodesten çıkmak için yeterli paraya sahip değil!");
                    allowedToMove = false;
                }
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
                    OnUpdateMessage?.Invoke($"<b>{playerList[currentPlayer].name}</b> <b>3 defa cift</b> zar atti. Bu yüzden <b><color=red>kodese</color></b> gitmesi gerekiyor!");
                    rolledADouble = false; // RESET

                    return;
                }
            }
        }

        // HAPİSTEN ÇIKABİLİR Mİ?
        // İZİN VERİLİRSE İLERLE
        if (allowedToMove)
        {
            OnUpdateMessage.Invoke($"<b>{playerList[currentPlayer].name}</b>  {rolledDice[0]} ve {rolledDice[1]} attı");
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            // OYUNCU DEĞİŞTİRİLEBİLİR
            OnUpdateMessage.Invoke($"<b>{playerList[currentPlayer].name}</b>  {rolledDice[0]} ve {rolledDice[1]} attı.<br><b><color=red>Kodeste</b> kalmalı!");
            StartCoroutine(DelayBetweenSwitchPlayer());
        }
        UpdateJailButtons();
    }
    IEnumerator DelayBeforeMove(int rolledDice)
    {
        CameraSwitcher.instance.SwitchToPlayer(playerList[currentPlayer].MyToken.transform);
        yield return new WaitForSeconds(secondsBetweenTurns);

        // İLERLEMEYE İZİN VERİLİRSE
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);
        // İLERLEMEYE İZİN VERİLMEZSE HAREKET ETMEZ ZATEN
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

        // ATILAN ZARI SIFIRLA - YENİ OYUNCU HENÜZ ZAR ATMADI
        hasRolledDice = false;
        // ÇİFT Mi ATILDI?
        doubleRollCount = 0;

        // OYUNCU FAZLA MI?
        if (currentPlayer >= playerList.Count)
            currentPlayer = 0;

        DeactivateArrows();
        playerList[currentPlayer].ActivateSelector(true);

        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)  // OYUNCU AI MI?
        {
            RollPysicalDice();
            OnShowHumanPanel.Invoke(false, false, false, false, false, false);
        }
        else // OYUNCU INSAN MI? - UI GÖSTER
        {
            bool jail1 = playerList[currentPlayer].HasChanceFreeCard;
            bool jail2 = playerList[currentPlayer].HasCommunityFreeCard;
            bool showPayToGetOut = playerList[currentPlayer].IsInJail && !HasRolledDice;
            UpdateJailButtons();
            OnShowHumanPanel.Invoke(true, true, false, showPayToGetOut, jail1, jail2);
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
        int removedIndex = playerList.IndexOf(player);
        playerList.Remove(player);
        // OYUNUN BİTTİĞİNİ KONTROL ET
        CheckForGameOver();
        
            if (currentPlayer >= playerList.Count)
                currentPlayer = 0;
    }

    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {   // KAZANAN OYUNCU
            string str = playerList[0].name + " OYUNU KAZANDI!";
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
    public void HumanBankrupt() // BANKRUPTPANEL YES BUTTON"
    {
        playerList[currentPlayer].Bankrupt();
        Continue();
    }

    // ------------------------------------------------------- İNSAN - HAPİSTEN ÇIKMA KARTI BUTONLARI ------------------------------------
    public void Jail1CardButtonEvent() // ŞANS KARTI
    {
        playerList[currentPlayer].UseChanceJailFreeCard();
    }
    public void Jail2CardButtonEvent() // KAMU KURULUŞU KARTI
    {
        playerList[currentPlayer].UseCommunityJailFreeCard();
    }

    public void PayToFreeJailButtonEvent()
    {
        playerList[currentPlayer].PayToFree();
        UpdateJailButtons();
    }

    void UpdateJailButtons()
    {
        Player player = playerList[currentPlayer];
        bool isInJail = player.IsInJail;

        payToFreeButton.interactable = isInJail;
        jailFreeCardButton1.interactable = isInJail;
        jailFreeCardButton2.interactable = isInJail;
    }
}