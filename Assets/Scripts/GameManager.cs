using System.Collections;
using System.Collections.Generic;
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

    // ATILAN ZAR
    int[] rolledDice;
    bool rolledADouble;
  
    public bool RolledADouble => rolledADouble;
    public void ResetRolledADouble() => rolledADouble = false;
    int doubleRollCount;

    // VERGİ HAVUZU
    int taxPoll = 0;

    // PARA ALMAK İÇİN GEÇ
    public int GetGoMoney => goMoney;
    public float SecondsBetweenTurns => secondsBetweenTurns;
    public List<Player> GetPlayers => playerList;

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    // İNSANLAR İÇİN PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn);
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
        Initialize();
        if(playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
        }
        else
        {
            // İNSAN INPUT'LARI İÇİN UI GÖSTER
        }
        
    }

    void Initialize ()
    {
        for (int i = 0; i < playerList.Count; i++)
        {   // BÜTÜN OYUNCULARI OLUŞTUR
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            // RASTGELE TOKEN
            int randIndex = Random.Range(0, playerTokenList.Count);

            // BAŞLANGIÇ
            GameObject newToken = Instantiate(playerTokenList[randIndex], gameBoard.route[0].transform.position, Quaternion.identity);

            playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        }
        playerList[currentPlayer].ActivateSelector(true);

        if(playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
            OnShowHumanPanel.Invoke(true, true, false);
        else
            OnShowHumanPanel.Invoke(false, false, false);

    }

    
    public void RollDice()  // INSAN VEYA AI TARAFINDAN ZAR ATMA BUTONUNA BAS
    {
        bool allowedToMove = true;
        // SON ATILAN ZARI SIFIRLA
        rolledDice = new int[2];

        if(!DebugRoll)
        {   // ZAR AT VE SAKLA
            rolledDice[0] = Random.Range(1, 7);
            rolledDice[1] = Random.Range(1, 7);
        }
        else
        {   // DEBUG
            rolledDice[0] = rolledDice1;
            rolledDice[1] = rolledDice2;
        }
        Debug.Log("Zarlar atildi: " + rolledDice[0] + " & " + rolledDice[1]);

        // ÇİFT Mİ?
        rolledADouble = rolledDice[0] == rolledDice[1]; // if(rolledDice[0] == rolledDice[1]) rolledADouble = true;

        // ARD ARDA 3 DEFA ÇİFT ATARSA -> KODESE -> TURU SONLANDIR


        // ZATEN HAPİSTE Mİ?
        if(playerList[currentPlayer].IsInJail)
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();

            if(rolledADouble)
            {
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>kodesten çıkabilir</color>, çünkü <b>çift</b> zar attı");
                doubleRollCount++;

                // OYUNCUYU HAREKET ETTİR

            }
            else if(playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
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
            if(!rolledADouble)
                doubleRollCount = 0;
            
            else
            {
                doubleRollCount++;
                if(doubleRollCount >= 3)
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
        if(allowedToMove)
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " " + rolledDice[0] + " & " + rolledDice[1] + " attı");
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            // OYUNCU DEĞİŞTİRİLEBİLİR
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <b><color=red>kodeste</color></b> kalmalı!");
            StartCoroutine(DelayBetweenSwitchPlayer());
        }
        
        // UI GÖSTER VEYA GİZLE
        if(playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
            OnShowHumanPanel.Invoke(true, false, false);
        
    }
    IEnumerator DelayBeforeMove(int rolledDice)
    {
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

    public void SwitchPlayer ()
    {
        currentPlayer++;
        // ÇİFT Mi ATILDI?
        doubleRollCount = 0;

        // OYUNCU FAZLA MI?
        if (currentPlayer >= playerList.Count)
            currentPlayer = 0;
        
        DeactivateArrows();
        playerList[currentPlayer].ActivateSelector(true);
        // KODES KONTROL

        // OYUNCU AI MI?
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
            OnShowHumanPanel.Invoke(false, false, false);
        }
            
        else // OYUNCU INSAN MI? - UI GÖSTER
            OnShowHumanPanel.Invoke(true, true, false);
        

    }

    public int[] LastRolledDice => rolledDice;
    
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
        if(playerList.Count == 1)
        {   // KAZANAN OYUNCU
            string str = playerList[0].name + " OYUNU KAZANDI!";
            Debug.Log(str);
            OnUpdateMessage.Invoke(str);
            // OYUN DÖNGÜSÜNÜ DURDUR
            

            //UI GÖSTER
            
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
}