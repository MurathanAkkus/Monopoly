using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public enum MonopolyNodeType
{
    Mulk,
    Fatura,
    Demir,
    Vergi,
    Sans,
    KamuFonu,
    Baslangic,
    Kodes,
    Otopark,
    KodeseGit
}

public class MonopolyNode : MonoBehaviour
{
    public MonopolyNodeType monopolyNodeType;
    public Image propertyColorField;
    [Header("Text Name")]
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;

    [Header("Price")]
    public int price;
    [SerializeField] TMP_Text priceText;

    [Header("Rent")]
    [SerializeField] bool calculateRentAuto; // Kira hesaplasın mı?
    [SerializeField] int currentRent;  
    [SerializeField] internal int baseRent;
    [SerializeField] internal List<int> rentWithHouses = new List<int>();

    int numberOfHouses;
    
    [Header("Property Mortgage")]
    [SerializeField] GameObject mortgageImage;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;

    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    Player owner;

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    public Player Owner => owner;
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
    }
    
    void OnValidate()
    {
        // İsimi güncelle
        if (nameText != null)
            nameText.text = name;
            
        else
            Debug.LogError($"{gameObject.name} icinde 'Name Text' adli bir TMP_Text bulunamadi!");

        // KİRA HESAPLAMA
        if(calculateRentAuto)
        {
            if (monopolyNodeType == MonopolyNodeType.Mulk)
            {   
                if (baseRent > 0)
                {
                    price = 3 * baseRent * 10;
                    // İPOTEK DEĞERİ
                    mortgageValue = price / 2;
                    rentWithHouses.Clear();
                    rentWithHouses.Add(baseRent * 5);
                    rentWithHouses.Add(baseRent * 5 * 3);
                    rentWithHouses.Add(baseRent * 5 * 9);
                    rentWithHouses.Add(baseRent * 5 * 16);
                    rentWithHouses.Add(baseRent * 5 * 25);
                }
                else if(baseRent <= 0)
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouses.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Fatura)
                mortgageValue = price / 2;
                
            if (monopolyNodeType == MonopolyNodeType.Demir)
                mortgageValue = price / 2;
        }  
        if (priceText != null)
                priceText.text = price + " M";
        // SAHİPLİK GÜNCELLE
        OnOwnerUpdated();
        UnMortgageProperty();
        //isMortgaged = false;
    }

    public void UpdateColorField(Color color)
    {
        if(propertyColorField != null)
            propertyColorField.color = color;
    }

    // İPOTEK İÇERİĞİ
    public int MortgageProperty()
    {
        isMortgaged = true;
        if (mortgageImage != null)
            mortgageImage.SetActive(true);
            
        if (propertyImage != null)
            propertyImage.SetActive(false);

        return mortgageValue;
    }

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if (mortgageImage != null)
            mortgageImage.SetActive(false);

        if (propertyImage != null)
            propertyImage.SetActive(true);
    }

    public bool IsMortgaged => isMortgaged;
    public int MortgageValue => mortgageValue;

    // SAHİPLİK GÜNCELLE
    public void OnOwnerUpdated()
    {
        if (owner == null || ownerBar == null || ownerText == null)
        {
            //Debug.LogWarning($"[OnOwnerUpdated] {gameObject.name} için owner null!");
            return;
        }
            
        if (ownerBar != null)
        {
            if(owner != null)
            {
                ownerBar.SetActive(true);
                ownerText.text = owner.name;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "";
            }
        }
    }

    public void PlayerLandedOnNode(Player currentPlayer)
    {
        bool playerIsHuman = currentPlayer.playerType == Player.PlayerType.HUMAN;
        bool continueTurn = true;
        // NODE TİPİNE GÖRE KONTROL ET 
        switch(monopolyNodeType)
        {
            case MonopolyNodeType.Mulk:
                if(!playerIsHuman) // AI
                {
                    if(owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // BİR PLAYER'A KİRA ÖDE

                        // KİRA HESAPLA
                        //Debug.Log("OYUNCU KİRA ÖDEYEBİLİR VE BURANIN SAHİBİ: "+ owner.name);
                        int rentToPay = CalculatePropertyRent();
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);
                        
                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + " , " + owner.name + " OYUNCUSUNA " + rentToPay + " KİRA ÖDEDİ");
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        // NODE'U SATIN AL
                        //Debug.Log(currentPlayer.name + " SATIN ALABİLİR");
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + this.name + " SATIN ALDI");
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();
                        // UI GÖSTER
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
                else    // İNSAN
                {
                    if(owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // BİR PLAYER'A KİRA ÖDE

                        // KİRA HESAPLA

                        // SAHİBİNE KİRA ÖDE

                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                    }
                    else if (owner == null)
                    {
                        // NODE'U SATIN AL
                        
                        // UI GÖSTER
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
            break;

            case MonopolyNodeType.Fatura:
                if(!playerIsHuman) // AI
                {
                    if(owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // BİR PLAYER'A KİRA ÖDE

                        // KİRA HESAPLA
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);
                        
                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + " , " + owner.name + " OYUNCUSUNA " + rentToPay + " " + this.name + " FATURASI ÖDEDİ");
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        // NODE'U SATIN AL
                        //Debug.Log(currentPlayer.name + " SATIN ALABİLİR");
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + this.name + " SATIN ALDI");
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();
                        // UI GÖSTER
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
                else    // İNSAN
                {
                    if(owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // BİR PLAYER'A KİRA ÖDE

                        // KİRA HESAPLA

                        // SAHİBİNE KİRA ÖDE

                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                    }
                    else if (owner == null)
                    {
                        // NODE'U SATIN AL
                        
                        // UI GÖSTER
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
            break;

            case MonopolyNodeType.Demir:
                if(!playerIsHuman) // AI
                {
                    if(owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // BİR PLAYER'A KİRA ÖDE

                        // KİRA HESAPLA
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);
                        
                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + " , BURANIN SAHİBİ " + owner.name + " OYUNCUSUNA " + rentToPay + " ULAŞIM ÜCRETİ ÖDEDİ");
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        // NODE'U SATIN AL
                        //Debug.Log(currentPlayer.name + " SATIN ALABİLİR");
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + this.name + " SATIN ALDI");
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();
                        // UI GÖSTER
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
                else    // İNSAN
                {
                    if(owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // BİR PLAYER'A KİRA ÖDE

                        // KİRA HESAPLA

                        // SAHİBİNE KİRA ÖDE

                        // 
                    }
                    else if (owner == null)
                    {
                        // NODE'U SATIN AL
                        
                        // UI GÖSTER
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
            break;

            case MonopolyNodeType.Vergi:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                OnUpdateMessage.Invoke(currentPlayer.name + " " + price + " " + this.name + " <color=red>ÖDEDİ</color>");
            break;

            case MonopolyNodeType.Otopark:
                int tax = GameManager.instance.GetTaxPool();
                currentPlayer.CollectMoney(tax);
                // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                OnUpdateMessage.Invoke(currentPlayer.name + " BİRİKEN " + tax + " PARAYI <color=green>ALDI</color>");
            break;

            case MonopolyNodeType.KodeseGit:
                int indexOnBoard = Board.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
                currentPlayer.GoToJail(indexOnBoard);
                OnUpdateMessage.Invoke(currentPlayer.name + " <b><color=red>KODESE</b></color> GİRDİ");
                continueTurn = false;
            break;

            case MonopolyNodeType.Sans:

            break;

            case MonopolyNodeType.KamuFonu:

            break;
        }

        // GEREKTİĞİNGDE BURADA DUR
        if(!continueTurn)
            return;

        if (!playerIsHuman)
            Invoke("ContinueGame", GameManager.instance.SecondsBetweenTurns);
        
        else
        {
            // SHOW UI
        }
    }

    void ContinueGame()
    {
        // SON ATILAN ZARLAR ÇİFT GELDİYSE
        if (GameManager.instance.RolledADouble)
        {
            // TEKRAR AT
            GameManager.instance.RollDice();
        }
        else
        {
            // ÇİFT GELMEDİYSE
            // OYUNCU DEĞİŞTİR
            GameManager.instance.SwitchPlayer();
        }
    }

    int CalculatePropertyRent()
    {
        switch(numberOfHouses)
        {
            case 0:
                // SAHİBİNİN BU NODE'LARIN TAM SETİNE SAHİP OLUP OLMADIĞINI KONTROL EDER
                var (list,allSame) = Board.instance.PlayerHasAllNodesOfSet(this);

                if(allSame)
                {
                    currentRent = baseRent * 2;
                }
                else
                {
                    currentRent = baseRent;
                }
            break;
            case 1:
                currentRent = rentWithHouses[numberOfHouses - 1];
            break;
            case 2:
                currentRent = rentWithHouses[numberOfHouses - 1];
            break;
            case 3:
                currentRent = rentWithHouses[numberOfHouses - 1];
            break;
            case 4:
                currentRent = rentWithHouses[numberOfHouses - 1];
            break;
            case 5: // OTEL
                currentRent = rentWithHouses[numberOfHouses - 1];
            break;
        }

        return currentRent;
    }

    int CalculateUtilityRent()
    {
        int[] lastRolledDice = GameManager.instance.LastRolledDice;

        int result = 0;
        var (list,allSame) = Board.instance.PlayerHasAllNodesOfSet(this);

        if(allSame)
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 10;
        }
        else
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 4;
        }

        return result;
    }

    int CalculateRailroadRent()
    {
        int result = 0;
        var (list,allSame) = Board.instance.PlayerHasAllNodesOfSet(this);
        //Debug.Log(list.Count);

        int amount = 0;
        foreach (var item in list)
            amount += (item.owner == this.owner) ? 1 : 0;

        // baseRent = 25/(2^0) = 50/(2^1) =  100/(2^2) = 200/(2^3)
        result = baseRent * (int)Mathf.Pow(2, amount-1);
        
        return result;
    }
}