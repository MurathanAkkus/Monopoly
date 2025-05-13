using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class MessageSystem : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    void OnEnable()
    {
        ClearMessage();
        GameManager.OnUpdateMessage += ReceiveMessage;
        Player.OnUpdateMessage += ReceiveMessage;
        MonopolyNode.OnUpdateMessage += ReceiveMessage;
        TradingSystem.OnUpdateMessage += ReceiveMessage;
    }

    void OnDisable()
    {
        GameManager.OnUpdateMessage -= ReceiveMessage;
        Player.OnUpdateMessage -= ReceiveMessage;
        MonopolyNode.OnUpdateMessage -= ReceiveMessage;
        TradingSystem.OnUpdateMessage -= ReceiveMessage;
    }

    void ReceiveMessage(string _message)
    {
        messageText.text = _message;
    }

    void ClearMessage()
    {
        messageText.text = "";
    }
}
