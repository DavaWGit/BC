/**
 * Autor: David Zahálka
 *
 * Správa uživatelského rozhraní
 * 
 * Skript řídí všechny aspekty uživatelského rozhraní (UI) ve hře, včetně zobrazení zdraví,
 * zkušeností, mincí a skóre. Umožňuje také správu herního menu a poskytuje funkce pro
 * ovládání herních statistik a upgrade systému.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public bool isGameOver = false, isMenuOpen = false;
    public Slider expSlider, bossHealthSlider, playerHealthSlider;
    public TMP_Text expText, bossHealthText, playerHealthText;
    public TMP_Text coinText, timeText, timeSurvivedText, totalPointsText, currentPointsText, finalPointsText;
    public TMP_Text strengthText, agilityText, intelligenceText, vitalityText, speedText, luckText, pointsText, upgradeText;
    public GameObject characterPanel, strengthButton, agilityButton, 
        intelligenceButton, vitalityButton, speedButton, luckButton, endScreenPanel, enterDungeon, cantEnterDungeon;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(isGameOver)
        {
            Time.timeScale = 0f;
        }
        if(Input.GetKeyDown(KeyCode.C) && !isGameOver)
        {
            characterPanel.gameObject.SetActive(!characterPanel.gameObject.activeSelf);
            if(characterPanel.gameObject.activeSelf)
            {
                Time.timeScale = 0f;
                isMenuOpen = true;
            }
            else
            {
                Time.timeScale = 1f;
                isMenuOpen = false;
            }
            playerHealthSlider.maxValue = PlayerMovement.instance.maxHp;
            playerHealthSlider.value = PlayerMovement.instance.hp;
            playerHealthText.text = PlayerMovement.instance.hp + "/" + PlayerMovement.instance.maxHp;
        }
        if(GameManager.instance.upgradePoints > 0)
        {
            strengthButton.gameObject.SetActive(true);
            agilityButton.gameObject.SetActive(true);
            intelligenceButton.gameObject.SetActive(true);
            vitalityButton.gameObject.SetActive(true);
            speedButton.gameObject.SetActive(true);
            luckButton.gameObject.SetActive(true);
        }
        else
        {
            strengthButton.gameObject.SetActive(false);
            agilityButton.gameObject.SetActive(false);
            intelligenceButton.gameObject.SetActive(false);
            vitalityButton.gameObject.SetActive(false);
            speedButton.gameObject.SetActive(false);
            luckButton.gameObject.SetActive(false);
        }
        pointsText.text = "Points available: " + GameManager.instance.upgradePoints;
        if(GameManager.instance.upgradePoints > 0)
        {
            upgradeText.gameObject.SetActive(true);
        }
        else
        {
            upgradeText.gameObject.SetActive(false);
        }
    }

    public void ToggleUI(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
    public void UpdateScore()
    {
        currentPointsText.text = "Score: " + GameManager.instance.points;
    }

    public void UpdateExp(int currentExp, int maxExp, int currentLvl)
    {
        expSlider.maxValue = maxExp;
        expSlider.value = currentExp;
        expText.text = "Level: " + currentLvl;
    }

    public void ResetExpUI()
    {
        expSlider.maxValue = 0;
        expSlider.value = 0;
        expText.text = "Level: 1";
        currentPointsText.text = "Score: " + GameManager.instance.points;
    }

    public void UpdateCoins()
    {
        coinText.text = PlayerPrefs.GetInt("TotalCoinCount", 0).ToString();
    }

    public void UpdateStats()
    {
        strengthText.text = "Strength: " + PlayerMovement.instance.strength;
        agilityText.text = "Agility: " + PlayerMovement.instance.agility;
        intelligenceText.text = "Intelligence: " + PlayerMovement.instance.intelligence;
        vitalityText.text = "Vitality: " + PlayerMovement.instance.vitality;
        speedText.text = "Speed: " + PlayerMovement.instance.speed;
        luckText.text = "Luck: " + PlayerMovement.instance.luck;
    }

    public void upgradeStrength()
    {
        PlayerMovement.instance.upgradeStrength();
    }

    public void upgradeAgility()
    {
        PlayerMovement.instance.upgradeAgility();
    }

    public void upgradeIntelligence()
    {
        PlayerMovement.instance.upgradeIntelligence();
    }

    public void upgradeVitality()
    {
        PlayerMovement.instance.upgradeVitality();
    }

    public void upgradeSpeed()
    {
        PlayerMovement.instance.upgradeSpeed();
    }

    public void upgradeLuck()
    {
        PlayerMovement.instance.upgradeLuck();
    }

    public void updateTimer(float timer)
    {
        float min = Mathf.FloorToInt(timer / 60.0f);
        float sec = Mathf.FloorToInt(timer % 60);

        timeText.text = min + ":" + sec.ToString("00");
    }

    public void backToMainMenu(int sceneID)
    {
        isGameOver = false;
        Time.timeScale = 1f;
        TilemapManager.instance.DeleteSavedMap();
        endScreenPanel.SetActive(false);
        SceneManager.LoadSceneAsync(sceneID);
    }
}
