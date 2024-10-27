/**
 * Autor: David Zahálka
 *
 * Skript pro správu mincí
 * 
 * Tento skript zajišťuje správu herních mincí. Umožňuje přidávání mincí do celkového počtu,
 * ukládání a načítání mincí a generování mincí v herním světě.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    public static CoinController instance;
    private const string CoinCountKey = "TotalCoinCount";
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        LoadCoins();
    }

    public int coinCnt;
    public CoinPickup coin;
    public void AddCoins(int amount)
    {
        LoadCoins();
        coinCnt += amount;
        SaveCoin();
        UIController.instance.UpdateCoins();
    }
    
    private void LoadCoins()
    {
        coinCnt = PlayerPrefs.GetInt(CoinCountKey, 0);
    }

    private void SaveCoin()
    {
        PlayerPrefs.SetInt(CoinCountKey, coinCnt);
        PlayerPrefs.Save();
    }
    public void DropCoin(Vector3 position, int amount)
    {
        CoinPickup newCoin = Instantiate(coin, position, Quaternion.identity);
        newCoin.amount = amount;
        newCoin.gameObject.SetActive(true);
    }
}
