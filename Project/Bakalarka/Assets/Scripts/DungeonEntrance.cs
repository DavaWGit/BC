/**
 * Autor: David Zahálka
 *
 * Vstup do dungeonu
 * 
 * Skript spravuje interakce s vchodem do dungeonu. Zajišťuje načítání dungeonu,
 * zobrazuje UI pro vstup a řídí přístupnost dungeonu.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DungeonEntrance : MonoBehaviour
{
    public string dungeonToLoad;
    public string areaTransitionName;
     public float activationTime = 2f;

    private bool playerInRange = false;
    public float delay = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        if(areaTransitionName == PlayerMovement.instance.areaTransitionName)
        {
            PlayerMovement.instance.transform.position = transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UIController.instance.enterDungeon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UIController.instance.enterDungeon.SetActive(false);
        }
    }

    private void Update()
    {
        if(playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if(!EnemySpawning.instance.isBossSpawned)
            {
                LoadScene();
            }
            else
            {
                ShowAndHide();
            } 
        } 
    }

    public void ShowAndHide()
    {
        UIController.instance.cantEnterDungeon.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        UIController.instance.cantEnterDungeon.SetActive(false);
    }

    private void LoadScene()
    {
        if (dungeonToLoad == "Dungeon")
        {
            TilemapManager.instance.SaveMap();
            TilemapManager.instance.ClearSavedMap();
            PlayerMovement.instance.areaTransitionName = "Dungeon";
        }
        else if (dungeonToLoad == "Main")
        {
            PlayerMovement.instance.areaTransitionName = "Main";
            TilemapManager.instance.SaveMap();
            TilemapManager.instance.ClearSavedMap();
        }

        SceneManager.LoadScene(dungeonToLoad);
    }
}
