using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Board gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] int currentPlayer;
    [SerializeField] int maxTurnsInJail = 3; // KODESTE KAÇ TUR KALACAĞINI AYARLAR
    [SerializeField] int startMoney = 1500;
    [SerializeField] int goMoney = 500;

    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel; // PlayerInfo Prefab'larının ebeveyn olarak kabul edilmesi için

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            playerList[i].Initialize(gameBoard.route[0], startMoney, info);
        } 
    }
}
