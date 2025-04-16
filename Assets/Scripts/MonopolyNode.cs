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
    [SerializeField] Image propertyColorField;
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
    [SerializeField] internal int[] rentWithHouses;

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

    public Player Owner => owner;
    
    void OnValidate()
    {
        // Tüm çocuklarda Canvas'ı ara
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            // Canvas'ın altındaki TÜM TMP_Text ve GameObject'leri topla
            TMP_Text[] allTexts = canvas.GetComponentsInChildren<TMP_Text>(true);
            GameObject[] allGameObjects = canvas.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).ToArray();
            
            // "Name Text" adını taşıyanı bul
            foreach (TMP_Text text in allTexts)
            {
                if (text.gameObject.name == "Name Text") 
                {
                    nameText = text;
                    break;
                }
            }

            // İsimi güncelle
            if (nameText != null)
                nameText.text = name;
            
            else
                Debug.LogError($"{gameObject.name} icinde 'Name Text' adli bir TMP_Text bulunamadi!");

            // "Price Text" adını taşıyanı bul
            foreach (TMP_Text text in allTexts)
            {
                if (text.gameObject.name == "Price Text") 
                {
                    priceText = text;
                    break;
                }
            }

            // "Owner Text" adını taşıyanı bul
            foreach (TMP_Text text in allTexts)
            {
                if (text.gameObject.name == "Owner Text") 
                {
                    ownerText = text;
                    break;
                }
            }

            // "Owner Bar" adını taşıyan GameObject'i bul
            foreach (GameObject obj in allGameObjects)
            {
                if (obj.name == "Owner Bar")
                {
                    ownerBar = obj;
                    break;
                }
            }

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
                        rentWithHouses = new int []
                        {
                            baseRent * 5,
                            baseRent * 5 * 3,
                            baseRent * 5 * 9,
                            baseRent * 5 * 16,
                            baseRent * 5 * 25
                        };
                    }
                }
                if (monopolyNodeType == MonopolyNodeType.Fatura)
                    mortgageValue = price / 2;
                
                if (monopolyNodeType == MonopolyNodeType.Demir)
                    mortgageValue = price / 2;
            }  
        }
        if (priceText != null)
                priceText.text = price + " TL";
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
            Debug.LogWarning($"[OnOwnerUpdated] {gameObject.name} için owner null!");
            return;
        }
            
        if (ownerBar != null)
        {
            if(owner.name != "")
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

        // NODE TİPİNE GÖRE KONTROL ET 
        switch(monopolyNodeType)
        {
            case MonopolyNodeType.Mulk:
                if(!playerIsHuman) // AI
                {
                    if(owner.name != "" && owner != currentPlayer && !isMortgaged)
                    {
                        // BİR PLAYER'A KİRA ÖDE

                        // KİRA HESAPLA
                        int rentToPay = CalculatePropertyRent();

                        // SAHİBİNE KİRA ÖDE

                        // OLAYLA İLGİLİ BİR MESAJ GÖSTER
                    }
                    else if (owner.name == "")
                    {
                        // NODE'A AİT BİR SATIN ALMA PENCERESİ GÖSTER
                    }
                    else
                    {
                        // SAHİPSİZ VE SATIN ALACAK PARA YOK
                    }
                }
            break;

            case MonopolyNodeType.Fatura:

            break;

            case MonopolyNodeType.Demir:

            break;

            case MonopolyNodeType.Vergi:

            break;

            case MonopolyNodeType.KodeseGit:

            break;

            case MonopolyNodeType.Sans:

            break;

            case MonopolyNodeType.KamuFonu:

            break;

            
        }

        if (!playerIsHuman)
        {
            Invoke("ContinueGame", 2f);
        }
        else
        {
            // SHOW UI
        }
    }

    void ContinueGame()
    {
        // SON ATILAN ZARLAR ÇİFT GELDİYSE
        // TEKRAR AT

        // ÇİFT GELMEDİYSE
        // OYUNCU DEĞİŞTİR

        GameManager.instance.SwitchPlayer();
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
}