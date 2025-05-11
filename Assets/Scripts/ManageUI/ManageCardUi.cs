using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class ManageCardUi : MonoBehaviour
{
    [SerializeField] Image colorField;
    [SerializeField] GameObject[] buildings;
    [Space]

    [SerializeField] TMP_Text mortgagedText;
    [SerializeField] TMP_Text mortgageValueText;
    [SerializeField] Button mortgageButton, unMortgageButton;
    
    Player playerReference;
    MonopolyNode nodeReference;
    ManagePropertyUi propertyReference;

    string msg;
    // publi void Initialize(Color setColor, int numberOfBuildings, bool isMortgaged, int mortgageValue)
    public void SetCard (MonopolyNode node, Player owner, ManagePropertyUi propertySet)
    {
        nodeReference = node;
        playerReference = owner;
        propertyReference = propertySet;
        if (node.propertyColorField != null || node.propertyColorField.color != null)
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
    }
    public void MortgageButtonEvent()
    {
        if (!propertyReference.CheckIfMortgageAllowed())
        {
            // HATA MESAJI
            msg = "<u>İpotek</u>lemen için kartında <b>ev</b> veya <b>otel</b> bulunmaması gereklidir.";
            return;
        }
        if (nodeReference.IsMortgaged)
        {
            msg = "Bu kart ZATEN <u>ipotek</u>li.";
            // HATA MESAJI
            return;
        }
        playerReference.CollectMoney(nodeReference.MortgageProperty());
        mortgagedText.gameObject.SetActive(true);
        mortgageButton.interactable = false;
        unMortgageButton.interactable = true;
        ManageUi.instance.UpdateMoneyText();

        msg = "Bu kart ipoteklendi.";
        ManageUi.instance.UpdateSystemMessage(msg);
    }
    public void UnMortgageButtonEvent()
    {
        if (!nodeReference.IsMortgaged)
        {
            // HATA MESAJI
            msg = "Bu kart zaten <b>ipotek</b>lenmemiş";
            return;
        }

        if (playerReference.ReadMoney < nodeReference.MortgageValue)
        {
            // HATA MESAJI
            msg = "Hesabındaki para <b>ipoteğ</b>i kaldırmak için yeterli değil.";
            return;
        }

        int unMortgageValue = nodeReference.MortgageProperty() + nodeReference.MortgageProperty() / 10;
        playerReference.PayMoney(unMortgageValue); 
        nodeReference.UnMortgageProperty();
        mortgagedText.gameObject.SetActive(false);
        mortgageButton.interactable = true;
        unMortgageButton.interactable = false;
        ManageUi.instance.UpdateMoneyText();

        msg = "Bu kartın <u>ipoteğ</u>i kaldırıldı.";
        ManageUi.instance.UpdateSystemMessage(msg);
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
                msg = "Bir <b>ev</b> inşa ettin.";
            }
        }
        else
        {
            buildings[4].SetActive(true);
            msg = "Bir <b>otel</b> inşa ettin.";
        }
        ManageUi.instance.UpdateSystemMessage(msg);
    }
}