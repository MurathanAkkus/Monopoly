using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ManageUi : MonoBehaviour
{
    public static ManageUi instance;

    [SerializeField] GameObject managePanel;        // PANELİ GÖSTER VEYA GİZLE
    [SerializeField] Transform propertyGrid;        // PARENTTEKİ SETLERE GÖRE GRİD - SCROLL İÇİNDEKİ GRİD - YANİ BİR UI CONTAINER
    [SerializeField] GameObject propertySetPrefab;  // HER SET İÇİN PREFAB
    [Space]
    [SerializeField] TMP_Text yourMoneyText;
    [Space]
    [SerializeField] TMP_Text systemMessageText;


    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        managePanel.SetActive(false);
    }

    public void OpenManager() // YÖNET BUTONUNDAN ÇAĞIR
    {
        playerReference = GameManager.instance.GetCurrentPlayer;

        // TÜM NODELARI SET OLARAK AL
        // HER DÖNGÜDE NODEun AİT OLDUĞU SET, proccessedSet İLE KARŞILAŞTIRILIR
        List<MonopolyNode> proccessedSet = null;    // DAHA ÖNCE İŞLENEN SETLERİ TEKRAR İŞLEMEMEK İÇİN

        foreach (var node in playerReference.GetMonopolyNodes)
        {
            // list: node'un ait olduğu setteki diğer nodeların listesi
            // allsame: bu setteki tüm nodelar aynı oyuncuya mı ait?
            var (list, allsame) = Board.instance.PlayerHasAllNodesOfSet(node); // node un AİT OLDUĞU SETİ KONTROL EDER
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);

            if(nodeSet != null && list != proccessedSet)
            {   // Eğer bu set boş değilse ve zaten işlenmiş bir set değilse, devam et
                // ÖNCE PROCCESSED GÜNCELLE
                proccessedSet = list;

                // SADECE OYUNCUYA AİT NODELARI BIRAK
                nodeSet.RemoveAll(n => n.Owner != playerReference); // SETİN İÇİNDE DİĞER OYUNCULARA AİT MÜLK VARSA ÇIKART

                // OYUNCUNUN SAHİP OLDUĞU TÜM NODELAR İÇİN PREFABLER OLUŞTUR
                GameObject newPropertySet = Instantiate(propertySetPrefab, propertyGrid, false);
                // YENİ OLUŞTURULAN PREFABLERE ManagePropertyUi UYGULA
                newPropertySet.GetComponent<ManagePropertyUi>().SetProperty(nodeSet, playerReference);

                propertyPrefabs.Add(newPropertySet);
            }
        }
        managePanel.SetActive(true);
        UpdateMoneyText();
    }

    public void CloseManager()
    {
        managePanel.SetActive(false);

        for (int i = propertyPrefabs.Count-1; i >= 0; i--)
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney>=0) ? $"<color=green>{playerReference.ReadMoney}M</color>" : $"<color=red>{playerReference.ReadMoney}M</color>";
        yourMoneyText.text = $"<color=black>Hesabında:</color> " + showMoney; 
    }

    public void UpdateSystemMessage(string message)
    {
        systemMessageText.text = message;
    }
}
