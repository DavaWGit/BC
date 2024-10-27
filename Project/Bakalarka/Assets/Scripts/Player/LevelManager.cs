/**
 * Autor: David Zahálka
 *
 * Správce úrovní
 * 
 * Skript spravuje logiku herních úrovní, včetně sledování času, správy bodů a končení úrovně.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    private void Awake()
    {
        instance = this;
    }

    public bool gameActive;
    public float timer;
    public float spawnTimer;
    public float healthTimer;
    public int points;
    // Start is called before the first frame update
    void Start()
    {
        gameActive = true;
        string currentSceneName = SceneManager.GetActiveScene().name;
        if(currentSceneName == "Main")
        {
            MusicScript.instance.PlayMainLevelMusic();
        }
        else if(currentSceneName == "Dungeon")
        {
            MusicScript.instance.PlayDungeonLevelMusic();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameActive)
        {
            timer += Time.deltaTime;
            GameManager.instance.timePassed += Time.deltaTime;
            spawnTimer += Time.deltaTime;
            healthTimer += Time.deltaTime;
            UIController.instance.updateTimer(GameManager.instance.timePassed);
        }
    }

    public void EndLevel()
    {
        gameActive = false;

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if ((GameManager.instance.points + Mathf.FloorToInt(GameManager.instance.timePassed)) > highScore)
        {
            PlayerPrefs.SetInt("HighScore", (GameManager.instance.points + Mathf.FloorToInt(GameManager.instance.timePassed)));
            PlayerPrefs.Save();
        }

        StartCoroutine(EndLevelCo());
    }

    IEnumerator EndLevelCo()
    {
        yield return new WaitForSeconds(1f);

        float min = Mathf.FloorToInt(GameManager.instance.timePassed / 60.0f);
        float sec = Mathf.FloorToInt(GameManager.instance.timePassed % 60);

        UIController.instance.timeSurvivedText.text = "Time Survived: " + min.ToString() + ":" + sec.ToString("00");
        UIController.instance.totalPointsText.text = "Enemy Score: " + GameManager.instance.points + "  + Time Points: " + Mathf.FloorToInt(GameManager.instance.timePassed);
        UIController.instance.finalPointsText.text = "Total Points: " + (GameManager.instance.points + Mathf.FloorToInt(GameManager.instance.timePassed));
        UIController.instance.isGameOver = true;
        UIController.instance.endScreenPanel.SetActive(true);
        MusicScript.instance.PlayGameOverMusic();
    }

    public void AddScore(int pointsToAdd)
    {
        GameManager.instance.points += pointsToAdd;
    }
}
