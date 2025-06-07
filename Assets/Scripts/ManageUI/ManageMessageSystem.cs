using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ManageMessageSystem : MonoBehaviour
{
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject messageBoxPrefab;
    [SerializeField] private ScrollRect scrollRect;

    private Queue<GameObject> messageQueue = new Queue<GameObject>();
    private const int maxMessages = 20;

    private void OnEnable()
    {
        ManageUi.OnUpdateManageMessage += ShowManageMessage;
        ManageCardUi.OnUpdateManageMessage += ShowManageMessage;
        ManagePropertyUi.OnUpdateManageMessage += ShowManageMessage;
        MonopolyNode.OnUpdateManageMessage += ShowManageMessage;
    }

    private void OnDisable()
    {
        ManageUi.OnUpdateManageMessage -= ShowManageMessage;
        ManageCardUi.OnUpdateManageMessage -= ShowManageMessage;
        ManagePropertyUi.OnUpdateManageMessage -= ShowManageMessage;
        MonopolyNode.OnUpdateManageMessage -= ShowManageMessage;
    }

    public void ShowManageMessage(string message)
    {
        if (message != null)
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
}