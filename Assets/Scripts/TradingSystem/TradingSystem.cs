using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using UnityEditor.VersionControl;

public class TradingSystem : MonoBehaviour
{
    public static TradingSystem instance;

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    string msg;

    void Awake()
    {
        instance = this;
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
    // Node sahibi (nodeOwner): Elinde istediğimiz mülk bulunan oyuncudur.
    // İstenen mülk node (requestedNode)
    // Teklif edilen node (offeredNode)
    // Teklif edilen para (offeredMoney)
    // Talep edilen para (requestedMoney): Örneğin karşı tarafın bize para iadesi yapması gerekebilir.
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
        // 300 - 600 (-300) + 0 - 300 = -600
        // 300 + 300(requestMoney) - 600 + 0
        // (600 + 0requestMoney) - (300 + offerMoney300)
        // İSTENEN          VERİLEN
        // 200 + 200    >   200 + 100

        // PARA KAZANMAK İÇİN BİR NODE SAT
        if (requestedNode == null && offeredNode != null && requestedMoney <= nodeOwner.ReadMoney / 3)
        {
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            return;
        }

        // NORMAL BİR TİCARET
        if(valueOfTheTrade <= 0)
        {
            // NODE TİCARETİ GEÇERLİ
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            msg = "kabul etti";
        }
        else
        {
            Debug.Log("AI TEKLİFİNİ REDDETTİ");
            msg = "reddetti";
        }
        OnUpdateMessage.Invoke($"{currentPlayer.name} {requestedNode.name} istedi ve {nodeOwner.name} teklifi " + msg);
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
            
            // UI İÇİN BİR MESAJ GÖNDER
            string offeredNodeName = (offeredNode != null) ? " ve " + offeredNode.name : ""; // TEKLİFİNDE NODE YOKSA
            OnUpdateMessage.Invoke($"{currentPlayer.name} {requestedNode.name} kartını almak için, {offeredMoney}M ve {offeredNode.name} kartı karşılığında {nodeOwner.name} ile anlaştı.");
        }

        else if (offeredNode != null && requestedNode == null)
        {
            currentPlayer.CollectMoney(requestedMoney);
            nodeOwner.PayMoney(requestedMoney);
            offeredNode.ChangeOwner(nodeOwner);

            // UI İÇİN BİR MESAJ GÖNDER
            OnUpdateMessage.Invoke($"{currentPlayer.name} {offeredNode.name} kartını {nodeOwner.name} oyuncusuna {requestedMoney}M karşılığında sattı");
        }
    }

    // ---------------------------- NODE EKLEME - ÇIKARMA ------------------------------------------------
}
