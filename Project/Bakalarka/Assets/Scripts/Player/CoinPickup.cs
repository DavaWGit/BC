/**
 * Autor: David Zahálka
 *
 * Skript pro sbírání mincí
 * 
 * Tento skript řídí interakci hráče s herními mincemi. Při kontaktu s hráčem přidá mince
 * do celkového skóre a zničí objekt mince v herním světě.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int amount = 1;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag == "Player")
        {
            CoinController.instance.AddCoins(amount);

            Destroy(gameObject);
        }
    }
}
