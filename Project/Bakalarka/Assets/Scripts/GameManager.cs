using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentExperience = 0;
    public float timePassed = 0.0f;
    public int currentLevel = 1;
    public int points = 0;
    public int upgradePoints = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void StartNewGame()
    {
        currentExperience = 0;
        timePassed = 0.0f;
        currentLevel = 1;
        points = 0;
        upgradePoints = 0;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            StartNewGame();
        }
    }
}
