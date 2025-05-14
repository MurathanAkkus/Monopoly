using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class TradePropertyCard : MonoBehaviour
{
    MonopolyNode nodeReference;

    [SerializeField] Image colorField;
    [SerializeField] TMP_Text nameText;
    [Space]
    [SerializeField] Image typeImage;
    [SerializeField] Sprite propertySprite, railRoadSprite, utilitySprite;
    [Space]
    [SerializeField] TMP_Text mortgagedText;
    [SerializeField] TMP_Text priceText;
    [Space]
    [SerializeField] Toggle toogleButton;

    public void SetTradeCard (MonopolyNode node, ToggleGroup toogleGroup)
    {
        nodeReference = node;
        colorField.color = (node.propertyColorField != null) ? node.propertyColorField.color : Color.black;
        nameText.text = node.name;
        
        switch (node.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                typeImage.sprite = propertySprite;
            break;
            case MonopolyNodeType.Railroad:
                typeImage.sprite = railRoadSprite;
            break;
            case MonopolyNodeType.Utility:
                typeImage.sprite = utilitySprite;
            break;
        }
        mortgagedText.gameObject.SetActive(node.IsMortgaged);
        priceText.text = $"{node.price}M";
        toogleButton.isOn = false;
        toogleButton.group = toogleGroup;
    }

    public MonopolyNode Node()
    {
        return nodeReference;
    }
}
