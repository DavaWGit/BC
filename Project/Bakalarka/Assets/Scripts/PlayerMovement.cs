/**
 * Autor: David Zahálka
 *
 * Skript pohybu hráče
 * 
 * Tento skript řídí pohyb hráčovy postavy, omezuje pohyb podle hranic mapy,
 * spravuje zdraví, statistiky hráče a upgrade schopností.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D playerRB;
    public int maxHp = 100;
    public float ms;
    public int hp, strength, agility, intelligence, luck, vitality, speed;
    public static PlayerMovement instance;
    public Animator animator;
    public GameObject deathEffect;

    private Vector3 bottomLeftLimit, topRightLimit;
    public string areaTransitionName;
    private float healingAccumulator = 0f;
    private float healingRate = 2f;

    public GameObject dungeonEntrance, arrowUI;
    private bool dungeonFinderPurchased = false;
    // Start is called before the first frame update
    void Start()
    {
        hp = maxHp;
        if(instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
        }
        UIController.instance.UpdateStats();
        DontDestroyOnLoad(gameObject);

        
    }

    // Update is called once per frame
    void Update()
    {
        // pohyb postavy a animace
        playerRB.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * ms;

        animator.SetFloat("moveX", playerRB.velocity.x);
        animator.SetFloat("moveY", playerRB.velocity.y);

        if(Input.GetAxisRaw("Horizontal") == 1 || Input.GetAxisRaw("Horizontal") == -1 || Input.GetAxisRaw("Vertical") == 1 || Input.GetAxisRaw("Vertical") == -1 )
        {
            animator.SetFloat("lastMoveX", Input.GetAxisRaw("Horizontal"));
            animator.SetFloat("lastMoveY", Input.GetAxisRaw("Vertical"));
        }
        
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, bottomLeftLimit.x, topRightLimit.x), Mathf.Clamp(transform.position.y, bottomLeftLimit.y, topRightLimit.y), transform.position.z);
        // vylepsení magnet na mince a regenerace životů
        if(PlayerPrefs.GetInt("CoinMagnet", 0) == 1)
        {
            LayerMask coinLayerMask = LayerMask.GetMask("coins");
            Collider2D[] coins = Physics2D.OverlapCircleAll(transform.position, 10, coinLayerMask);
            foreach(Collider2D coin in coins)
            {
                coin.transform.position = Vector2.MoveTowards(coin.transform.position, transform.position, ms * Time.deltaTime);
            }
        }
        if(PlayerPrefs.GetInt("HealingOT", 0) == 1)
        {
            if(hp < maxHp)
            {
                healingAccumulator += healingRate * Time.deltaTime;
                if(healingAccumulator >= 1f)
                {
                    int healAmount = Mathf.FloorToInt(healingAccumulator);
                    hp += healAmount;
                    hp = Mathf.Min(hp, maxHp);
                    healingAccumulator -= healAmount;
                    transform.Find("HealthBar/Bar").localScale = new Vector3((float)hp/maxHp, 0.1f);
                }
            }
        }

        dungeonEntrance = GameObject.Find("Dungeon_Door(Clone)");
        GameObject canvas = GameObject.Find("UI");
        arrowUI = canvas.transform.Find("DungeonArrow").gameObject;
        if(PlayerPrefs.GetInt("DungeonFinder", 0) == 1)
        {
            DungeonFinderBought();
        }

        if(dungeonFinderPurchased)
        {
            if(dungeonEntrance != null || arrowUI != null)
            {
                UpdateArrowDirection();
            }
        }
    }

    // udeření hráče nepřítelem
    public void hit(int damage, GameObject sender)
    {
        if(sender.layer == gameObject.layer)
        {
            return;
        }
        float damageReductionMultiplier = 1f - (0.02f * agility);
        int finalDamage = Mathf.FloorToInt(damage * damageReductionMultiplier);
        hp -= finalDamage;
        transform.Find("HealthBar/Bar").localScale = new Vector3((float)hp/maxHp, 0.1f);
        DamageNumberController.instance.SpawnDamage(finalDamage, transform.position, Color.red);
        if (hp <= 0)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            LevelManager.instance.EndLevel();
        }
    }

    // vylepsovani vsech statů
    public void upgradeStrength()
    {
        strength++;
        GameManager.instance.upgradePoints--;
        UIController.instance.UpdateStats();
    }

    public void upgradeAgility()
    {
        agility++;
        GameManager.instance.upgradePoints--;
        UIController.instance.UpdateStats();
    }

    public void upgradeIntelligence()
    {
        intelligence++;
        GameManager.instance.upgradePoints--;
        UIController.instance.UpdateStats();
    }

    public void upgradeVitality()
    {
        vitality++;
        maxHp += 3;

        float healthPercentage = (float)hp / (maxHp - 3);
        hp = Mathf.RoundToInt(healthPercentage * maxHp);

        transform.Find("HealthBar/Bar").localScale = new Vector3((float)hp / maxHp, 0.1f);
        GameManager.instance.upgradePoints--;
        UIController.instance.UpdateStats();
    }

    public void upgradeSpeed()
    {
        speed++;
        ms+=0.2f;
        GameManager.instance.upgradePoints--;
        UIController.instance.UpdateStats();
    }

    public void upgradeLuck()
    {
        luck++;
        GameManager.instance.upgradePoints--;
        UIController.instance.UpdateStats();
    }

    // limity na pohyb hráče jen v oblasti mapy
    public void SetBounds(Vector3 botLeft, Vector3 topRight)
    {
        bottomLeftLimit = botLeft + new Vector3(1f, 1f, 0f);
        topRightLimit = topRight + new Vector3(-1f, -1f, 0f);
    }

    // vylepseni na hledani podzemí
    void UpdateArrowDirection()
    {
        Vector3 entrancePosition = dungeonEntrance.transform.position;
        Vector3 direction = (entrancePosition - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
        
        arrowUI.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public void DungeonFinderBought()
    {
        arrowUI.SetActive(true);
        dungeonFinderPurchased = true;
    }
}
