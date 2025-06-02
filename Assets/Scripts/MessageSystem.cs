using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MessageSystem : MonoBehaviour
{
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject messageBoxPrefab;
    [SerializeField] private ScrollRect scrollRect;

    private Queue<GameObject> messageQueue = new Queue<GameObject>();
    private const int maxMessages = 10;

    private void OnEnable()
    {
        GameManager.OnUpdateMessage += ShowMessage;
        Player.OnUpdateMessage += ShowMessage;
        MonopolyNode.OnUpdateMessage += ShowMessage;
        TradingSystem.OnUpdateMessage += ShowMessage;
    }

    private void OnDisable()
    {
        GameManager.OnUpdateMessage -= ShowMessage;
        Player.OnUpdateMessage -= ShowMessage;
        MonopolyNode.OnUpdateMessage -= ShowMessage;
        TradingSystem.OnUpdateMessage -= ShowMessage;
    }

    public void ShowMessage(string message)
    {
        GameObject newMessage = Instantiate(messageBoxPrefab, contentTransform);
        TMP_Text textComponent = newMessage.GetComponentInChildren<TMP_Text>();

        if (textComponent != null)
            textComponent.text = message;

        messageQueue.Enqueue(newMessage);

        if (messageQueue.Count > maxMessages)
        {
            GameObject oldMessage = messageQueue.Dequeue();
            Destroy(oldMessage);
        }

        // SCROLLU EN AŞAĞI KAYDIR
        Canvas.ForceUpdateCanvases(); // LAYOUTU GÜNCELLEMEYE ZORLA
        scrollRect.verticalNormalizedPosition = 0f;
    }
}