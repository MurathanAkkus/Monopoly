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

    [Header("Player Info")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel; // PlayerInfo Prefab'larının ebeveyn olarak kabul edilmesi için
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();

    // ATILAN ZAR
    int[] rolledDice;
    bool rolledADouble;
    int doubleRollCount;

    // VERGİ HAVUZU
    int taxPoll = 0;

    // PARA ALMAK İÇİN GEÇ
    public int GetGoMoney => goMoney;
 
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
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            // RASTGELE TOKEN
            int randIndex = Random.Range(0, playerTokenList.Count);

            // BAŞLANGIÇ
            GameObject newToken = Instantiate(playerTokenList[randIndex], gameBoard.route[0].transform.position, Quaternion.identity);

            playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        } 
    }

    
    public void RollDice()  // INSAN VEYA AI TARAFINDAN ZAR ATMA BUTONUNA BAS
    {
        // SON ATILAN ZARI SIFIRLA
        rolledDice = new int[2];

        // ZAR AT VE SAKLA
        rolledDice[0] = Random.Range(1, 7);
        rolledDice[1] = Random.Range(1, 7);
        Debug.Log("Zarlar atildi: " + rolledDice[0] + " & " + rolledDice[1]);

        // ÇİFT Mİ?
        rolledADouble = rolledDice[0] == rolledDice[1]; // if(rolledDice[0] == rolledDice[1]) rolledADouble = true;

        // ARD ARDA 3 DEFA ÇİFT ATARSA -> KODESE -> TURU SONLANDIR


        // ZATEN HAPİSTE Mİ?


        // HAPİSTEN ÇIKABİLİR Mİ?


        // İZİN VERİLİRSE İLERLE
        rolledDice[0] = 2;
        rolledDice[1] = 2;
        StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));

        // UI GÖSTER VEYA GİZLE


    }
    IEnumerator DelayBeforeMove(int rolledDice)
    {
        yield return new WaitForSeconds(2f);

        // İLERLEMEYE İZİN VERİLİRSE
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);

        // İLERLEMEYE İZİN VERİLMEZSE
    }

    public void SwitchPlayer ()
    {
        currentPlayer++;
        // ÇİFT Mi ATILDI?


        // OYUNCU FAZLA MI?
        if (currentPlayer >= playerList.Count)
        {
            currentPlayer = 0;
        }

        // KODES KONTROL

        // OYUNCU AI MI?
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
        }


        // OYUNCU INSAN MI? - UI GÖSTER

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
}