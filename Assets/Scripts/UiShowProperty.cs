using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UiShowProperty : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Property UI")]
    [SerializeField] GameObject propertyUiPanel;
    [SerializeField] TMP_Text propertyNameText;
    [SerializeField] Image colorField;
    [Space]

    [SerializeField] TMP_Text rentPriceText; // EVSİZ KİRA ÜCRETİ
    [SerializeField] TMP_Text oneHouseRentText;
    [SerializeField] TMP_Text twoHouseRentText;
    [SerializeField] TMP_Text threeHouseRentText;
    [SerializeField] TMP_Text fourHouseRentText;
    [SerializeField] TMP_Text hotelRentText;
    [Space]

    [SerializeField] TMP_Text houseAndHotelPriceText;
    [Space]

    [SerializeField] TMP_Text mortgagePriceText;

    [SerializeField] Button buyPropertyButton;
    [Space]

    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowPropertyBuyPanel += ShowBuyPropertyPanel;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowPropertyBuyPanel -= ShowBuyPropertyPanel;
    }

    void Start()
    {
        propertyUiPanel.SetActive(false);
    }

    void ShowBuyPropertyPanel(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        // EN ÜSTTEKİ PANEL
        propertyNameText.text = node.name;
        colorField.color = node.propertyColorField.color;

        // ORTADAKİ PANEL(KART)
        rentPriceText.text = node.baseRent.ToString() + "M";
        oneHouseRentText.text = node.rentWithHouses[0].ToString() + "M";
        twoHouseRentText.text = node.rentWithHouses[1].ToString() + "M";
        threeHouseRentText.text = node.rentWithHouses[2].ToString() + "M";
        fourHouseRentText.text = node.rentWithHouses[3].ToString() + "M";
        hotelRentText.text = node.rentWithHouses[4].ToString() + "M";
        mortgagePriceText.text = node.MortgageValue.ToString() + "M";

        // EVLERİN VE OTELİN ÜCRETİ
        houseAndHotelPriceText.text = node.houseCost.ToString() + "M"; // HER KARTTAKİ EV VE HOTEL İNŞA ETME BEDELİ AYNIDIR

        // ALTTAKİ PANEL
        propertyPriceText.text = "Fiyat : " + node.price.ToString();
        playerMoneyText.text = "Hesabında : " + currentPlayer.ReadMoney.ToString();

        // SATIN ALMA BUTONU
        if(currentPlayer.CanAffordNode(node.price))
        {
            buyPropertyButton.interactable = true;
        }
        else
        {
            buyPropertyButton.interactable = false;
        }

        // PANELİ GÖSTER
        propertyUiPanel.SetActive(true);
    }

    public void BuyPropertyButtonEvent()
    {
        // SATIN ALMA
        playerReference.BuyProperty(nodeReference);
        // ARSA KARTINI KAPAT

        // BUTONU TIKLANAMAZ YAP
        buyPropertyButton.interactable = false;
    }

    public void CloseButtonEvent()
    {
        // PANELİ KAPAT
        propertyUiPanel.SetActive(false);
        // NODE UN REFERANSLARINI TEMİZLE - ARTIK GEREK KALMIYOR
        nodeReference = null;
        playerReference = null;
    }
}
