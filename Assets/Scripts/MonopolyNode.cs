using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public enum MonopolyNodeType
{
    Property,
    Utility,
    Railroad,
    Tax,
    Chance,
    CommunityChest,
    Go,
    Jail,
    FreeParking,
    GoToJail
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
    public int houseCost;
    [SerializeField] TMP_Text priceText;

    [Header("Rent")]
    [SerializeField] bool calculateRentAuto; // Kira hesaplasın mı?
    [SerializeField] int currentRent;
    [SerializeField] internal int baseRent;
    [SerializeField] internal List<int> rentWithHouses = new List<int>();
    int numberOfHouses;
    public int NumberOfHouses => numberOfHouses;
    [SerializeField] GameObject[] houses;
    [SerializeField] GameObject hotel;

    [Header("Mortgage")]
    [SerializeField] TMP_Text mortgagedText;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;

    [Header("Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    Player owner;

    [Header("Effect")]
    [SerializeField] GameObject highlightSquare;

    [SerializeField] SpriteRenderer highlightRenderer;
    Coroutine blinkCoroutine;

    // MESAJLAŞMA SİSTEMİ
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    // BİR KAMU FONU KARTI ÇEK
    public delegate void DrawCommunityCard(Player player);
    public static DrawCommunityCard OnDrawCommunityCard;

    // BİR ŞANS KARTI ÇEK
    public delegate void DrawChanceCard(Player player);
    public static DrawChanceCard OnDrawChanceCard;

    // İNSANLAR İÇİN PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool enablePayToFree, bool hasChanceJailCard, bool hasCommunityJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    // ARSA İÇİN SATIN ALMA PANELİ
    public delegate void ShowBuyPropertyPanel(MonopolyNode node, Player player, bool allowBuy);
    public static ShowBuyPropertyPanel OnShowPropertyBuyPanel;

    // TCDD VEYA DENİZ YOLLARI İÇİN SATIN ALMA PANELİ
    public delegate void ShowBuyRailroadPanel(MonopolyNode node, Player player, bool allowBuy);
    public static ShowBuyRailroadPanel OnShowRailroadBuyPanel;

    // KAMU KURULUŞLARI(FATURA) İÇİN SATIN ALMA PANELİ
    public delegate void ShowBuyUtilityPanel(MonopolyNode node, Player player, bool allowBuy);
    public static ShowBuyUtilityPanel OnShowUtilityBuyPanel;

    public Player Owner => owner;
    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
        OnOwnerUpdated();
    }

    void OnValidate()
    {
        if (gameObject.name == "Go Node" || gameObject.name == "In Jail Node" || gameObject.name == "Free Parking Node" || gameObject.name == "Go To Jail Node")
            return;
        // İsimi güncelle
        if (nameText != null)
            nameText.text = name;

        else
            Debug.LogWarning($"{gameObject.name} icinde 'Name Text' adli bir TMP_Text bulunamadi!");

        // KİRA HESAPLAMA
        if (calculateRentAuto)
        {
            if (monopolyNodeType == MonopolyNodeType.Property)
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
                else if (baseRent <= 0)
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouses.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Utility)
                mortgageValue = price / 2;

            if (monopolyNodeType == MonopolyNodeType.Railroad)
                mortgageValue = price / 2;
        }
        if (priceText != null)
            priceText.text = price + " M";
        // SAHİPLİK GÜNCELLE
        OnOwnerUpdated();
        UnMortgageProperty();
        //isMortgaged = false;
    }

    /*public void UpdateColorField(Color color)
    {
        if(propertyColorField != null)
            propertyColorField.color = color;
    }*/

    // İPOTEK İÇERİĞİ
    public int MortgageProperty()
    {
        isMortgaged = true;
        if (mortgagedText != null)
            mortgagedText.gameObject.SetActive(true);

        if (propertyImage != null)
            propertyImage.SetActive(false);

        return mortgageValue;
    }

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if (mortgagedText != null)
            mortgagedText.gameObject.SetActive(false);

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
            if (owner != null)
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
        switch (monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                if (!playerIsHuman) // AI
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // BİR PLAYER'A KİRA ÖDE

                        // KİRA HESAPLA
                        //Debug.Log("OYUNCU KİRA ÖDEYEBİLİR VE BURANIN SAHİBİ: "+ owner.name);
                        int rentToPay = CalculatePropertyRent();
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);

                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + $" , {name} kartının sahibine " + rentToPay + "M kira ödedi.");
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();
                        // UI GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + name + " kartını satın aldı.");
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
                else    // İNSAN
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // KİRA HESAPLA
                        //Debug.Log("OYUNCU KİRA ÖDEYEBİLİR VE BURANIN SAHİBİ: "+ owner.name);
                        int rentToPay = CalculatePropertyRent();
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);

                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + $" , {name} kartının sahibine " + rentToPay + "M kira ödedi.");
                    }
                    else if (owner == null)
                    {
                        // NODE İÇİN SATIN ALMA PANELİNİ GÖSTER
                        OnShowPropertyBuyPanel.Invoke(this, currentPlayer, true);
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + name + " kartını satın aldı.");
                    }
                    else
                    {
                        // SATIN ALACAK PARA YOK VE SAHİPSİZ KALACAK
                    }
                }
                break;

            case MonopolyNodeType.Utility:
                if (!playerIsHuman) // AI
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // KİRA HESAPLA
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);

                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + $" , {name} kartının sahibine " + rentToPay + "M fatura ödedi.");
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();
                        // UI GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + name + " kartını satın aldı.");
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
                else    // İNSAN
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // KİRA HESAPLA
                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);

                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + $" , {name} kartının sahibine " + rentToPay + "M fatura ödedi.");
                    }
                    else if (owner == null)
                    {
                        // NODE SATIN ALMAK İÇİN PANELİ GÖSTER
                        OnShowRailroadBuyPanel.Invoke(this, currentPlayer, true);
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + name + " kartını satın aldı.");
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
                break;

            case MonopolyNodeType.Railroad:
                if (!playerIsHuman) // AI
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // KİRA HESAPLA
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);

                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + $" , {name} kartının sahibine " + rentToPay + "M ulaşım ücreti ödedi.");
                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        currentPlayer.BuyProperty(this);
                        // UI GÖSTER
                        OnOwnerUpdated();
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + name + " kartını satın aldı.");
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
                else    // İNSAN
                {
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // KİRA HESAPLA
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        // SAHİBİNE KİRA ÖDE
                        currentPlayer.PayRent(rentToPay, owner);
                        // MESAJ GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + $" , {name} kartının sahibine " + rentToPay + "M ulaşım ücreti ödedi.");
                    }
                    else if (owner == null)
                    {
                        // NODE'U SATIN AL
                        OnShowRailroadBuyPanel.Invoke(this, currentPlayer, true);
                        // UI GÖSTER
                        OnUpdateMessage.Invoke(currentPlayer.name + " " + name + " kartını satın aldı.");
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
                break;

            case MonopolyNodeType.Tax:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                OnUpdateMessage.Invoke($"{currentPlayer.name} <color=red>{price}</color> {name} ödedi.");
                break;

            case MonopolyNodeType.FreeParking:
                int tax = GameManager.instance.GetTaxPool();
                currentPlayer.CollectMoney(tax);
                // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                if (tax == 0)
                {
                    OnUpdateMessage.Invoke("Biriken hiçbir para bulunmamaktadır.");
                    break;
                }

                OnUpdateMessage.Invoke($"{currentPlayer.name} biriken {tax} parayı aldı!");
                break;

            case MonopolyNodeType.GoToJail:
                int indexOnBoard = Board.instance.route.IndexOf(currentPlayer.MyMonopolyNode);
                currentPlayer.GoToJail(indexOnBoard);
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=red>Kodes</b>e girdi");
                continueTurn = false;
                break;

            case MonopolyNodeType.Chance:
                OnDrawChanceCard.Invoke(currentPlayer);
                continueTurn = false;
                break;

            case MonopolyNodeType.CommunityChest:
                OnDrawCommunityCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
        }

        // GEREKTİĞİNGDE BURADA DUR
        if (!continueTurn)
            return;

        if (!playerIsHuman)
            currentPlayer.ChangeState(Player.AiStates.TRADING);

        else
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;
            bool canRollDice = GameManager.instance.RolledADouble && currentPlayer.ReadMoney >= 0;

            bool jail1 = currentPlayer.HasChanceFreeCard;
            bool jail2 = currentPlayer.HasCommunityFreeCard;
            bool showPayToGetOut = currentPlayer.IsInJail && !GameManager.instance.HasRolledDice;

            // UI GÖSTER
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, showPayToGetOut, jail1, jail2);
        }
    }

    int CalculatePropertyRent()
    {
        switch (numberOfHouses)
        {
            case 0:
                // SAHİBİNİN BU NODE'LARIN TAM SETİNE SAHİP OLUP OLMADIĞINI KONTROL EDER
                var (list, allSame) = Board.instance.PlayerHasAllNodesOfSet(this);

                if (allSame)
                    currentRent = baseRent * 2;
                else
                    currentRent = baseRent;

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
        List<int> lastRolledDice = GameManager.instance.LastRolledDice;

        int result = 0;
        var (list, allSame) = Board.instance.PlayerHasAllNodesOfSet(this);

        if (allSame)
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
        var (list, allSame) = Board.instance.PlayerHasAllNodesOfSet(this);
        //Debug.Log(list.Count);

        int amount = 0;
        foreach (var item in list)
            amount += (item.owner == this.owner) ? 1 : 0;

        // baseRent = 25/(2^0) = 50/(2^1) =  100/(2^2) = 200/(2^3)
        result = baseRent * (int)Mathf.Pow(2, amount - 1);

        return result;
    }

    void VisualizeHouses()
    {
        for (int i = 0; i < houses.Length; i++)
            houses[i].SetActive(numberOfHouses > i && numberOfHouses < 5);

        hotel.SetActive(numberOfHouses == 5);

        /*switch (numberOfHouses)
        {
            case 0:
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
            break;
            case 1:
                houses[0].SetActive(true);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
            break;
            case 2:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(false);
            break;
            case 3:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(false);
                hotel.SetActive(false);
            break;
            case 4:
                houses[0].SetActive(true);
                houses[1].SetActive(true);
                houses[2].SetActive(true);
                houses[3].SetActive(true);
                hotel.SetActive(false);
            break;
            case 5:
                houses[0].SetActive(false);
                houses[1].SetActive(false);
                houses[2].SetActive(false);
                houses[3].SetActive(false);
                hotel.SetActive(true);
            break;
        }*/
    }

    public void BuildHouseOrHotel()
    {
        if (monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses++;
            VisualizeHouses();
        }
    }

    public int SellHouseOrHotel()
    {

        if (monopolyNodeType == MonopolyNodeType.Property && numberOfHouses > 0)
        {
            numberOfHouses--;
            VisualizeHouses();
            return houseCost / 2;
        }
        return 0;
    }

    public void ResetNode()
    {
        // İPOTEKLİ İSE
        if (isMortgaged)
        {
            propertyImage.SetActive(true);
            mortgagedText.gameObject.SetActive(false);
            isMortgaged = false;
        }

        // EVLERİ VE OTELİ RESETLE
        if (monopolyNodeType == MonopolyNodeType.Property)
        {
            numberOfHouses = 0;
            VisualizeHouses();
        }

        // SAHİBİNİ RESETLE - SAHİBİNİ SİL
        owner.RemoveProperty(this);
        owner.name = "";
        owner.ActivateSelector(false);
        owner = null;

        // UI GÜNCELLE
        OnOwnerUpdated();
    }

    // ---------------------------- NODEun SAHİBİNİ DEĞİŞTİRME ---------------------------- 
    public void ChangeOwner(Player newOwner)
    {
        owner.RemoveProperty(this);
        newOwner.AddNode(this);
        SetOwner(newOwner);
    }

    // ---------------------------- NODEu VURGULAMAK İÇİN EFEKT ---------------------------
    IEnumerator BlinkEffect()
    {
        float speed = 2f;
        float alpha = 0f;
        bool increasing = true;

        while (true)
        {
            Color color = highlightRenderer.color;

            alpha += (increasing ? 1 : -1) * Time.deltaTime * speed;

            if (alpha >= 1f)
            {
                alpha = 1f;
                increasing = false;
            }
            else if (alpha <= 0.2f)
            {
                alpha = 0.2f;
                increasing = true;
            }

            color.a = alpha;
            highlightRenderer.color = color;

            yield return null;
        }
    }

    public void ShowHighlightEffect()
    {
        if (highlightSquare != null)
            highlightSquare.SetActive(true);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkEffect());
    }

    public void HideHighlightEffect()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (highlightSquare != null)
            highlightSquare.SetActive(false);
    }
}