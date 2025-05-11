using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class Board : MonoBehaviour
{
    public static Board instance;

    public List<MonopolyNode> route = new List<MonopolyNode>();

    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodesInSetList = new List<MonopolyNode>();
    }

    public List <NodeSet> nodeSetList = new List<NodeSet>();

    void Awake()
    {
        instance = this;
    }
    void OnValidate()
    {
        route.Clear();
        foreach (Transform node in transform.GetComponentInChildren<Transform>())
        {
            route.Add(node.GetComponent<MonopolyNode>());
        }

        /*// TÜM NODE RENKLERİNİ GÜNCELLE
        for (int i = 0; i < nodeSetList.Count; i++)
        {
            for (int j = 0; j < nodeSetList[i].nodesInSetList.Count; j++)
            {
                nodeSetList[i].nodesInSetList[j].UpdateColorField(nodeSetList[i].setColor);
            }
        }*/
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
        GameObject token = player.MyToken; // TOKENİN HAREKETİ İÇİN
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);
        bool moveOverGo = false;
        bool isMovingForward = steps > 0;
        if(isMovingForward)
        {
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
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;

                // HAREKETİ GERÇEKLEŞTİR
                while (MoveToNextNode(token, endPos,20))
                    yield return null;
                
                stepsLeft--;
            }
        }
        else
        {
            while (stepsLeft<0)
            {
                indexOnBoard--;
                // HAREKET BİTTİ Mİ?
                if (indexOnBoard < 0)
                {
                    indexOnBoard = route.Count-1;
                }
                // BAŞLANGIÇ VE BİTİŞ POZİSYONLARINI AL
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;

                // HAREKETİ GERÇEKLEŞTİR
                while (MoveToNextNode(token, endPos,20))
                    yield return null;
                
                stepsLeft++;
            }
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

    public void MovePlayerToken(MonopolyNodeType type, Player player) // EN YAKIN VERİLEN NODE a HAREKET ETTİRİYOR
    {
        int indexOfNextNodeType = -1; // INDEX i BULMAK İÇİN
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode); // OYUNCU NEREDE
        int startSearchIndex = (indexOnBoard + 1) % route.Count;
        int nodeSearches = 0;
        
        while (indexOfNextNodeType == -1 && nodeSearches < route.Count) // ARAMAYA DEVAM EDER
        {
            if(route[startSearchIndex].monopolyNodeType == type)
                indexOfNextNodeType = startSearchIndex;
            
            startSearchIndex = (startSearchIndex + 1) % route.Count;
            nodeSearches++;
        }
        if(indexOfNextNodeType == -1)
        {
            Debug.LogError("NODE BULUNAMADI!");
            return;
        }

        StartCoroutine(MovePlayerInSteps(nodeSearches, player));
    }

    bool MoveToNextNode(GameObject tokenToMove, Vector3 endPos, float speed)
    {   // SON POZİSYONA GELMEDİYSE İLERLE
        return endPos != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPos, speed * Time.deltaTime));
    }

    public (List<MonopolyNode> list, bool allSame) PlayerHasAllNodesOfSet(MonopolyNode node)
    {   // TEK BİR NODEa BAKARAK, SETTEKİ TÜM ARSALAR AYNI SAHİBE mi AİT?
        bool allSame = false;
        foreach (var nodeSet in nodeSetList)
        {
            if (nodeSet.nodesInSetList.Contains(node))
            {
                // LINQ
                allSame = nodeSet.nodesInSetList.All(_node => _node.Owner == node.Owner);
                return (nodeSet.nodesInSetList, allSame);
            }
        }
        return (null, allSame);
    }

    // ------------------------------------- SETTEKİ EKSİK DÜĞÜMLERİ TALEP ET -------------------------------------

    // ------------------------------------- NODELAR ARASINDAKİ MESAFEYİ HESAPLA ----------------------------------
}