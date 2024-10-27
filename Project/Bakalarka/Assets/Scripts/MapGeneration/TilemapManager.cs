/**
 * Autor: David Zahálka
 *
 * Ukládání/Načítání mapy, objektů a nepřátel
 * 
 * Tento skript umožňuje správu herních map a objektů. Zajišťuje ukládání a načítání
 * stavu hry, včetně dlaždic a herních objektů, do souborů JSON.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.SceneManagement;

[Serializable]
public class SavedLevel
{
    public int levelIndex;
    public string name;
    public SavedTile[] tiles;
    public SavedGameObject[] gameObjects;
}

[Serializable]
public struct SavedTile
{
    public Vector3Int position; // pozice políčka na mapě
    public string tileName; // název políčka
}

[Serializable]
public struct SavedGameObject
{
    public Vector3 position;    // pozice objektu
    public string gameObjectType;   // typ objektu
    public string objectName;   // název objektu
}

public class TilemapManager : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap, tilemap1;
    [SerializeField] public TileBase waterTile, sandTile, stoneTile, snowTile, forestTile, grassTile, dungeonFloorTile,
                    D0Tile, D1Tile, D2Tile, D3Tile, D4Tile, D5Tile, D6Tile, D7Tile, D8Tile, D9Tile,
                    D10Tile, D11Tile, D12Tile, D13Tile, D14Tile;    // seznam políček
    [SerializeField] public GameObject goblinForestPrefab, goblinMagePrefab, goblinSandPrefab, slimePrefab, eyeBallPrefab, knightPrefab, ghostKnightPrefab, bush1Prefab, bush2Prefab, cactus1Prefab, cactus2Prefab,
                            miniCactusPrefab, tree1Prefab, tree2Prefab, tree3Prefab, boulder1Prefab,
                            boulder2Prefab, boulder3Prefab, boulder4Prefab, smallrock1Prefab, smallrock2Prefab, dungeonPrefab,
                            logPrefab, snowCapPrefab, snowRockPrefab, signPrefab;   // seznam objektů
    [SerializeField] public int levelIndex;

    private GameObject exitInstance;
    public static TilemapManager instance;
    public static bool mapGenerated { get; set; } = false;
    
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
        /*if(!tilemap_test.instance.firstStarted)
        {
            LoadMap();
        }*/
    }
    // uložení mapy a objektů do JSONu
    public void SaveMap()
    {
        List<SavedTile> savedTiles = new List<SavedTile>();

        foreach(var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if(tilemap.HasTile(pos))
            {
                TileBase tile = tilemap.GetTile(pos);
                string tileName = tile.name;
                savedTiles.Add(new SavedTile{position = pos, tileName = tileName});
            }
        }
        foreach(var pos in tilemap1.cellBounds.allPositionsWithin)
        {
            if(tilemap1.HasTile(pos))
            {
                TileBase tile = tilemap1.GetTile(pos);
                string tileName = tile.name;
                savedTiles.Add(new SavedTile{position = pos, tileName = tileName});
            }
        }

        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] propObjects = GameObject.FindGameObjectsWithTag("Prop");
        SavedGameObject[] savedGameObjects = enemyObjects
                .Select(enemy => new SavedGameObject { position = enemy.transform.position, gameObjectType = "Enemy", objectName = enemy.name})
                .Concat(propObjects
                .Select(prop => new SavedGameObject { position = prop.transform.position, gameObjectType = "Prop", objectName = prop.name}))
                .ToArray();

        // vytvoření a uložení souboru
        SavedLevel savedLevel = new SavedLevel
        {
            levelIndex = levelIndex,
            name = $"Level {levelIndex}",
            tiles = savedTiles.ToArray(),
            gameObjects = savedGameObjects
        };

        string json = JsonUtility.ToJson(savedLevel);
        string path = Path.Combine(Application.persistentDataPath, savedLevel.name);
        File.WriteAllText(path, json);
    }
    // metoda vyčištění mapy
    public void ClearSavedMap()
    {
        var maps = FindObjectsOfType<Tilemap>();
        foreach(var tilemap in maps)
        {
            tilemap.ClearAllTiles();
        }
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        GameObject[] props = GameObject.FindGameObjectsWithTag("Prop");
        foreach (GameObject prop in props)
        {
            Destroy(prop);
        }
    }
    // metoda načtení mapy a objektů z JSONu
    public void LoadMap()
    {
        string mapFilePath = Path.Combine(Application.persistentDataPath, $"Level {levelIndex}");
        if (!File.Exists(mapFilePath))
        {
            return;
        }

        string json = File.ReadAllText(mapFilePath);
        SavedLevel savedLevel = JsonUtility.FromJson<SavedLevel>(json);
        ClearSavedMap();

        foreach (var savedTile in savedLevel.tiles)
        {
            Vector3Int position = savedTile.position;
            string tileName = savedTile.tileName;

            switch (tileName)
            {
                case "grass":
                    tilemap.SetTile(position, grassTile);
                    break;
                case "Water 1":
                    tilemap1.SetTile(position, waterTile);
                    break;
                case "sand":
                    tilemap.SetTile(position, sandTile);
                    break;
                case "forest":
                    tilemap.SetTile(position, forestTile);
                    break;
                case "stone1":
                    tilemap.SetTile(position, stoneTile);
                    break;
                case "snow":
                    tilemap.SetTile(position, snowTile);
                    break;
                case "Dungeon_floor1_24":
                    tilemap.SetTile(position, dungeonFloorTile);
                    break;
                case "D0":
                    tilemap1.SetTile(position, D0Tile);
                    break;
                case "D1":
                    tilemap1.SetTile(position, D1Tile);
                    break;
                case "D2":
                    tilemap1.SetTile(position, D2Tile);
                    break;
                case "D3":
                    tilemap1.SetTile(position, D3Tile);
                    break;
                case "D4":
                    tilemap1.SetTile(position, D4Tile);
                    break;
                case "D5":
                    tilemap1.SetTile(position, D5Tile);
                    break;
                case "D6":
                    tilemap1.SetTile(position, D6Tile);
                    break;
                case "D7":
                    tilemap1.SetTile(position, D7Tile);
                    break;
                case "D8":
                    tilemap1.SetTile(position, D8Tile);
                    break;
                case "D9":
                    tilemap1.SetTile(position, D9Tile);
                    break;
                case "D10":
                    tilemap1.SetTile(position, D10Tile);
                    break;
                case "D11":
                    tilemap1.SetTile(position, D11Tile);
                    break;
                case "D12":
                    tilemap1.SetTile(position, D12Tile);
                    break;
                case "D13":
                    tilemap1.SetTile(position, D13Tile);
                    break;
                case "D14":
                    tilemap1.SetTile(position, D14Tile);
                    break;
            }
        }

        foreach (var savedObject in savedLevel.gameObjects)
        {
            Vector3 position = savedObject.position;
            string gameObjectType = savedObject.gameObjectType;
            string objectName = savedObject.objectName;

            switch (gameObjectType)
            {
                case "Enemy":
                    InstantiateEnemy(position, objectName);
                    break;
                case "Prop":
                    InstantiateProp(position, objectName);
                    break;
            }
        }
    }
    private void InstantiateEnemy(Vector3 position, string enemyType)
    {
        switch(enemyType)
        {
            case "ForestGoblin(Clone)":
                Instantiate(goblinForestPrefab, position, Quaternion.identity);
                break;
            case "MageGoblin(Clone)":
                Instantiate(goblinMagePrefab, position, Quaternion.identity);
                break;
            case "SandGoblin(Clone)":
                Instantiate(goblinSandPrefab, position, Quaternion.identity);
                break;
            case "Enemy_knight(Clone)":
                Instantiate(knightPrefab, position, Quaternion.identity);
                break;
            case "Ghost_Knight(Clone)":
                Instantiate(ghostKnightPrefab, position, Quaternion.identity);
                break;
            case "Slime(Clone)":
                Instantiate(slimePrefab, position, Quaternion.identity);
                break;
            case "EyeBall(Clone)":
                Instantiate(eyeBallPrefab, position, Quaternion.identity);
                break;
        }
    }
    private void InstantiateProp(Vector3 position, string propName)
    {
        switch(propName)
        {
            case "Bush1(Clone)":
                Instantiate(bush1Prefab, position, Quaternion.identity);
                break;
            case "Bush2(Clone)":
                Instantiate(bush2Prefab, position, Quaternion.identity);
                break;
            case "Cactus1(Clone)":
                Instantiate(cactus1Prefab, position, Quaternion.identity);
                break;
            case "Cactus2(Clone)":
                Instantiate(cactus2Prefab, position, Quaternion.identity);
                break;
            case "miniCactus(Clone)":
                Instantiate(miniCactusPrefab, position, Quaternion.identity);
                break;
            case "boulder1(Clone)":
                Instantiate(boulder1Prefab, position, Quaternion.identity);
                break;
            case "boulder2(Clone)":
                Instantiate(boulder2Prefab, position, Quaternion.identity);
                break;
            case "boulder3(Clone)":
                Instantiate(boulder3Prefab, position, Quaternion.identity);
                break;
            case "boulder4(Clone)":
                Instantiate(boulder4Prefab, position, Quaternion.identity);
                break;
            case "smallrock1(Clone)":
                Instantiate(smallrock1Prefab, position, Quaternion.identity);
                break;
            case "smallrock2(Clone)":
                Instantiate(smallrock2Prefab, position, Quaternion.identity);
                break;
            case "tree1(Clone)":
                Instantiate(tree1Prefab, position, Quaternion.identity);
                break;
            case "tree2(Clone)":
                Instantiate(tree2Prefab, position, Quaternion.identity);
                break;
            case "tree3(Clone)":
                Instantiate(tree3Prefab, position, Quaternion.identity);
                break;
            case "Sign(Clone)":
                Instantiate(signPrefab, position, Quaternion.identity);
                break;
            case "Log(Clone)":
                Instantiate(logPrefab, position, Quaternion.identity);
                break;
            case "SnowCap(Clone)":
                Instantiate(snowCapPrefab, position, Quaternion.identity);
                break;    
            case "SnowRock(Clone)":
                Instantiate(snowRockPrefab, position, Quaternion.identity);
                break;
            case "Dungeon_Door(Clone)":
                exitInstance = Instantiate(dungeonPrefab, position, Quaternion.identity);
                if(levelIndex == 1)
                {
                    exitInstance.GetComponent<DungeonEntrance>().dungeonToLoad = "Main";
                    exitInstance.GetComponent<DungeonEntrance>().areaTransitionName = "Dungeon";
                }
                break;
        }
    }

    // odstranění uložených map
    public void DeleteSavedMap()
    {
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
    }

}