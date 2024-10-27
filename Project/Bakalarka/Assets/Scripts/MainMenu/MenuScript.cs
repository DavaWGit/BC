/**
 * Autor: David Zahálka
 *
 * Hlavní Menu Skript
 * 
 * Tento skript řídí chování hlavního menu hry, včetně zobrazení a skrývání různých UI prvků, načítání scén. 
 * Také obsahuje metody pro spouštění a ukončení hry.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MenuScript : MonoBehaviour
{
    public GameObject LoadingScreen, MainMenuScreen, ShopScreen;
    public GameObject enemyForestGoblin, enemySandGoblin, enemyMageGoblin, enemySlime, enemyEyeBall,
            enemyKnight, enemyGhostKnight, bossPrefab;
    public Slider LoadingBar;
    public TMP_Text coinText, highScoreText;

    public static MenuScript instance;
    private void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        UpdateCoins();
        MusicScript.instance.PlayMainMenuMusic();
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore.ToString();
        if(UIController.instance != null)
        {
            UIController.instance.ToggleUI(false);
        }
    }

    public void Update()
    {
        UpdateCoins();
    }

    public void OnStartGame()
    {
        if (UIController.instance != null)
        {
            UIController.instance.ToggleUI(true);
        }
    }

    public void LoadScene(int sceneID)
    {
        //TilemapManager.instance.DeleteSavedMap();
        string mapFilePath = Path.Combine(Application.persistentDataPath, $"Level 0");
        string dungeonFilePath = Path.Combine(Application.persistentDataPath, $"Level 1");
        if(File.Exists(mapFilePath))
        {
            File.Delete(mapFilePath);
        }
        if(File.Exists(dungeonFilePath))
        {
            File.Delete(dungeonFilePath);
        }
        ResetHealth();
        OnStartGame();
        StartCoroutine(LoadSceneAsync(sceneID));
    }

    IEnumerator LoadSceneAsync(int sceneID)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneID);

        LoadingScreen.SetActive(true);
        MainMenuScreen.SetActive(false);

        while(!op.isDone)
        {
            float progressVal = Mathf.Clamp01(op.progress/0.9f);
            LoadingBar.value = progressVal;
            yield return null;
        }
    }

    public void ShowShop()
    {
        MainMenuScreen.SetActive(false);
        ShopScreen.SetActive(true);
    }

    public void ResetToMenu()
    {
        ShopScreen.SetActive(false);
        MainMenuScreen.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void UpdateCoins()
    {
        coinText.text = PlayerPrefs.GetInt("TotalCoinCount", 0).ToString();
    }

    public void ResetHealth()
    {
        enemyForestGoblin.GetComponent<EnemyController>().maxHp = 100;
        enemySandGoblin.GetComponent<EnemyController>().maxHp = 125;
        enemyMageGoblin.GetComponent<EnemyController>().maxHp = 60;
        enemySlime.GetComponent<EnemyController>().maxHp = 150;
        enemyEyeBall.GetComponent<EnemyController>().maxHp = 80;
        enemyKnight.GetComponent<EnemyController>().maxHp = 180;
        enemyGhostKnight.GetComponent<EnemyController>().maxHp = 150;
        bossPrefab.GetComponent<EnemyController>().maxHp = 500;
    }
}
