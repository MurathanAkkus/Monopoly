using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Unity.VisualScripting;

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
    
    [Header("Property Mortgage")]
    [SerializeField] GameObject mortgageImage;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;

    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;

    void OnValidate()
    {
        // Tüm çocuklarda Canvas'ı ara
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            // Canvas'ın altındaki TÜM TMP_Text'leri topla
            TMP_Text[] allTexts = canvas.GetComponentsInChildren<TMP_Text>(true);
            
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
    }

    // İPOTEK İÇERİĞİ
    public int MortgageProperty()
    {
        isMortgaged = true;
        mortgageImage.SetActive(true);
        propertyImage.SetActive(false);
        return mortgageValue;
    }

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        mortgageImage.SetActive(false);
        propertyImage.SetActive(true);
    }

    public bool IsMortgaged => isMortgaged;
    public int MortgageValue => mortgageValue;

    // SAHİPLİK GÜNCELLE
    public void OnOwnerUpdated()
    {
        if (ownerBar != null)
        {
            if(ownerText.text != "")
            {
                ownerBar.SetActive(true);
                //ownerText.text = owner.name;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "";
            }
        }
    }
}
