/**
 * Autor: David Zahálka
 *
 * Skript pro správu zkušeností
 * 
 * Skript řídí zkušenostní systém ve hře, včetně získávání zkušeností, levelování a správy
 * upgrade bodů. Zajišťuje také aplikaci zkušenostních bonusů založených na statistikách
 * hráče a aktualizuje UI pro zobrazení zkušeností a levelů.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceController : MonoBehaviour
{
    public static ExperienceController instance;

    private void Awake()
    {
        instance = this;
    }

    public int currentExperience, upgradePoints;
    public List<int> levels;
    public int currentLevel = 1, levelCount = 100;
    // Start is called before the first frame update
    void Start()
    {
        levels = new List<int> { 1 };
        while(levels.Count < levelCount)
        {
            levels.Add(Mathf.CeilToInt((levels[levels.Count - 1] + 20)* 1.06f));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetExp(int amount)
    {
        float expMultiplier = 1 + (0.02f * PlayerMovement.instance.intelligence); // 2% per intelligence point
        GameManager.instance.currentExperience += Mathf.FloorToInt(amount * expMultiplier);
        if(GameManager.instance.currentExperience >= levels[GameManager.instance.currentLevel])
        {
            LevelUp();
        }

        UIController.instance.UpdateExp(GameManager.instance.currentExperience, levels[GameManager.instance.currentLevel], GameManager.instance.currentLevel);
    }

    void LevelUp()
    {
        GameManager.instance.currentExperience -= levels[GameManager.instance.currentLevel];
        GameManager.instance.currentLevel++;
        GameManager.instance.upgradePoints += 3;
        PlayerMovement.instance.hp = PlayerMovement.instance.maxHp;
        PlayerMovement.instance.transform.Find("HealthBar/Bar").localScale 
        = new Vector3((float)PlayerMovement.instance.hp/PlayerMovement.instance.maxHp, 0.1f);
    }

}
