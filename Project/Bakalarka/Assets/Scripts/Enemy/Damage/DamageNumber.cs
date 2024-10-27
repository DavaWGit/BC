/**
 * Autor: David Zahálka
 *
 * Zobrazení poškození
 * 
 * Tento skript řídí vizualizaci poškození zobrazeného jako textový objekt ve hře. 
 * Skript zajišťuje animaci a životní cyklus čísla poškození.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public TMP_Text damageNumber;
    public float lifeTime;
    private float lifeCounter;

    public float speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        lifeCounter = lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(lifeCounter > 0)
        {
            lifeCounter -= Time.deltaTime;
            if(lifeCounter <= 0)
            {
                //Destroy(gameObject);
                DamageNumberController.instance.PutInPool(this);
            }
        }

        transform.position += Vector3.up * speed * Time.deltaTime;
    }

    public void SetColor(Color color)
    {
        damageNumber.color = color;
    }

    public void DisplayDamage(int damage)
    {
        lifeCounter = lifeTime;

        damageNumber.text = damage.ToString();
    }
}
