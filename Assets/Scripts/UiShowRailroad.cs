using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UiShowRailroad : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Railroad UI")]
    [SerializeField] GameObject railroadUiPanel;
    [SerializeField] TMP_Text railroadNameText;
    [SerializeField] Image colorField;
    [Space]

    [SerializeField] TMP_Text oneRailRentText;
    [SerializeField] TMP_Text twoRailRentText;
    [SerializeField] TMP_Text threeRailRentText;
    [SerializeField] TMP_Text fourRailRentText;
    [Space]

    [SerializeField] TMP_Text mortgagePriceText;
    [Space]

    [SerializeField] Button buyRailroadButton;
    [SerializeField] Button viewButton;
    [Space]

    [SerializeField] TMP_Text railroadPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowRailroadBuyPanel += ShowBuyRailroadPanel;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowRailroadBuyPanel -= ShowBuyRailroadPanel;
    }

    void Start()
    {
        railroadUiPanel.SetActive(false);
    }

    void ShowBuyRailroadPanel(MonopolyNode node, Player currentPlayer, bool allowBuy)
    {
        nodeReference = node;
        playerReference = currentPlayer;

        // EN ÜSTTEKİ PANEL
        railroadNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;

        // ORTADAKİ PANEL(KART)
        // baseRent = 25/(2^0) = 50/(2^1) =  100/(2^2) = 200/(2^3)
        // result = baseRent * (int)Mathf.Pow(2, amount-1);
        oneRailRentText.text = node.baseRent * (int)Mathf.Pow(2, 1 - 1) + "M";
        twoRailRentText.text = node.baseRent * (int)Mathf.Pow(2, 2 - 1) + "M";
        threeRailRentText.text = node.baseRent * (int)Mathf.Pow(2, 3 - 1) + "M";
        fourRailRentText.text = node.baseRent * (int)Mathf.Pow(2, 4 - 1) + "M";
        mortgagePriceText.text = node.MortgageValue.ToString() + "M";

        // ALTTAKİ PANEL
        railroadPriceText.text = "Fiyat : " + node.price.ToString();
        playerMoneyText.text = "Hesabında : " + currentPlayer.ReadMoney.ToString();

        buyRailroadButton.gameObject.SetActive(allowBuy);
        viewButton.gameObject.SetActive(!allowBuy);
        if (allowBuy)
        {   // SATIN ALMA BUTONU
            if (currentPlayer.CanAffordNode(node.price))
                buyRailroadButton.interactable = true;
            else
                buyRailroadButton.interactable = false;
        }
        else
        {
            if (nodeReference == null)
                Debug.LogWarning("node bulunamadı");
            viewButton.onClick.RemoveAllListeners();
            viewButton.onClick.AddListener(() => ViewManager.instance.ViewButtonEvent(nodeReference));
        }

        // PANELİ GÖSTER
        railroadUiPanel.SetActive(true);
        railroadUiPanel.transform.SetAsLastSibling();
    }

    public void BuyRailroadButtonEvent()
    {
        // SATIN ALMA
        playerReference.BuyProperty(nodeReference); // ARSA ALIRKENKİ AYNI FONKSİYON

        // BUTONU TIKLANAMAZ YAP
        buyRailroadButton.interactable = false;
        UiShowPanel.instance.HideBuyButton();
    }

    public void CloseButtonEvent()
    {
        // PANELİ KAPAT
        railroadUiPanel.SetActive(false);
        // NODE UN REFERANSLARINI TEMİZLE - ARTIK GEREK KALMIYOR
        nodeReference = null;
        playerReference = null;
    }
}
