using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TradePropertyCard : MonoBehaviour, IPointerClickHandler
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

    public MonopolyNode Node()
    {
        return nodeReference;
    }
}
