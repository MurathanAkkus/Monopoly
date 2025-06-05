using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ManageCardUi : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image colorField;
    [SerializeField] GameObject[] buildings;
    [SerializeField] TMP_Text cardNameText;
    [Space]

    [SerializeField] TMP_Text mortgagedText;
    [SerializeField] TMP_Text mortgageValueText;
    [SerializeField] Button mortgageButton, unMortgageButton;
    [Space]

    [SerializeField] Image iconImage;
    [SerializeField] Sprite propertySprite, railroadSprite, utilitySprite;
    
    Player playerReference;
    MonopolyNode nodeReference;
    ManagePropertyUi propertyReference;

    public delegate void UpdateManageMessage(string message);
    public static UpdateManageMessage OnUpdateManageMessage;

    string msg;

    // publi void Initialize(Color setColor, int numberOfBuildings, bool isMortgaged, int mortgageValue)
    public void SetCard (MonopolyNode node, Player owner, ManagePropertyUi propertySet)
    {
        nodeReference = node;
        playerReference = owner;
        propertyReference = propertySet;
        if (node.propertyColorField != null)
            colorField.color = node.propertyColorField.color;
        else
            colorField.color = Color.black;
        
        ShowBuildings();

        // İPOTEKLİ OLDUĞUNU GÖSTER
        mortgagedText.gameObject.SetActive(node.IsMortgaged);
        // İPOTEK DEĞERİNİ GÜNCELLE
        mortgageValueText.text = "İpotek Değeri : " + node.MortgageValue + "M";

        // BUTONLAR
        mortgageButton.interactable = !node.IsMortgaged;
        unMortgageButton.interactable = node.IsMortgaged;

        // ICONu AYARLA
        switch (nodeReference.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                iconImage.sprite = propertySprite;
            break;
            case MonopolyNodeType.Railroad:
                iconImage.sprite = railroadSprite;
            break;
            case MonopolyNodeType.Utility:
                iconImage.sprite = utilitySprite;
            break;
        }

        // İSİMLERİ AYARLA
        cardNameText.text = nodeReference.name;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            Player currentPlayer = GameManager.instance.GetCurrentPlayer;
            switch (nodeReference.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    MonopolyNode.OnShowPropertyBuyPanel?.Invoke(nodeReference, currentPlayer, false);
                    break;
                case MonopolyNodeType.Railroad:
                    MonopolyNode.OnShowRailroadBuyPanel?.Invoke(nodeReference, currentPlayer, false);
                    break;
                case MonopolyNodeType.Utility:
                    MonopolyNode.OnShowUtilityBuyPanel?.Invoke(nodeReference, currentPlayer, false);
                    break;
            }
        }
    }
    public void MortgageButtonEvent()
    {
        if (!propertyReference.CheckIfMortgageAllowed())
        {
            // HATA MESAJI
            msg = "İpoteklemen için kartında <b>ev</b> veya <b>otel</b> bulunmaması gereklidir.";
            return;
        }
        if (nodeReference.IsMortgaged)
        {
            msg = "Bu kart ZATEN ipotekli.";
            // HATA MESAJI
            return;
        }
        playerReference.CollectMoney(nodeReference.MortgageProperty());
        mortgagedText.gameObject.SetActive(true);
        mortgageButton.interactable = false;
        unMortgageButton.interactable = true;
        ManageUi.instance.UpdateMoneyText();

        OnUpdateManageMessage.Invoke($"{nodeReference.name} <u>ipoteklendi</u>.");
    }
    
    public void UnMortgageButtonEvent()
    {
        if (!nodeReference.IsMortgaged)
        {
            // HATA MESAJI
            msg = "Bu kart zaten ipoteklenmemiş";
            return;
        }

        if (playerReference.ReadMoney < nodeReference.MortgageValue)
        {
            // HATA MESAJI
            msg = "Hesabındaki para ipoteği kaldırmak için yeterli değil.";
            return;
        }

        int unMortgageValue = nodeReference.MortgageProperty() + nodeReference.MortgageProperty() / 10;
        playerReference.PayMoney(unMortgageValue); 
        nodeReference.UnMortgageProperty();
        mortgagedText.gameObject.SetActive(false);
        mortgageButton.interactable = true;
        unMortgageButton.interactable = false;
        ManageUi.instance.UpdateMoneyText();

        OnUpdateManageMessage.Invoke($"{nodeReference.name} kartının <u>ipoteği kaldırıldı</u>.");
    }

    public void ShowBuildings()
    {
        // TÜM BİNALARI GİZLE
        foreach (var icon in buildings)
        {
            icon.SetActive(false);
        }

        // BİNALARI GÖSTER
        if(nodeReference.NumberOfHouses < 5)
        {   
            // Debug.Log("Nodetaki ev sayısı" + node.NumberOfHouses);
            for (int i = 0; i < nodeReference.NumberOfHouses; i++)
            {
                buildings[i].SetActive(true);
                msg = "Bir <b><color=green>ev</color></b> inşa ettin.";
            }
        }
        else
        {
            buildings[4].SetActive(true);
            msg = "Bir <b><color=red><u>otel</u></color></b> inşa ettin.";
        }
        OnUpdateManageMessage.Invoke(msg);
    }
}