using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public List<MonopolyNode> route = new List<MonopolyNode>();

    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodesInSetList = new List<MonopolyNode>();
    }

    [SerializeField] List <NodeSet> nodeSetList = new List<NodeSet>();

    void OnValidate()
    {
        route.Clear();
        foreach (Transform node in transform.GetComponentInChildren<Transform>())
        {
            route.Add(node.GetComponent<MonopolyNode>());
        }

        // TÜM NODE RENKLERİNİ GÜNCELLE
        for (int i = 0; i < nodeSetList.Count; i++)
        {
            for (int j = 0; j < nodeSetList[i].nodesInSetList.Count; j++)
            {
                nodeSetList[i].nodesInSetList[j].UpdateColorField(nodeSetList[i].setColor);
            }
        }
    }

    void OnDrawGizmos()
    {
        if(route.Count > 1)
        {
            for (int i = 0; i < route.Count; i++)
            {
                Vector3 current = route[i].transform.position;
                Vector3 next = (i+1<route.Count)?route[i+1].transform.position:current;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(current,next);
            }
        }       
    }

    public void MovePlayerToken(int steps, Player player)
    {
        StartCoroutine(MovePlayerInSteps(steps, player));
    }
    
    IEnumerator MovePlayerInSteps(int steps, Player player)
    {
        int stepsLeft = steps;
        GameObject tokenToMove = player.MyToken;
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);
        bool moveOverGo = false;
        while (stepsLeft>0)
        {
            indexOnBoard++;
            
            // HAREKET BİTTİ Mİ?
            if (indexOnBoard > route.Count-1)
            {
                indexOnBoard = 0;
                moveOverGo = true;
            }
            // BAŞLANGIÇ VE BİTİŞ POZİSYONLARINI AL
            Vector3 startPos = tokenToMove.transform.position;
            Vector3 endPos = route[indexOnBoard].transform.position;

            // HAREKETİ GERÇEKLEŞTİR
            while (MoveToNextNode(tokenToMove, endPos,20))
                yield return null;
            
            stepsLeft--;
        }
        // PARA ALMAYA GİT
        if(moveOverGo)
        {
            // OYUNCUDAN PARA TOPLA
            player.CollectMoney(GameManager.instance.GetGoMoney);

        }
        // ŞUANKİ OYUNCUNUN YENİ NODU'NU AYARLA
        player.SetMyCurrentNode(route[indexOnBoard]);
    }

    bool MoveToNextNode(GameObject tokenToMove, Vector3 endPos, float speed)
    {   // SON POZİSYONA GELMEDİYSE İLERLE
        return endPos != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPos, speed * Time.deltaTime));
    }


}
