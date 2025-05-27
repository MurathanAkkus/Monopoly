using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager instance;

    [Header("For Camera UI")]
    [SerializeField] GameObject uiCanvas;
    [SerializeField] GameObject cardCanvas;
    [SerializeField] GameObject topDownCamUi;

    MonopolyNode activeHighlightedNode = null;


    void Awake()
    {
        instance = this;
    }

    public void ViewButtonEvent(MonopolyNode node)
    {
        if (GameManager.instance.IsBusy)
        {
            Debug.LogWarning("Oyun meşgulken kart görüntülenemez.");
            return;
        }
 
        cardCanvas.SetActive(false);
        uiCanvas.SetActive(false);

        node.ShowHighlightEffect();
        activeHighlightedNode = node;

        topDownCamUi.SetActive(true);
        CameraSwitcher.instance.SwitchToTopDown();
    }

    public void EyeViewButtonEvent()
    {
        if (GameManager.instance.IsBusy)
        {
            Debug.LogWarning("Oyun meşgulken kart görüntülenemez.");
            return;
        }
        cardCanvas.SetActive(false);
        uiCanvas.SetActive(false);

        CameraSwitcher.instance.SwitchToTopDown();
        topDownCamUi.SetActive(true);
    }


    public void ReturnButtonEvent()
    {
        cardCanvas.SetActive(true);
        uiCanvas.SetActive(true);

        if (activeHighlightedNode != null)
        {
            activeHighlightedNode.HideHighlightEffect();
            activeHighlightedNode = null;
        }

        topDownCamUi.SetActive(false);
        CameraSwitcher.instance.ReturnToLastCam();
        ShowHumanPanelForCurrentPlayer();
    }

    void ShowHumanPanelForCurrentPlayer()
    {
        Player currentPlayer = GameManager.instance.GetCurrentPlayer;

        if (currentPlayer.playerType != Player.PlayerType.HUMAN)
            return;

        bool hasRolled = GameManager.instance.HasRolledDice;
        bool rolledDouble = GameManager.instance.RolledADouble;

        bool canRollDice = !hasRolled || rolledDouble;
        bool canEndTurn = hasRolled && !rolledDouble;

        bool jail1 = currentPlayer.HasChanceFreeCard;
        bool jail2 = currentPlayer.HasCommunityFreeCard;
        bool showPayToGetOut = currentPlayer.IsInJail && !GameManager.instance.HasRolledDice;

        MonopolyNode.OnShowHumanPanel?.Invoke(true, canRollDice, canEndTurn, showPayToGetOut, jail1, jail2);
    }
}