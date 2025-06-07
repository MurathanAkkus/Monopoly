 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using TMPro;
using UnityEngine.UI;
using System;

public class TradingSystem : MonoBehaviour
{
    public static TradingSystem instance;

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
    Player rightPlayerReference;

    [Header("Trade Offer Panel")]
    [SerializeField] GameObject tradeOfferPanel;
    [SerializeField] TMP_Text leftPlayerText, rightPlayerText, leftMoneyText, rightMoneyText;
    [Space]
    [SerializeField] GameObject leftCard, rightCard;
    [SerializeField] Image leftColorField, rightColorField;
    [SerializeField] TMP_Text leftPropertyNameText, leftPropertyPriceText, rightPropertyPriceText, rightPropertyNameText;
    [SerializeField] Image leftImage, rightImage;
    [SerializeField] Sprite propertySprite, railRoadSprite, utilitySprite;

    // İNSAN OYUNCU İÇİN TEKLİFİ SAKLA
    Player currentPlayer, nodeOwner;
    MonopolyNode requestedNode, offeredNode;
    int requestedMoney, offeredMoney;

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        tradePanel.SetActive(false);
        resultPanel.SetActive(false);
        tradeOfferPanel.SetActive(false);
    }
    
    // ---------------------------- AI İÇİN - SETTE EKSİK OLAN MÜLKLERİ BUL ------------------------------
    public void FindMissingProperty(Player currentPlayer)
    {
        HashSet<string> processedSetNames = new HashSet<string>();
        MonopolyNode requestedNode = null;

        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            // list: node'un ait olduğu setteki diğer nodeların listesi
            // allsame: bu setteki tüm nodelar aynı oyuncuya mı ait?
            var (list, allsame) = Board.instance.PlayerHasAllNodesOfSet(node); // node un AİT OLDUĞU SETİ KONTROL EDER
            string setKey = string.Join(",", list.Select(n => n.name).OrderBy(n => n));
            // Zaten işlenen bir set mi?
            if (allsame || processedSetNames.Contains(setKey))
                continue;

            // Bu seti işlenmiş olarak işaretle
            processedSetNames.Add(setKey);

            // DİĞER OYUNCULARIN SAHİP OLDUKLARINI BUL
            // ORTALAMADAN FAZLASINA SAHİP OLUP OLMADIĞIMIZI KONTROL ET
            if (list.Count == 2) // UTILITY ve PROPERTY
            {
                requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                if (requestedNode != null)
                {
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
            if (list.Count >= 3) // PROPERTY
            {
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);
                if (hasMostOfSet >= 2)
                {
                    requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                    // NODEun SAHİBİNE TEKLİF OLUŞTUR
                    if (requestedNode != null)
                    {
                        MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                        break;
                    }
                }
            }
        }

        if (requestedNode == null)      // EKSİK MÜLK BULAMADIYSA
            currentPlayer.ChangeState(Player.AiStates.IDLE);
    }

    // ---------------------------- AI İÇİN - TİCARET TEKLİFİ OLUŞTURMAYA KARAR VER ----------------------
    void MakeTradeDecision(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode)
    {
        // PARAMIZ MÜMKÜN OLDUĞUNCA İLE TİCARET
        if (currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode))
        {
            // TİCARET
            MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, null, CalculateValueOfNode(requestedNode), 0);
            return;
        }

        bool foundDecision = false;

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
                    // dif = 600 - 300 > 0

                    // MÜMKÜN OLAN TİCARET
                    if (difference > 0)
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, difference, 0);
                    else // 300 - 600 < 0
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(difference));
                    // TİCARET TEKLİFİ OLUŞTUR
                    foundDecision = true;
                    break;
                }
            }
        }
        if (foundDecision)
            currentPlayer.ChangeState(Player.AiStates.IDLE);
        
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
            ShowTradeOfferPanel(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
    }

    // ---------------------------- AI İÇİN - TİCARET TEKLİFİNİ DEĞERLENDİRME ----------------------------
    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        // TİCARETİN GERÇEK DEĞERİNİ HESAPLAMAMIZ GEREKİYOR
        int requestedValue = CalculateValueOfNode(requestedNode);
        int offeredValue = CalculateValueOfNode(offeredNode);
        int valueOfTheTrade = offeredValue + offeredMoney - (requestedValue + requestedMoney);
        bool isSetCompleted = Board.instance.PlayerHasAllNodesOfSet(requestedNode).allSame;

        Debug.Log($"{offeredValue} + {offeredMoney} - ({requestedValue} + {requestedMoney}) = {valueOfTheTrade}");

        if (requestedNode == null && offeredNode != null && isSetCompleted)   // SADECE PARA KARŞILIĞINDA GELEN TEKLİF
        {   
            // Bu node için makul bir para istenmiş mi?
            if (valueOfTheTrade >= 0)
            {
                Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);

                if (currentPlayer.playerType == Player.PlayerType.HUMAN)
                    TradeResult(true);
                
                return;
            }
        }

        // NORMAL BİR TİCARET
        if (valueOfTheTrade >= 0 && !isSetCompleted)
        {
            // NODE TİCARETİ GEÇERLİ
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);

            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
                TradeResult(true);

            Debug.Log("AI TEKLİFİNİ KABUL ETTİ");
        }
        else
        {
            if (currentPlayer.playerType == Player.PlayerType.HUMAN)
                TradeResult(false);

            Debug.Log("AI TEKLİFİNİ REDDETTİ");

            // if (requestedNode != null)
            //     msg = "oyuncusundan <color=red>satın alınamadı</color>.";
            // else
            //     msg = "oyuncusuna <color=red>satılamadı</color>.";
        }
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
        }      
        // MESAJI YAYINLA
        OnUpdateMessage?.Invoke(CreateTradeMessage(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney));

        // UI GİZLE İNSANLAR İÇİN
        CloseTradePanelButtonEvent();

        if (currentPlayer.playerType == Player.PlayerType.AI)
            currentPlayer.ChangeState(Player.AiStates.IDLE);
    }

    // Mesajı oluşturmak için yardımcı fonksiyon
    string CreateTradeMessage(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int resultMoney = offeredMoney - requestedMoney;
        Debug.LogWarning("Test!");
        // 1. Takas: Kart + Para <=> Kart
        if (requestedNode != null && offeredNode != null)
        {
            return $"<b>{currentPlayer.name}</b>, <u>{offeredNode.name}</u> kartı ve <color=yellow>{resultMoney}M</color> karşılığında <b>{nodeOwner.name}</b> oyuncusundan <u>{requestedNode.name}</u> kartını aldı.";
        }
        // 2. Sadece satın alma (para karşılığı kart alındı)
        else if (requestedNode != null && offeredNode == null)
        {
            return $"<b>{currentPlayer.name}</b>, <color=yellow>{resultMoney}M</color> ödeyerek <b>{nodeOwner.name}</b> oyuncusundan <u>{requestedNode.name}</u> kartını satın aldı.";
        }
        // 3. Sadece satış (kart karşılığı para alındı)
        else if (requestedNode == null && offeredNode != null)
        {
            return $"<b>{currentPlayer.name}</b>, <u>{offeredNode.name}</u> kartını <b>{nodeOwner.name}</b> oyuncusuna <color=yellow>{Math.Abs(resultMoney)}M</color> karşılığında sattı.";
        }
        // 4. Sadece para transferi (bağış)
        else if (requestedNode == null && offeredNode == null && resultMoney != 0)
        {
            if (resultMoney > 0)
                return $"<b>{currentPlayer.name}</b>, <b>{nodeOwner.name}</b> oyuncusuna <color=yellow>{resultMoney}M</color> bağış yaptı.";
            else
                return $"<b>{nodeOwner.name}</b>, <b>{currentPlayer.name}</b> oyuncusuna <color=yellow>{Math.Abs(resultMoney)}M</color> bağış yaptı.";
        }
        // 5. Hiçbir şey verilmedi veya başarısız
        else
        {
            return $"<b>{currentPlayer.name}</b> için ticaret işlemi başarısız oldu veya iptal edildi.";
        }
    }

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

        // ORTAKİ BUTONA TIKLANINCA SAĞDAKİ PANELDE O OYUNCUYU GÖSTER
        rightOffererNameText.text = rightPlayerReference.name;

        List<MonopolyNode> referenceNodes = rightPlayerReference.GetMonopolyNodes;
        for (int i = 0; i < referenceNodes.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, rightCardGrid, false);
            

            // KART İÇERİĞİNİ AYARLA
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNodes[i], rightToggleGroup);

            rightCardPrefabList.Add(tradeCard);
        }
        rightYourMoneyText.text = $"Onun Bakiyesi : {rightPlayerReference.ReadMoney}M";
        // SLIDER VE TEXT AYARLA
        rightMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);
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
            decision = "<color=green>kabul etti</color>.";
        
        else
            decision = "<color=red>reddetti</color>.";
        

        resultMessageText.text = $"{rightPlayerReference.name} teklifini {decision}.";

        resultPanel.SetActive(true);
    }

    // ---------------------------- TİCARET TEKLİFİ PANELİ -------------------------------------------------
    void ShowTradeOfferPanel(Player _currentPlayer, Player _nodeOwner, MonopolyNode _requestedNode, MonopolyNode _offeredNode, int _offeredMoney, int _requestedMoney)
    {
        // ASIL TEKLİF İLE EŞLEŞTİR
        currentPlayer = _currentPlayer;
        nodeOwner = _nodeOwner;
        requestedNode = _requestedNode;
        offeredNode = _offeredNode;
        requestedMoney = _requestedMoney;
        offeredMoney = _offeredMoney;

        // PANELİN İÇERİĞİNİ GÖSTER
        tradeOfferPanel.SetActive(true);
        leftPlayerText.text = currentPlayer.name + " teklifi: ";
        rightPlayerText.text = nodeOwner.name;
        leftMoneyText.text = $"{offeredMoney}M";
        rightMoneyText.text = $"{requestedMoney}M";
        leftCard.SetActive(offeredNode != null ? true : false);
        rightCard.SetActive(requestedNode != null ? true : false);

        if (leftCard.activeInHierarchy)
        {
            leftColorField.color = (offeredNode.propertyColorField != null) ? offeredNode.propertyColorField.color : Color.black;
        
            switch (offeredNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    leftImage.sprite = propertySprite;
                break;
                case MonopolyNodeType.Railroad:
                    leftImage.sprite = railRoadSprite;
                break;
                case MonopolyNodeType.Utility:
                    leftImage.sprite = utilitySprite;
                break;
            }

            leftPropertyNameText.text = offeredNode.name;
            leftPropertyPriceText.text = offeredNode.price.ToString();
        }

        if (rightCard.activeInHierarchy)
        {
            rightColorField.color = (requestedNode.propertyColorField != null) ? requestedNode.propertyColorField.color : Color.black;
        
            switch (requestedNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    rightImage.sprite = propertySprite;
                break;
                case MonopolyNodeType.Railroad:
                    rightImage.sprite = railRoadSprite;
                break;
                case MonopolyNodeType.Utility:
                    rightImage.sprite = utilitySprite;
                break;
            }

            rightPropertyNameText.text = requestedNode.name;
            rightPropertyPriceText.text = requestedNode.price.ToString();
        }
    }

    public void AcceptOfferButtonEvent()
    {
        Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        ResetOffer();
    }

    public void RejectOfferButtonEvent()
    {
        // TURA DEVAM
        if (currentPlayer != null)
        {
            Debug.LogWarning(currentPlayer.name);
            currentPlayer.ChangeState(Player.AiStates.IDLE);
        }
        else
            Debug.LogWarning("currentPlayer boş!");
        ResetOffer();
    }

    void ResetOffer()
    {
        currentPlayer = null;
        nodeOwner = null;
        requestedNode = null;
        offeredNode = null;
        requestedMoney = 0;
        offeredMoney = 0;
    }
}