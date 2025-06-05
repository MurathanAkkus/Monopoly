using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UiShowPanel : MonoBehaviour
{
    [SerializeField] GameObject humanPanel;
    [SerializeField] Button rollDiceButton;
    [SerializeField] Button endTurnButton;
    [Space]
    [SerializeField] Button payToFree;
    [SerializeField] Button jailFreeCard1;
    [SerializeField] Button jailFreeCard2;
    [Space]
    [SerializeField] Button showBuyPanelButton;
    [Space]
    [SerializeField] Button hideHumanPanel;
    [SerializeField] Button showHumanPanel;

    public static UiShowPanel instance;

    void Awake()
    {
        instance = this;
    }

    void OnEnable()
    {
        GameManager.OnShowHumanPanel += ShowPanel;
        MonopolyNode.OnShowHumanPanel += ShowPanel;
        CommunityChest.OnShowHumanPanel += ShowPanel;
        ChanceField.OnShowHumanPanel += ShowPanel;
        Player.OnShowHumanPanel += ShowPanel;
    }

    void OnDisable()
    {
        GameManager.OnShowHumanPanel -= ShowPanel;
        MonopolyNode.OnShowHumanPanel -= ShowPanel;
        CommunityChest.OnShowHumanPanel -= ShowPanel;
        ChanceField.OnShowHumanPanel -= ShowPanel;
        Player.OnShowHumanPanel -= ShowPanel;
    }

    void ShowPanel(bool showPanel, bool enableRollDice, bool enableEndTurn, bool enablePayToFree, bool hasChanceJailCard, bool hasCommunityJailCard)
    {
        humanPanel.SetActive(showPanel);
        hideHumanPanel.gameObject.SetActive(showPanel);

        bool isHuman = GameManager.instance.GetCurrentPlayer.playerType == Player.PlayerType.HUMAN;
        bool isHideButtonHidden = !hideHumanPanel.gameObject.activeSelf;
        showHumanPanel.gameObject.SetActive(isHuman && isHideButtonHidden);

        rollDiceButton.interactable = enableRollDice;
        endTurnButton.interactable = enableEndTurn;

        payToFree.gameObject.SetActive(enablePayToFree);
        jailFreeCard1.gameObject.SetActive(hasChanceJailCard);
        jailFreeCard2.gameObject.SetActive(hasCommunityJailCard);
    }

    public void ShowBuyButton(Player currentPlayer)
    {
        // Buton görünür mü olacak?
        MonopolyNode currentNode = currentPlayer.MyMonopolyNode;
        if (currentNode == null || currentNode.Owner != null )
        {
            showBuyPanelButton.gameObject.SetActive(false);
            return;
        }
        if (currentNode.monopolyNodeType == MonopolyNodeType.Property || currentNode.monopolyNodeType == MonopolyNodeType.Railroad || currentNode.monopolyNodeType == MonopolyNodeType.Utility)
        {
            showBuyPanelButton.gameObject.SetActive(true);
            showBuyPanelButton.onClick.RemoveAllListeners();
            showBuyPanelButton.onClick.AddListener(() => currentNode.PlayerLandedOnNode(currentPlayer));
        }
    }

    public void HideBuyButton()
    {
        showBuyPanelButton.gameObject.SetActive(false);
    }
}