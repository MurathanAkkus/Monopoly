using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardList : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardGrid;
    [SerializeField] GameObject cardListPanel;

    List<GameObject> cardPrefabList = new List<GameObject>();

    public void OpenListButtonEvent()
    {
        CreateList();
        cardListPanel.SetActive(true);
    }

    void CreateList()
    {
        // Tüm board node'larını gez
        foreach (var node in Board.instance.route)
        {
            if (node.monopolyNodeType == MonopolyNodeType.Property ||
                node.monopolyNodeType == MonopolyNodeType.Railroad ||
                node.monopolyNodeType == MonopolyNodeType.Utility)
            {
                GameObject card = Instantiate(cardPrefab, cardGrid);
                card.GetComponent<CardPrefabForList>().SetPrefabCard(node); // Prefaba bilgileri aktar
                cardPrefabList.Add(card);
            }
        }
    }
}