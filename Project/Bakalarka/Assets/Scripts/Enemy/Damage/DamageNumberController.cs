/**
 * Autor: David Zahálka
 *
 * Kontroler čísel poškození
 * 
 * Tento kontroler spravuje instancování a recyklaci objektů čísel poškození ve hře. Umožňuje
 * optimalizované použití prostředků tím, že recykluje objekty poškození místo jejich opakovaného 
 * vytváření a ničení. Spravuje pool těchto objektů a poskytuje metody pro jejich získání,
 * aktivaci a deaktivaci.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumberController : MonoBehaviour
{
    public static DamageNumberController instance;
    private List<DamageNumber> numberPool = new List<DamageNumber>();

    private void Awake()
    {
        instance = this;
    }

    public DamageNumber numberToSpawn;
    public Transform numberCanvas;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnDamage(int damage, Vector3 spawnPoint, Color color)
    {
        //DamageNumber newDamage = Instantiate(numberToSpawn, spawnPoint, Quaternion.identity, numberCanvas);

        DamageNumber newDamage = GetNumber();

        newDamage.SetColor(color);
        newDamage.DisplayDamage(damage);
        newDamage.gameObject.SetActive(true);

        newDamage.transform.position = spawnPoint;
    }

    public DamageNumber GetNumber()
    {
        DamageNumber number = null;
        if(numberPool.Count == 0)
        {
            number = Instantiate(numberToSpawn, numberCanvas);
        }
        else
        {
            number = numberPool[0];
            numberPool.RemoveAt(0);
        }

        return number;
    }

    public void PutInPool(DamageNumber numberToPool)
    {
        numberToPool.gameObject.SetActive(false);

        numberPool.Add(numberToPool);
    }
}
