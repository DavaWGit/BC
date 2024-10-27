/**
 * Autor: David Zahálka
 *
 * Skript obchodu
 * 
 * Skript řídí logiku obchodu ve hře, včetně nákupu, prodeje a správy inventáře. Umožňuje hráčům
 * kupovat a vybavovat předměty, zaznamenává koupené předměty a spravuje zobrazení dostupných
 * a koupených předmětů. Dále spravuje měnu hráče a umožňuje nákup speciálních schopností.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shop : MonoBehaviour
{
    // Player Prefs
    private const string CoinCountKey = "TotalCoinCount";
    private const string BoughtItemsKey = "BoughtItems";
    private const string EquippedItemKey = "EquippedItem";
    private const string CoinMagnetPurchased = "CoinMagnet";
    private const string HealingOT = "HealingOT";
    private const string DungeonFinder = "DungeonFinder";
    public Canvas canvas;
    int coinCnt;
    private string[] boughtItems;
    private string equippedItem;
    //public TMP_Text equippedText;
    public GameObject priceTag, buyButton, equipButton, coinMagnetBuy, coinMagnetBought, coinMagnetPrice, healingOTBuy, healingOTBought, healingOTPrice, 
                    dungeonFinderBuy, dungeonFinderBought, dungeonFinderPrice;
    public GameObject player;
    private Dictionary<string, GameObject> weaponObjects = new Dictionary<string, GameObject>();
    void Start()
    {
        //RemoveAllBoughtItems();
        // nacteni vsech koupených a nasazených předmětů, změna jejich UI v obchodu
        LoadBoughtItems();
        LoadEquippedItem();
        foreach (string itemName in boughtItems)
        {
            Transform shopGameObject = canvas.transform.Find("Shop");
            Transform itemGameObject = shopGameObject.Find(itemName);
            Debug.Log("found items: " + itemGameObject);
            if (itemGameObject != null)
            {
                Transform priceTag = itemGameObject.transform.Find("Price");
                Transform buyButton = itemGameObject.transform.Find("Buy");
                Transform equipButton = itemGameObject.transform.Find("Equip");

                if (priceTag != null)
                    priceTag.gameObject.SetActive(false);
                if (buyButton != null)
                    buyButton.gameObject.SetActive(false);
                if (equipButton != null)
                    equipButton.gameObject.SetActive(true);
            }
        }

        if (!string.IsNullOrEmpty(equippedItem))
        {
            Transform shopGameObject = canvas.transform.Find("Shop");
            Transform itemGameObject = shopGameObject.Find(equippedItem);
            if (itemGameObject != null)
            {
                Transform equipButton = itemGameObject.transform.Find("Equip");
                if (equipButton != null)
                    equipButton.gameObject.SetActive(false);

                Transform equippedText = itemGameObject.transform.Find("Equipped");
                equippedText.gameObject.SetActive(true);
            }
        }

        for (int i = 0; i <= 6; i++)
        {
            string weaponName = "Weapon" + i;
            GameObject weaponObject = player.transform.Find(weaponName).gameObject;
            weaponObjects[weaponName] = weaponObject;
        }

        if (!string.IsNullOrEmpty(equippedItem))
        {
            EquipItem(equippedItem);
        }
    }

    void Update()
    {
        // změna UI vylepšení v případě nejich zakoupení
        if(PlayerPrefs.GetInt("CoinMagnet", 0) == 1)
        {
            coinMagnetBought.SetActive(true);
            coinMagnetBuy.SetActive(false);
            coinMagnetPrice.SetActive(false);
        }
        if(PlayerPrefs.GetInt("HealingOT", 0) == 1)
        {
            healingOTBought.SetActive(true);
            healingOTBuy.SetActive(false);
            healingOTPrice.SetActive(false);
        }
        if(PlayerPrefs.GetInt("DungeonFinder", 0) == 1)
        {
            dungeonFinderBought.SetActive(true);
            dungeonFinderBuy.SetActive(false);
            dungeonFinderPrice.SetActive(false);
        }
    }

    public void BuyItem(string itemName, int cost)
    {
        // odečtení ceny předmětu od nasbíraných mincí
        coinCnt = PlayerPrefs.GetInt(CoinCountKey, 0);
        if(coinCnt >= cost && !IsItemBought(itemName))
        {
            coinCnt -= cost;
            PlayerPrefs.SetInt(CoinCountKey, coinCnt);
            PlayerPrefs.Save();

            // přidání předmětu do zakoupených předmětů
            AddBoughtItem(itemName);

            // změna UI
            GameObject itemGameObject = GameObject.Find(itemName);
            if (itemGameObject != null)
            {
                Transform buyButton = itemGameObject.transform.Find("Buy");
                if (buyButton != null)
                    buyButton.gameObject.SetActive(false);
                Transform priceTag = itemGameObject.transform.Find("Price");
                if (priceTag != null)
                    priceTag.gameObject.SetActive(false);
                Transform equipButton = itemGameObject.transform.Find("Equip");
                equipButton.gameObject.SetActive(true);
            }
        }
    }

    // načtení koupených předmětů
    private void LoadBoughtItems()
    {
        string boughtItemsString = PlayerPrefs.GetString(BoughtItemsKey, "");
        if (!string.IsNullOrEmpty(boughtItemsString))
        {
            boughtItems = boughtItemsString.Split(',');
        }
        else
        {
            boughtItems = new string[0];
        }
    }

    // přidání předmětu mezi zakoupené předměty
    private void AddBoughtItem(string itemName)
    {
        string boughtItemsString = PlayerPrefs.GetString(BoughtItemsKey, "");
        boughtItemsString += (string.IsNullOrEmpty(boughtItemsString) ? "" : ",") + itemName;
        PlayerPrefs.SetString(BoughtItemsKey, boughtItemsString);
        PlayerPrefs.Save();

        LoadBoughtItems();
    }

    private bool IsItemBought(string itemName)
    {
        foreach (string item in boughtItems)
        {
            if (item == itemName)
            {
                return true;
            }
        }
        return false;
    }

    // nasazení předmětu
    public void EquipItem(string itemName)
    {
        UnequipItem();

        equippedItem = itemName;
        SaveEquippedItem();

        // změna UI
        GameObject itemGameObject = GameObject.Find(itemName);
        if (itemGameObject != null)
        {
            Transform equipButton = itemGameObject.transform.Find("Equip");
            if (equipButton != null)
                equipButton.gameObject.SetActive(false);
            
            Transform equippedText = itemGameObject.transform.Find("Equipped");
            equippedText.gameObject.SetActive(true);
        }

        if (weaponObjects.ContainsKey(itemName))
        {
            weaponObjects[itemName].SetActive(true);
        }
    }

    // sundání nasazených předmětů
    private void UnequipItem()
    {
        if (!string.IsNullOrEmpty(equippedItem))
        {
            GameObject equippedItemGameObject = GameObject.Find(equippedItem);
            if (equippedItemGameObject != null)
            {
                Transform equippedItemEquipButton = equippedItemGameObject.transform.Find("Equip");
                if (equippedItemEquipButton != null)
                {
                    equippedItemEquipButton.gameObject.SetActive(true);
                    if (weaponObjects.ContainsKey(equippedItem))
                    {
                        weaponObjects[equippedItem].SetActive(false);
                    }
                    equippedItem = "";
                    SaveEquippedItem();

                    Transform equippedText = equippedItemGameObject.transform.Find("Equipped");
                    equippedText.gameObject.SetActive(false);
                }
            }
        }

        
    }

    private void LoadEquippedItem()
    {
        equippedItem = PlayerPrefs.GetString(EquippedItemKey, "");
    }

    private void SaveEquippedItem()
    {
        PlayerPrefs.SetString(EquippedItemKey, equippedItem);
        PlayerPrefs.Save();
    }

    private void RemoveAllBoughtItems()
    {
        PlayerPrefs.SetString(BoughtItemsKey, "");
        PlayerPrefs.Save();

        boughtItems = new string[0];
    }

    // zakoupení pasivních vylepšení
    public void BuyCoinMagnet()
    {
        int cost = 100;
        int currentCoins = PlayerPrefs.GetInt(CoinCountKey, 0);
        if(currentCoins >= cost)
        {
            PlayerPrefs.SetInt(CoinCountKey, currentCoins-cost);
            PlayerPrefs.SetInt(CoinMagnetPurchased, 1);
            PlayerPrefs.Save();
            MenuScript.instance.coinText.text = PlayerPrefs.GetInt(CoinCountKey, 0).ToString();
        }
    }

    public void BuyHealingOT()
    {
        int cost = 150;
        int currentCoins = PlayerPrefs.GetInt(CoinCountKey, 0);
        if(currentCoins >= cost)
        {
            PlayerPrefs.SetInt(CoinCountKey, currentCoins-cost);
            PlayerPrefs.SetInt(HealingOT, 1);
            PlayerPrefs.Save();
            MenuScript.instance.coinText.text = PlayerPrefs.GetInt(CoinCountKey, 0).ToString();
        }
    }

    public void BuyDungeonFinder()
    {
        int cost = 150;
        int currentCoins = PlayerPrefs.GetInt(CoinCountKey, 0);
        if(currentCoins >= cost)
        {
            PlayerPrefs.SetInt(CoinCountKey, currentCoins-cost);
            PlayerPrefs.SetInt(DungeonFinder, 1);
            PlayerPrefs.Save();
            MenuScript.instance.coinText.text = PlayerPrefs.GetInt(CoinCountKey, 0).ToString();
        }
    }
}
