using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

public class UiShowUtility : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Utility UI")]
    [SerializeField] GameObject utilityUiPanel;
    [SerializeField] TMP_Text utilityNameText;
    [SerializeField] Image colorField;
    [Space]

    [SerializeField] TMP_Text mortgagePriceText;
    [Space]

    [SerializeField] Button buyUtilityButton;
    [SerializeField] Button viewButton;
    [Space]

    [SerializeField] TMP_Text utilityPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowUtilityBuyPanel += ShowBuyUtilityPanel;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowUtilityBuyPanel -= ShowBuyUtilityPanel;
    }

    void Start()
    {
        utilityUiPanel.SetActive(false);
    }

    void ShowBuyUtilityPanel(MonopolyNode node, Player currentPlayer, bool allowBuy)
    {
        nodeReference = node;
        playerReference = currentPlayer;

        // EN ÜSTTEKİ PANEL
        utilityNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;

        // ORTADAKİ PANEL(KART)

        mortgagePriceText.text = node.MortgageValue.ToString() + "M";

        // ALTTAKİ PANEL
        utilityPriceText.text = "Fiyat : " + node.price.ToString();
        playerMoneyText.text = "Hesabında : " + currentPlayer.ReadMoney.ToString();

        buyUtilityButton.gameObject.SetActive(allowBuy);
        viewButton.gameObject.SetActive(!allowBuy);
        if (allowBuy)
        {   // SATIN ALMA BUTONU

            if (currentPlayer.CanAffordNode(node.price))
                buyUtilityButton.interactable = true;
            else
                buyUtilityButton.interactable = false;
        }
        else
        {
            if (nodeReference == null)
                Debug.LogWarning("node bulunamadı");
            viewButton.onClick.RemoveAllListeners();
            viewButton.onClick.AddListener(() => ViewManager.instance.ViewButtonEvent(nodeReference));
        }

        // PANELİ GÖSTER
        utilityUiPanel.SetActive(true);
        utilityUiPanel.transform.SetAsLastSibling();
    }

    public void BuyUtilityButtonEvent()
    {
        // SATIN ALMA
        playerReference.BuyProperty(nodeReference); // ARSA ALIRKENKİ AYNI FONKSİYON

        // BUTONU TIKLANAMAZ YAP
        buyUtilityButton.interactable = false;
    }

    public void CloseButtonEvent()
    {
        // PANELİ KAPAT
        utilityUiPanel.SetActive(false);
        // NODE UN REFERANSLARINI TEMİZLE - ARTIK GEREK KALMIYOR
        nodeReference = null;
        playerReference = null;
    }
}