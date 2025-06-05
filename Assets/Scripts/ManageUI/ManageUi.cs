using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using System.Data;
using System.Linq;

public class ManageUi : MonoBehaviour
{
    public static ManageUi instance;

    [SerializeField] GameObject managePanel;        // PANELİ GÖSTER VEYA GİZLE
    [SerializeField] Transform propertyGrid;        // PARENTTEKİ SETLERE GÖRE GRİD - SCROLL İÇİNDEKİ GRİD - YANİ BİR UI CONTAINER
    [SerializeField] GameObject propertySetPrefab;  // HER SET İÇİN PREFAB
    [Space]
    [SerializeField] TMP_Text yourMoneyText;

    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();

    public delegate void UpdateManageMessage(string message);
    public static UpdateManageMessage OnUpdateManageMessage;

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

        CreateProperties();

        managePanel.SetActive(true);
        UpdateMoneyText();
    }

    public void CloseManagerButtonEvent()
    {
        managePanel.SetActive(false);
        ClearProperties();

    }

    void ClearProperties()
    {
        for (int i = propertyPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }

    void CreateProperties()
    {
        // TÜM NODELARI SET OLARAK AL
        // HER DÖNGÜDE NODEun AİT OLDUĞU SET, proccessedSet İLE KARŞILAŞTIRILIR
        HashSet<string> processedSetKeys = new HashSet<string>();    // DAHA ÖNCE İŞLENEN SETLERİ TEKRAR İŞLEMEMEK İÇİN
        // SETTEKİ KARTLARDAN BİRDEN FAZLA GRİD OLUŞMAMASINI İSTİYORUZ, MESELA SETTEKİ KART SAYISI 2 VE BU YÜZDEN İKİ TANE AYNI SET GÖRÜNÜYOR
        foreach (var node in playerReference.GetMonopolyNodes)
        {
            // list: node'un ait olduğu setteki diğer nodeların listesi
            // _ = allsame: bu setteki tüm nodelar aynı oyuncuya mı ait?
            var (list, _) = Board.instance.PlayerHasAllNodesOfSet(node); // node un AİT OLDUĞU SETİ KONTROL EDER
            // SET ANAHTARI OLUŞTUR - ÖRN: Arnavutköy, Bahçelievler, Bakırköy
            string setKey = string.Join(",", list.Select(n => n.name).OrderBy(n => n));

            if (processedSetKeys.Contains(setKey)) // BU SET İŞLENDİ İSE ATLA
                continue;

            processedSetKeys.Add(setKey);
            // SADECE OYUNCUYA AİT NODELARI BIRAK
            List<MonopolyNode> playerNodesInSet = list.Where(n => n.Owner == playerReference).ToList();
            
            if (playerNodesInSet.Count > 0)
            {
                GameObject newPropertySet = Instantiate(propertySetPrefab, propertyGrid, false); // OYUNCUNUN SAHİP OLDUĞU TÜM NODELAR İÇİN PREFABLER OLUŞTUR
                newPropertySet.GetComponent<ManagePropertyUi>().SetProperty(playerNodesInSet, playerReference); // YENİ OLUŞTURULAN PREFABLERE ManagePropertyUi UYGULA
                propertyPrefabs.Add(newPropertySet);
            }
        }
    }

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney >= 0) ? $"<color=green>{playerReference.ReadMoney}M</color>" : $"<color=red>{playerReference.ReadMoney}M</color>";
        yourMoneyText.text = $"<color=black>Hesabında:</color> " + showMoney;
    }

    public void AutoHandleButtonEvent() // BUTTONDAN ÇAĞIRILIR
    {
        Debug.Log(playerReference.ReadMoney);
        if (playerReference.ReadMoney > 0)
        {
            OnUpdateManageMessage.Invoke("Borcun yok!");
            return;
        }
        playerReference.HandleInsufficientFunds(Mathf.Abs(playerReference.ReadMoney));

        // UI GÜNCELLE
        ClearProperties();
        CreateProperties();

        // MESSAGE SYSTEMe MESAJ GÖNDER
        OnUpdateManageMessage.Invoke("<color=blue><u>OBY</u></color> çalıştırıldı.");
        UpdateMoneyText();
    }
}
