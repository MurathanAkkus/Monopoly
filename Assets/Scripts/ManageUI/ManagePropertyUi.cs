using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

public class ManagePropertyUi : MonoBehaviour
{
    [SerializeField] Transform cardHolder; // YATAY LAYOUT
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Button buyHouseButton, sellHouseButton;
    [SerializeField] TMP_Text buyHousePriceText, sellHousePriceText;
    
    Player playerReference;
    List<MonopolyNode> nodesInSet = new List<MonopolyNode>();
    List<GameObject> cardsInSet = new List<GameObject>();

    [SerializeField] GameObject buttonBox;
    [SerializeField] GameObject cH; // CardHolder

    string msg;

    // SADECE 1 ARSA KARTI SETİ İÇİNDİR
    public void SetProperty(List<MonopolyNode> nodes, Player owner)
    {
        playerReference = owner;
        nodesInSet.AddRange(nodes);
        for (int i = 0; i < nodesInSet.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder, false);
            ManageCardUi manageCardUi = newCard.GetComponent<ManageCardUi>();
            cardsInSet.Add(newCard);
            manageCardUi.SetCard(nodesInSet[i], owner, this);
        }
        var (list, allsame) = Board.instance.PlayerHasAllNodesOfSet(nodesInSet[0]);
        if (allsame)
            Debug.Log($"SETİ TAMAMLADI - {playerReference.name}");
        buyHouseButton.interactable = allsame && CheckIfBuyAllowed();
        sellHouseButton.interactable = CheckIfSellAllowed();

        buyHousePriceText.text = "-" + nodesInSet[0].houseCost + "M";
        sellHousePriceText.text = "+" + (nodesInSet[0].houseCost / 2) + "M";

        // RailRoad ve Utility'de ev satma ve satın alma buttonları gözükmemeli
        if (nodes[0].monopolyNodeType != MonopolyNodeType.Property)
        {
            buttonBox.SetActive(false);
            HorizontalLayoutGroup hlg = cH.GetOrAddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 240;
        }
    }

    public void BuyHouseButtonEvent()
    {
        if(!CheckIfBuyAllowed())
        {
            // HATA MESAJI
            msg = "Setindeki kartlardan en az biri <u>ipotekle</u>ndiği için, ev inşa edemezsin.";
            ManageUi.instance.UpdateSystemMessage(msg);
            return;
        }
        if(playerReference.CanAffordHouse(nodesInSet[0].houseCost))
        {
            playerReference.BuildHouseOrHotelEvenly(nodesInSet);
            // OYUNCUNUN HESABINDAKİ PARAYI GÜNCELLE - UI
            UpdateHouseVisuals();
        }
        else
        {
            // EV ALMAK İÇİN YETERLİ PARA YOKSA, OYUNCUYA MESAJ GÖNDER
            msg = "Ev almak için yeterli paran bulunmamaktadır.";
            ManageUi.instance.UpdateSystemMessage(msg);
        }
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUi.instance.UpdateMoneyText();
    }

    public void SellHouseButtonEvent()
    {
        /*if(!CheckIfSellAllowed())
        {
            Debug.LogWarning("Satılacak ev yok : " + !CheckIfSellAllowed());
            return;
        }*/
        // EN AZ BİR EV VARSA DİYE KONTROL ET
        playerReference.SellHouseEvenly(nodesInSet);
        // OYUNCUNUN HESABINDAKİ PARAYI GÜNCELLE - UI
        UpdateHouseVisuals();

        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUi.instance.UpdateMoneyText();

        msg = "Bir <b>evini</b> veya <b>otelini</b> sattın.";
        ManageUi.instance.UpdateSystemMessage(msg);
    }

    public bool CheckIfSellAllowed()
    {
        if(nodesInSet.Any(n => n.NumberOfHouses > 0))
            return true;
        
        return false;
    }

    bool CheckIfBuyAllowed()
    {
        if(nodesInSet.Any(n => n.IsMortgaged == true))
            return false;
        
        return true;
    }

    public bool CheckIfMortgageAllowed()
    {
        if(nodesInSet.Any(n => n.NumberOfHouses > 0))
            return false;
        
        return true;
    }

    void UpdateHouseVisuals()
    {
        foreach (var card in cardsInSet)
        {
            card.GetComponent<ManageCardUi>().ShowBuildings();
        }
    }
}
