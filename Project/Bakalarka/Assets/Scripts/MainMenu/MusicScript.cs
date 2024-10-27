/**
 * Autor: David Zahálka
 *
 * Skript pro správu hudby
 * 
 * Tento skript spravuje přehrávání hudby během různých fází hry, včetně
 * hlavního menu, hlavní úrovně, dungeonu, boje s bossem a při game over scéně.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour
{
    public static MusicScript instance;
    public AudioSource mainMenuMusic, mainLevelMusic, dungeonLevelMusic, bossMusic, gameOverMusic;
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMainMenuMusic()
    {
        mainMenuMusic.Play();
        mainLevelMusic.Stop();
        dungeonLevelMusic.Stop();
        bossMusic.Stop();
        gameOverMusic.Stop();
    }
    public void PlayMainLevelMusic()
    {
        mainMenuMusic.Stop();
        mainLevelMusic.Play();
        dungeonLevelMusic.Stop();
        bossMusic.Stop();
        gameOverMusic.Stop();
    }
    public void PlayDungeonLevelMusic()
    {
        mainMenuMusic.Stop();
        mainLevelMusic.Stop();
        dungeonLevelMusic.Play();
        bossMusic.Stop();
        gameOverMusic.Stop();
    }
    public void PlayBossMusic()
    {
        mainMenuMusic.Stop();
        mainLevelMusic.Stop();
        dungeonLevelMusic.Stop();
        bossMusic.Play();
        gameOverMusic.Stop();
    }
    public void PlayGameOverMusic()
    {
        mainMenuMusic.Stop();
        mainLevelMusic.Stop();
        dungeonLevelMusic.Stop();
        bossMusic.Stop();
        gameOverMusic.Play();
    }
}
