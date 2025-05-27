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
        rollDiceButton.interactable = enableRollDice;
        endTurnButton.interactable = enableEndTurn;

        payToFree.gameObject.SetActive(enablePayToFree);
        jailFreeCard1.gameObject.SetActive(hasChanceJailCard);
        jailFreeCard2.gameObject.SetActive(hasCommunityJailCard);
    }
}
