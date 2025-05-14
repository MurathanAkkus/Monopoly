using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using UnityEngine.UI;

public class TradingSystem : MonoBehaviour
{
    public static TradingSystem instance;

    [Header("")]

    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] TMP_Text resultMessageText;
    [Space]
    [Header("Left Side")]
    [SerializeField] TMP_Text leftOffererNameText;
    [SerializeField] Transform leftCardGrid;
    [SerializeField] ToggleGroup leftToggleGroup;
    [SerializeField] TMP_Text leftYourMoneyText;
    [SerializeField] TMP_Text leftOfferMoney;
    [SerializeField] Slider leftMoneySlider;
    List<GameObject> leftCardPrefabList = new List<GameObject>();
    int leftChoosenMoneyAmount;
    MonopolyNode leftSelectedNode;
    Player leftPlayerReference;

    [Header("Middle")]
    [SerializeField] Transform buttonGrid;
    [SerializeField] GameObject playerButtonPrefab;
    List<GameObject> playerButtonList = new List<GameObject>();

    [Header("Right Side")]
    [SerializeField] TMP_Text rightOffererNameText;
    [SerializeField] Transform rightCardGrid;
    [SerializeField] ToggleGroup rightToggleGroup;
    [SerializeField] TMP_Text rightYourMoneyText;
    [SerializeField] TMP_Text rightOfferMoney;
    [SerializeField] Slider rightMoneySlider;
    List<GameObject> rightCardPrefabList = new List<GameObject>();
    int rightChoosenMoneyAmount;
    MonopolyNode rightSelectedNode;
    Player rightPlayerReference;


    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    string msg;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        tradePanel.SetActive(false);
        resultPanel.SetActive(false);
    }
    // ---------------------------- AI İÇİN - SETTE EKSİK OLAN MÜLKLERİ BUL ------------------------------
    public void FindMissingProperty(Player currentPlayer)
    {
        List<MonopolyNode> processedSet = null;
        MonopolyNode requestedNode = null;
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            // list: node'un ait olduğu setteki diğer nodeların listesi
            // allsame: bu setteki tüm nodelar aynı oyuncuya mı ait?
            var (list, allsame) = Board.instance.PlayerHasAllNodesOfSet(node); // node un AİT OLDUĞU SETİ KONTROL EDER
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);

            // HEPSİ SATIN ALINMAMIŞ ALDI MI?
            bool notAllPurchased = list.Any(n => n.Owner == null);

            if(allsame || processedSet == nodeSet)
            {
                processedSet = nodeSet;
                continue;
            }

            // DİĞER OYUNCULARIN SAHİP OLDUKLARINI BUL
            // ORTALAMADAN FAZLASINA SAHİP OLUP OLMADIĞIMIZI KONTROL ET
            if (list.Count == 2)
            {
                requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                if (requestedNode != null)
                {
                    // NODEun SAHİBİNE TEKLİF OLUŞTUR
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
            if(list.Count >= 3)
            {
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);
                if (hasMostOfSet >= 2)
                {
                    requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                    // NODEun SAHİBİNE TEKLİF OLUŞTUR
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
        }
    }

    // ---------------------------- AI İÇİN - TİCARET TEKLİFİ OLUŞTURMAYA KARAR VER ----------------------
    void MakeTradeDecision(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode)
    {
        // PARAMIZ MÜMKÜN OLDUĞUNCA İLE TİCARET
        if(currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode))
        {
            // TİCARET
            MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, null, CalculateValueOfNode(requestedNode), 0);
            return;
        }
        // TÜM EKSİK SETLERİ BUL VE İSTENEN DÜĞÜMÜ İÇEREN SETİ HARİÇ TUT
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            var checkedSet = Board.instance.PlayerHasAllNodesOfSet(node).list;
            if (checkedSet.Contains(requestedNode))
            {
                // KONTROL BİTİR
                continue;
            }

            /*
                Sadece para ile takas yapmak istiyorsak: Teklif edilen node null olur. offeredMoney ise node’un değeri kadar olur.
                Para + mülk ile takas yapmak istiyorsak: Mevcut mülklerden birini seçip offeredNode olarak geçeriz.
                    Geri kalan fark için offeredMoney'i hesaplarız (örneğin: istediğim mülk değeri - teklif ettiğim mülk değeri).
            */
            // UYGUN NODEu KONTROL ET
            if (checkedSet.Count(n => n.Owner == currentPlayer) == 1)
            {
                if (CalculateValueOfNode(node) + currentPlayer.ReadMoney >= requestedNode.price)
                {
                    int difference = Mathf.Abs(CalculateValueOfNode(requestedNode) - CalculateValueOfNode(node));

                    // MÜMKÜN OLAN TİCARET
                    if (difference > 0)
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, difference, 0);
                    else
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(difference));
                    // TİCARET TEKLİFİ OLUŞTUR
                    break;
                }
            }
        }

        // BULUNAN SETİN SADECE BİR DÜĞÜMÜNE SAHİP OLUNUP OLUNMADIĞINI ÖĞREN

        // O DÜĞÜMÜN DEĞERİNİ HESAPLA VE YETERLİ PARAYLA SATIN ALINABİLİR Mİ BAK

        // BU KOŞULLAR SAĞLANMIŞSA TAKAS TEKLİFİ YAP

    }

    // ---------------------------- TİCARET TEKLİFİ OLUŞTUR ----------------------------------------------

    // İstek yapan oyuncu (currentPlayer): Takas teklifini başlatan oyuncudur.
    // Node sahibi (nodeOwner): Karşı oyuncu. Ticaret yaptığımız oyuncu.
    // Teklif edilen node (offeredNode)
    // Teklif edilen para (offeredMoney)
    // İstenen mülk node (requestedNode)
    // İstenen para (requestedMoney): Örneğin karşı tarafın bize para iadesi yapması gerekebilir.

    void MakeTradeOffer (Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        if(nodeOwner.playerType == Player.PlayerType.AI)
        {
            ConsiderTradeOffer(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
        else if(nodeOwner.playerType == Player.PlayerType.HUMAN)
        {
            // UI GÖSTER
        }
    }
    // ---------------------------- AI İÇİN - TİCARET TEKLİFİNİ DEĞERLENDİRME ----------------------------
    void ConsiderTradeOffer (Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        // TİCARETİN GERÇEK DEĞERİNİ HESAPLAMAMIZ GEREKİYOR
        int valueOfTheTrade = (CalculateValueOfNode(requestedNode) + requestedMoney) - (CalculateValueOfNode(offeredNode) + offeredMoney)  ;
        //  İSTENEN     -   VERİLEN
        // 200 + 200    >   200 + 100

        Debug.Log(CalculateValueOfNode(requestedNode) +" + "+ requestedMoney +" - ("+ CalculateValueOfNode(offeredNode) +" + "+offeredMoney + ")");
        // PARA KAZANMAK İÇİN BİR NODE SAT
        if (requestedNode == null && offeredNode != null && requestedMoney <= (nodeOwner.ReadMoney / 3)  && !Board.instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            TradeResult(true);
            Debug.Log("AI TEKLİFİNİ PARADAN DOLAYI KABUL ETTİ");
            msg = "<color=green>kabul etti</color>.";
            return;
        }

        // NORMAL BİR TİCARET
        if(valueOfTheTrade <= 0 && !Board.instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            // NODE TİCARETİ GEÇERLİ
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            TradeResult(true);
            Debug.Log("AI TEKLİFİNİ KABUL ETTİ");
            msg = "<color=green>kabul etti</color>.";
        }
        else
        {
            TradeResult(false);
            Debug.Log("AI TEKLİFİNİ REDDETTİ");
            msg = "<color=red>reddetti</color>.";
        }

        Debug.Log($"{currentPlayer.name} {requestedNode.name} istedi ve {nodeOwner.name} teklifi " + msg);

        // UI İÇİN BİR MESAJ GÖNDER
        string offeredNodeName = (offeredNode != null) ? $" ve {offeredNode.name} kartı" : ""; // TEKLİFİNDE NODE YOKSA
        int resultMoney = offeredMoney - requestedMoney;
        //if(requestedMoney >)
        OnUpdateMessage.Invoke($"{currentPlayer.name} {requestedNode.name} kartını almak için, /*{offeredMoney}M {offeredNodeName}*/ karşılığında, {nodeOwner.name} bu teklifi " + msg);
    }

    // ---------------------------- AI İÇİN - NODEun DEĞERİNİ HESAPLAMA ----------------------------------
    int CalculateValueOfNode (MonopolyNode requestedNode)
    {
        int value = 0;
        if(requestedNode != null)
        {
            if (requestedNode.monopolyNodeType == MonopolyNodeType.Property)
            {
                value = requestedNode.price + requestedNode.NumberOfHouses * requestedNode.houseCost;
            }
            else
            {
                value = requestedNode.price;
            }
            return value;
        }

        return value;
    }

    // ---------------------------- NODE TİCARETİ --------------------------------------------------------
    void Trade (Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        // currentPlayer(İSTEK YAPAN OYUNCU) GEREKİYOR
        if (requestedNode != null)
        {
            currentPlayer.PayMoney(offeredMoney);
            requestedNode.ChangeOwner(currentPlayer);

            // NODE SAHİBİNE ->
            nodeOwner.CollectMoney(offeredMoney);
            nodeOwner.PayMoney(requestedMoney);

            if (offeredNode != null)
                offeredNode.ChangeOwner(nodeOwner);
        }

        else if (offeredNode != null && requestedNode == null)
        {
            currentPlayer.CollectMoney(requestedMoney);
            nodeOwner.PayMoney(requestedMoney);
            offeredNode.ChangeOwner(nodeOwner);

            // UI İÇİN BİR MESAJ GÖNDER
            OnUpdateMessage.Invoke($"{currentPlayer.name} {offeredNode.name} kartını {nodeOwner.name} oyuncusuna {requestedMoney}M karşılığında sattı");
        }
    
        // UI GİZLE İNSANLAR İÇİN
        CloseTradePanelButtonEvent();
    }

    // ---------------------------- UI İÇERİĞİ -----------------------------------------------------------

    // ---------------------------- ŞİMDİKİ OYUNCU -------------------------------------------------------
    void CreateLeftPanel ()
    {
        leftOffererNameText.text = leftPlayerReference.name;

        List<MonopolyNode> referenceNodes = leftPlayerReference.GetMonopolyNodes;
        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, leftCardGrid, false);

            // KART İÇERİĞİNİ AYARLA
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], leftToggleGroup);
            leftCardPrefabList.Add(tradeCard);
        }
        leftYourMoneyText.text = $"Bakiye : {leftPlayerReference.ReadMoney}M";

        // SLIDER VE TEXT AYARLA
        leftMoneySlider.maxValue = leftPlayerReference.ReadMoney;
        leftMoneySlider.value = 0;
        UpdateLeftSlider(leftMoneySlider.value);
        //leftMoneySlider.onValueChanged.AddListener(UpdateLeftSlider);

        // ESKİ İÇERİĞİ SIFIRLA


        tradePanel.SetActive(true);
    }

    public void UpdateLeftSlider (float value)
    {
        leftOfferMoney.text = $"Vereceğin Para : {leftMoneySlider.value.ToString()}M";
    }

    public void CloseTradePanelButtonEvent()
    {
        tradePanel.SetActive(false);

        ClearAll();
    }

    public void OpenTradePanelButtonEvent()
    {
        leftPlayerReference = GameManager.instance.GetCurrentPlayer;
        rightOffererNameText.text = "Bir Oyuncu seç";

        CreateLeftPanel();
        CreateMiddleButtons();
    }

    // ---------------------------- SEÇİLEN OYUNCU -------------------------------------------------------
    public void ShowRightPlayer(Player player)
    {
        //Debug.Log("ShowRightPlayer çağrıldı.");
        rightPlayerReference = player;
        
        // ŞU ANKİ İÇERİĞİ SIFIRLA
        ClearRightPanel();
        //Debug.Log("ClearRightPanel çağrılıyor.");

        // ORTAKİ BUTONA TIKLANINCA SAĞDAKİ PANELDE O OYUNCUYU GÖSTER
        rightOffererNameText.text = rightPlayerReference.name;
        //Debug.Log("Oyuncu ismi güncelleniyor.");

        List<MonopolyNode> referenceNodes = rightPlayerReference.GetMonopolyNodes;
        //Debug.Log($"Node sayısı: {referenceNodes.Count}");
        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, rightCardGrid, false);
            

            // KART İÇERİĞİNİ AYARLA
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], rightToggleGroup);

            rightCardPrefabList.Add(tradeCard);
        }
        rightYourMoneyText.text = $"Onun Bakiyesi : {rightPlayerReference.ReadMoney}M";
        //Debug.Log("Bakiye güncelleniyor...");

        // SLIDER VE TEXT AYARLA
        rightMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);
        //rightMoneySlider.onValueChanged.AddListener(UpdaterightSlider);

        // ESKİ İÇERİĞİ SIFIRLA

        // PARAYI VE SLİDERı BU OYUNCUYA GÖRE GÜNCELLE
    }

    // ORTAYI AYARLA
    void CreateMiddleButtons()
    {
        // İÇERİĞİ TEMİZLE
        for (int i = playerButtonList.Count-1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        // BÜTÜN OYUNCULAR
        List<Player> allPlayers = new List<Player>();
        allPlayers.AddRange(GameManager.instance.GetPlayers);
        allPlayers.Remove(leftPlayerReference);

        // VE BUTONLAR
        foreach (var player in allPlayers)
        {
            GameObject newPlayerButton = Instantiate(playerButtonPrefab, buttonGrid, false);
            newPlayerButton.GetComponent<TradePlayerButton>().SetPlayer(player);

            playerButtonList.Add(newPlayerButton);
        }
    }

    // TRADE SYSTEM i AÇARKEN VE KAPATIRKEN TEMİZLE
    void ClearAll()
    {
        // ORTADAKİ BUTTONLARI TEMİZLE
        rightOffererNameText.text = "Bir Oyuncu seç";
        rightYourMoneyText.text = "Onun Bakiyesi : 0";
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);

        for (int i = playerButtonList.Count-1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        // SOLDAKİ KARTLARI TEMİZLE
        for (int i = leftCardPrefabList.Count-1; i >= 0; i--)
        {
            Destroy(leftCardPrefabList[i]);
        }
        leftCardPrefabList.Clear();

        // SAĞDAKİ KARTLARI TEMİZLE
        for (int i = rightCardPrefabList.Count-1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
    }

    void ClearRightPanel()
    {
        // SAĞDAKİ KARTLARI TEMİZLE
        for (int i = rightCardPrefabList.Count-1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();

        // SLİDER SIFIRLA
        // SLIDER VE TEXT AYARLA
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);
    }

    public void UpdateRightSlider(float value)
    {
        rightOfferMoney.text = $"İsteyeceğin Para : {rightMoneySlider.value.ToString()}M";
    }

    // ---------------------------- TEKLİF YAP ------------------------------------------------------------
    public void MakeOfferButtonEvent()  // İNSAN İÇİN BUTTON
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;

        if (rightPlayerReference == null)
        {
            Debug.LogWarning("Sağdaki oyuncu bulunamadı!");
            return;
        }
        
        // SOLDAKİ SEÇİLEN NODE
        Toggle offeredToggle = leftToggleGroup.ActiveToggles().FirstOrDefault();
        if (offeredToggle != null)
        {
            offeredNode = offeredToggle.GetComponentInParent<TradePropertyCard>().Node();
        }

        // SAĞDAKİ SEÇİLEN NODE
        Toggle requestedToggle = rightToggleGroup.ActiveToggles().FirstOrDefault();
        if (requestedToggle != null)
        {
            requestedNode = requestedToggle.GetComponentInParent<TradePropertyCard>().Node();
        }

        MakeTradeOffer(leftPlayerReference, rightPlayerReference, requestedNode, offeredNode, (int) leftMoneySlider.value, (int) rightMoneySlider.value);
    }

    // ---------------------------- TEKLİF SONUCU ----------------------------------------------------------
    void TradeResult (bool accepted)
    {
        string decision;
        if (accepted)
        {
            decision = "<color=green>kabul etti</color>.";
        }
        else
        {
            decision = "<color=red>reddetti</color>.";
        }

        resultMessageText.text = $"{rightPlayerReference.name} teklifini {decision}.";

        resultPanel.SetActive(true);
    }
}
