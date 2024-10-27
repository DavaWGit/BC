/**
 * Autor: David Zahálka
 *
 * Správce spawnování nepřátel
 * 
 * Tento skript koordinuje spawnování nepřátel v herním světě. Řídí frekvenci, místa a typy spawnovaných
 * nepřátel na základě herních podmínek a prostředí. Zahrnuje také logiku pro speciální události jako je
 * spawnování bossů, a spravuje životní cyklus spawnovaných nepřátel, včetně jejich recyklace po smrti (pooling).
 */


using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class EnemySpawning : MonoBehaviour
{
    public GameObject enemyForestGoblin, enemySandGoblin, enemyMageGoblin, enemySlime, player, 
                        bossPrefab, enemyEyeBall, enemyKnight, enemyGhostKnight;
    private int bossCurrentHealth, bossMaxHealth;
    public float spawnRate = 4;

    private float nextSpawnTime = 0;
    public Tilemap tilemap;
    private float despawnDistance, cameraHalfHeight, cameraHalfWidth;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private List<GameObject> forestGoblins = new List<GameObject>();
    private List<GameObject> sandGoblins = new List<GameObject>();
    private List<GameObject> mageGoblins = new List<GameObject>();
    private List<GameObject> slimes = new List<GameObject>();
    private List<GameObject> knights = new List<GameObject>();
    private List<GameObject> ghostKnights = new List<GameObject>();
    private List<GameObject> eyeBalls = new List<GameObject>();

    public int checkPerFrame;
    private int enemyToCheck;
    private float bossSpawnTimer;
    public bool isBossSpawned = false;
    private float bossSpawnTime = 80f;

    private float healthIncrementInterval = 15f;
    private float lastHealthIncrementTime = 0f;
    private int healthIncrementAmount = 10;

    public float checkRadius = 0.5f;
    public LayerMask wallLayerMask;

    public ParticleSystem snowEffect;
    // Start is called before the first frame update
    public static EnemySpawning instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        // přiřazená komponenty Tilemap a objektu hráče
        GameObject tilemapObject = GameObject.Find("Tilemap");
        tilemap = tilemapObject.GetComponent<Tilemap>();

        player = GameObject.FindGameObjectWithTag("Player");

        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;

        Vector3 cameraTopRight = Camera.main.transform.position + new Vector3(cameraHalfWidth, cameraHalfHeight, 0);
        despawnDistance = 65f;

        bossSpawnTimer = bossSpawnTime;

        if(UIController.instance.bossHealthSlider.gameObject.activeSelf == true)
        {
            UIController.instance.bossHealthSlider.gameObject.SetActive(false);
        }

        InitializeEnemyPools(3);
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {
            if(LevelManager.instance.spawnTimer >= 10.0f)
            {
                if(spawnRate < 10.0f)
                {
                    spawnRate += 0.2f;
                }
                EnemyController.instance.maxHp *= Mathf.FloorToInt(1.05f);
                LevelManager.instance.spawnTimer = 0f;
            }
            Vector3 playerWorldPos = player.transform.position;
            Vector3Int playerTilemapPos = tilemap.WorldToCell(playerWorldPos);
            TileBase tile = tilemap.GetTile(playerTilemapPos);

            if(tile.name == "snow" || tile.name == "stone1")
            {
                if(snowEffect != null)
                {
                   snowEffect.Play(); 
                }
            }
            else
            {
                if(snowEffect != null)
                {
                   snowEffect.Stop(); 
                } 
            }

            if (Time.time >= nextSpawnTime) 
            {
                nextSpawnTime = Time.time + (1 / spawnRate);

                spawn(5, tile);
            }

            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                GameObject enemy = spawnedEnemies[i];
                if (enemy != null)
                {
                    float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
                    //Debug.Log("distance: " + distance + ", despawnDistance: " + despawnDistance);
                    if (distance > despawnDistance)
                    {
                        //Debug.Log($"Despawning enemy at {enemy.transform.position} which is too far from player at {player.transform.position}");
                        PutEnemyInPool(enemy);
                    }
                }
            }
        }

        if(!isBossSpawned)
        {
            bossSpawnTimer -= Time.deltaTime;
            if(bossSpawnTimer <= 0)
            {
                GameObject spawnedBoss = SpawnBoss();
                isBossSpawned = true;
                bossSpawnTimer = 80f;
                MusicScript.instance.PlayBossMusic();
                EnemyController bossHealth = spawnedBoss.GetComponent<EnemyController>();
                if(bossHealth != null)
                {
                    UIController.instance.bossHealthSlider.maxValue = bossHealth.maxHp;
                    UIController.instance.bossHealthSlider.value = bossHealth.maxHp;
                    UIController.instance.bossHealthText.text = bossHealth.maxHp + "/" + bossHealth.maxHp;
                    bossCurrentHealth = bossHealth.maxHp;
                    bossMaxHealth = bossHealth.maxHp;
                }
                UIController.instance.bossHealthSlider.gameObject.SetActive(true);
            }
        }
        IncrementEnemyHealth();
    }

    // zvyšování životů enemy s průběhem hry
    private void IncrementEnemyHealth()
    {
        if (Time.time - lastHealthIncrementTime >= healthIncrementInterval)
        {
            enemyForestGoblin.GetComponent<EnemyController>().maxHp += healthIncrementAmount;
            enemySandGoblin.GetComponent<EnemyController>().maxHp += healthIncrementAmount;
            enemyMageGoblin.GetComponent<EnemyController>().maxHp += healthIncrementAmount;
            enemySlime.GetComponent<EnemyController>().maxHp += healthIncrementAmount;
            enemyEyeBall.GetComponent<EnemyController>().maxHp += healthIncrementAmount;
            enemyKnight.GetComponent<EnemyController>().maxHp += healthIncrementAmount;
            enemyGhostKnight.GetComponent<EnemyController>().maxHp += healthIncrementAmount;

            lastHealthIncrementTime = Time.time;
        }
    }

    public void UpdateBossHealth(int damage)
    {
        UIController.instance.bossHealthSlider.value -= damage;
        bossCurrentHealth -= damage;
        UIController.instance.bossHealthText.text = bossCurrentHealth + "/" + bossMaxHealth;
    }

    // spawnování bosse
    private GameObject SpawnBoss()
    {
        float x = Random.value * (cameraHalfWidth + 4);
        float y = x < cameraHalfWidth + 2 ? Random.Range(cameraHalfHeight + 2, cameraHalfHeight + 4) : Random.Range(0, cameraHalfHeight + 4);
        x = Random.value > 0.5f ? x : -x;
        y = Random.value > 0.5f ? y : -y;

        Vector3 bossSpawnPosition = player.transform.position + new Vector3(x, y);
        GameObject bossInstance = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
        return bossInstance;
    }

    // spawnování enemáků
    private void spawn(float spawnDistance, TileBase tile)
    {
        Vector3Int spawnTilemapPos;
        TileBase spawnTile;
        bool isWallTile;
        Vector3 spawnWorldPos;

        // nalezení ideální pozice na spawn
        do
        {
            float x = Random.value * (cameraHalfWidth + 2*spawnDistance);
            float y = x < cameraHalfWidth + spawnDistance ? Random.Range(cameraHalfHeight + spawnDistance, cameraHalfHeight + 2*spawnDistance) : Random.Range(0, cameraHalfHeight + 2*spawnDistance);
            x = Random.value > 0.5f ? x : -x;
            y = Random.value > 0.5f ? y : -y;

            spawnWorldPos = player.transform.position + new Vector3(x, y);
            spawnTilemapPos = tilemap.WorldToCell(spawnWorldPos);
            spawnTile = tilemap.GetTile(spawnTilemapPos);

            Collider2D hitCollider = Physics2D.OverlapCircle(spawnWorldPos, checkRadius, wallLayerMask);

            isWallTile = hitCollider == null;

        } while (spawnTile == null || !isWallTile);
        GameObject newEnemy;
        //Debug.Log("name of the sprite: " + tile.name + " height: " + cameraHalfHeight + " width: " + cameraHalfWidth + " despawn: " + despawnDistance);
        
        // spawnutí enemáka podle biomu, ve kterém se hráč nachází
        if(tile.name == "forest")
        {
            //newEnemy = Instantiate(enemyForestGoblin, player.transform.position + new Vector3(x, y), transform.rotation);
            newEnemy = GetEnemy(enemyForestGoblin);
            newEnemy.SetActive(true);
            newEnemy.transform.position = spawnWorldPos;
            newEnemy.GetComponent<EnemyController>().ResetEnemy();
            newEnemy.GetComponent<EnemyController>().maxHp = enemyForestGoblin.GetComponent<EnemyController>().maxHp;
            newEnemy.GetComponent<EnemyController>().hp = enemyForestGoblin.GetComponent<EnemyController>().maxHp;
        }
        else if(tile.name == "sand")
        {
            //newEnemy = Instantiate(enemySandGoblin, spawnWorldPos, transform.rotation); 
            newEnemy = GetEnemy(enemySandGoblin);
            newEnemy.SetActive(true);
            newEnemy.transform.position = spawnWorldPos;
            newEnemy.GetComponent<EnemyController>().ResetEnemy();
            newEnemy.GetComponent<EnemyController>().maxHp = enemySandGoblin.GetComponent<EnemyController>().maxHp;
            newEnemy.GetComponent<EnemyController>().hp = enemySandGoblin.GetComponent<EnemyController>().maxHp;
        
        }
        else if(tile.name == "snow" || tile.name == "stone1")
        {
            newEnemy = GetEnemy(enemyKnight);
            newEnemy.SetActive(true);
            newEnemy.transform.position = spawnWorldPos;
            newEnemy.GetComponent<EnemyController>().ResetEnemy();
            newEnemy.GetComponent<EnemyController>().maxHp = enemyKnight.GetComponent<EnemyController>().maxHp;
            newEnemy.GetComponent<EnemyController>().hp = enemyKnight.GetComponent<EnemyController>().maxHp;
        }
        else if(tile.name == "Dungeon_floor1_24")
        {
            float rand = Random.Range(0.0f, 1.0f);
            if(rand <= 0.6)
            {
                //newEnemy = Instantiate(enemyGhostKnight, spawnWorldPos, transform.rotation);
                newEnemy = GetEnemy(enemyGhostKnight);
                newEnemy.SetActive(true);
                newEnemy.transform.position = spawnWorldPos;
                newEnemy.GetComponent<EnemyController>().ResetEnemy();
                newEnemy.GetComponent<EnemyController>().maxHp = enemyGhostKnight.GetComponent<EnemyController>().maxHp;
                newEnemy.GetComponent<EnemyController>().hp = enemyGhostKnight.GetComponent<EnemyController>().maxHp;
            }
            else if(rand <= 0.7)
            {
                //newEnemy = Instantiate(enemySlime, spawnWorldPos, transform.rotation);
                newEnemy = GetEnemy(enemySlime);
                newEnemy.SetActive(true);
                newEnemy.transform.position = spawnWorldPos;
                newEnemy.GetComponent<EnemyController>().ResetEnemy();
                newEnemy.GetComponent<EnemyController>().maxHp = enemySlime.GetComponent<EnemyController>().maxHp;
                newEnemy.GetComponent<EnemyController>().hp = enemySlime.GetComponent<EnemyController>().maxHp;
            }
            else
            {
                //newEnemy = Instantiate(enemyEyeBall, spawnWorldPos, transform.rotation);
                newEnemy = GetEnemy(enemyEyeBall);
                newEnemy.SetActive(true);
                newEnemy.transform.position = spawnWorldPos;
                newEnemy.GetComponent<EnemyController>().ResetEnemy();
                newEnemy.GetComponent<EnemyController>().maxHp = enemyEyeBall.GetComponent<EnemyController>().maxHp;
                newEnemy.GetComponent<EnemyController>().hp = enemyEyeBall.GetComponent<EnemyController>().maxHp;
            }
        }
        else
        {
            float rand = Random.Range(0.0f, 1.0f);
            if(rand <= 0.35)
            {
                //newEnemy = Instantiate(enemyMageGoblin, spawnWorldPos, transform.rotation);
                newEnemy = GetEnemy(enemyMageGoblin);
                newEnemy.SetActive(true);
                newEnemy.transform.position = spawnWorldPos;
                newEnemy.GetComponent<EnemyController>().ResetEnemy();
                newEnemy.GetComponent<EnemyController>().maxHp = enemyMageGoblin.GetComponent<EnemyController>().maxHp;
                newEnemy.GetComponent<EnemyController>().hp = enemyMageGoblin.GetComponent<EnemyController>().maxHp;
            }
            else if(rand <= 0.7)
            {
                //newEnemy = Instantiate(enemySlime, spawnWorldPos, transform.rotation);
                newEnemy = GetEnemy(enemySlime);
                newEnemy.SetActive(true);
                newEnemy.transform.position = spawnWorldPos;
                newEnemy.GetComponent<EnemyController>().ResetEnemy();
                newEnemy.GetComponent<EnemyController>().maxHp = enemySlime.GetComponent<EnemyController>().maxHp;
                newEnemy.GetComponent<EnemyController>().hp = enemySlime.GetComponent<EnemyController>().maxHp;
            }
            else
            {
                //newEnemy = Instantiate(enemyForestGoblin, spawnWorldPos, transform.rotation);
                newEnemy = GetEnemy(enemyForestGoblin);
                newEnemy.SetActive(true);
                newEnemy.transform.position = spawnWorldPos;
                newEnemy.GetComponent<EnemyController>().ResetEnemy();
                newEnemy.GetComponent<EnemyController>().maxHp = enemyForestGoblin.GetComponent<EnemyController>().maxHp;
                newEnemy.GetComponent<EnemyController>().hp = enemyForestGoblin.GetComponent<EnemyController>().maxHp;
            }    
        }
        spawnedEnemies.Add(newEnemy);
    }

    // získání enemáka z poolu
    public GameObject GetEnemy(GameObject prefab)
    {
        GameObject enemy = null;
        if(prefab == enemyForestGoblin)
        {
            if(forestGoblins.Count == 0)
            {
                enemy = Instantiate(enemyForestGoblin);
            }
            else
            {
                enemy = forestGoblins[0];
                forestGoblins.RemoveAt(0);
            }
        }
        else if(prefab == enemySandGoblin)
        {
            if(sandGoblins.Count == 0)
            {
                enemy = Instantiate(enemySandGoblin);
            }
            else
            {
                enemy = sandGoblins[0];
                sandGoblins.RemoveAt(0);
            }
        }
        else if(prefab == enemyMageGoblin)
        {
            if(mageGoblins.Count == 0)
            {
                enemy = Instantiate(enemyMageGoblin);
            }
            else
            {
                enemy = mageGoblins[0];
                mageGoblins.RemoveAt(0);
            }
        }
        else if(prefab == enemySlime)
        {
            if(slimes.Count == 0)
            {
                enemy = Instantiate(enemySlime);
            }
            else
            {
                enemy = slimes[0];
                slimes.RemoveAt(0);
            }
        }
        else if(prefab == enemyKnight)
        {
            if(knights.Count == 0)
            {
                enemy = Instantiate(enemyKnight);
            }
            else
            {
                enemy = knights[0];
                knights.RemoveAt(0);
            }
        }
        else if(prefab == enemyGhostKnight)
        {
            if(ghostKnights.Count == 0)
            {
                enemy = Instantiate(enemyGhostKnight);
            }
            else
            {
                enemy = ghostKnights[0];
                ghostKnights.RemoveAt(0);
            }
        }
        else if(prefab == enemyEyeBall)
        {
            if(eyeBalls.Count == 0)
            {
                enemy = Instantiate(enemyEyeBall);
            }
            else
            {
                enemy = eyeBalls[0];
                eyeBalls.RemoveAt(0);
            }
        }

        return enemy;
    }

    // vložení enemáka do poolu
    public void PutEnemyInPool(GameObject enemyToPool)
    {
        enemyToPool.transform.position = Vector3.zero;
        enemyToPool.SetActive(false);
    
        if (spawnedEnemies.Contains(enemyToPool)) {
            spawnedEnemies.Remove(enemyToPool);
            //Debug.Log($"Removed {enemyToPool.name} from spawnedEnemies.");
        }

        if(enemyToPool.name == "ForestGoblin(Clone)")
        {
            forestGoblins.Add(enemyToPool);
        }
        else if(enemyToPool.name == "SandGoblin(Clone)")
        {
            sandGoblins.Add(enemyToPool);
        }
        else if(enemyToPool.name == "MageGoblin(Clone)")
        {
            mageGoblins.Add(enemyToPool);
        }
        else if(enemyToPool.name == "Enemy_knight(Clone)")
        {
            knights.Add(enemyToPool);
        }
        else if(enemyToPool.name == "Ghost_Knight(Clone)")
        {
            ghostKnights.Add(enemyToPool);
        }
        else if(enemyToPool.name == "Slime(Clone)")
        {
            slimes.Add(enemyToPool);
        }
        else if(enemyToPool.name == "EyeBall(Clone)")
        {
            eyeBalls.Add(enemyToPool);
        }
    }

    private void InitializeEnemyPools(int countPerType)
    {
        PopulatePool(enemyForestGoblin, forestGoblins, countPerType);
        PopulatePool(enemySandGoblin, sandGoblins, countPerType);
        PopulatePool(enemyMageGoblin, mageGoblins, countPerType);
        PopulatePool(enemySlime, slimes, countPerType);
        PopulatePool(enemyKnight, knights, countPerType);
        PopulatePool(enemyGhostKnight, ghostKnights, countPerType);
        PopulatePool(enemyEyeBall, eyeBalls, countPerType);
    }

    // naplnění poolů na začátku hry
    private void PopulatePool(GameObject prefab, List<GameObject> pool, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newEnemy = Instantiate(prefab);
            newEnemy.SetActive(false);
            pool.Add(newEnemy);
        }
    }
}