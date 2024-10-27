/**
 * Autor: David Zahálka
 *
 * Tlačítko pro nákup
 * 
 * Skript definuje funkci tlačítka pro nákup ve hře.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyButton : MonoBehaviour
{
    public Shop shop;
    public string itemName;
    public int itemCost;

    public void OnClick()
    {
        shop.BuyItem(itemName, itemCost);
    }
}
