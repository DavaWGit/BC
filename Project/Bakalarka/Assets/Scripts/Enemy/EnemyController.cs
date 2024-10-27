/**
 * Autor: David Zahálka
 *
 * Kontroler nepřítele
 * 
 * Tento skript řídí všechny aspekty chování nepřítele včetně pohybu, útoků, detekce hráče a správy zdraví.
 * Skript koordinuje mezi různými systémy, jako je AI pro pohyb, útočné chování a reakce na získání poškození.
 * Zahrnuje také logiku pro pokles zdraví a následnou smrt nepřítele.
 *
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private List<EnemySteering> steering;
    [SerializeField]
    private List<EnemyDetector> detectors;
    [SerializeField]
    private EnemyData enemyData;
    [SerializeField]
    private float detectionDelay = 0.05f, updateDelay = 0.06f;

    [SerializeField]
    private EnemyAI movementDir;
    [SerializeField] 
    private float attackDistance = 0.5f;
    [SerializeField] 
    private Vector2 movement;
    public Animator animator;

    public Rigidbody2D RB;
    public Transform player;
    public float speed = 2.0f;
    public int maxHp;
    public int expAward, coinAward, pointAward;
    public float coinDropRate;
    public int hp;
    bool chase = false;
    public bool isBoss = false;
    public bool playerIsEast;

    public GameObject weapon;

    public AudioSource hitSound, hitSound1, hitSound2;

    // Start is called before the first frame update
    public static EnemyController instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        hp = maxHp;
        InvokeRepeating("PerformDetection", 0, detectionDelay);
    }

    public void hit(int damage, GameObject sender)
    {
        if(sender.layer == gameObject.layer)
        {
            return;
        }
        damage += PlayerMovement.instance.strength;
        hp -= damage;
        float rand = Random.Range(0.0f, 1.0f);
        if(rand <= 0.3)
        {
            hitSound.Play();
        }
        else if(rand <= 0.6)
        {
            hitSound1.Play();
        }
        else if(rand <= 1.0)
        {
            hitSound2.Play();
        }
        if(!isBoss)
        {
            transform.Find("HealthBar/Bar").localScale = new Vector3((float)hp/maxHp, 0.1f);
        }
        DamageNumberController.instance.SpawnDamage(damage, transform.position, Color.white);
        if(isBoss)
        {
            EnemySpawning.instance.UpdateBossHealth(damage);
        }
        if (hp <= 0)
        {
            GameManager.instance.points += pointAward;
            UIController.instance.UpdateScore();
            ExperienceController.instance.GetExp(expAward);
            /*coinDropRate += PlayerMovement.instance.luck/10;
            if(coinDropRate > 1.0f)
            {
                coinDropRate = 1.0f;
            }*/
            float luckBonus = PlayerMovement.instance.luck * 0.01f;
            float totalDropRate = coinDropRate + luckBonus;
            int extraCoins = 0;

            if (totalDropRate > 1.0f)
            {
                extraCoins = Mathf.FloorToInt((totalDropRate - 1.0f) * 10);
                totalDropRate = 1.0f;
            }
            if(Random.value <= totalDropRate)
            {
                CoinController.instance.DropCoin(transform.position, coinAward+extraCoins);
            }
            EnemySpawning.instance.PutEnemyInPool(gameObject);
            if(isBoss)
            {
                EnemySpawning.instance.isBossSpawned = false;
                MusicScript.instance.PlayMainLevelMusic();
                UIController.instance.bossHealthSlider.gameObject.SetActive(false);
                EnemySpawning.instance.bossPrefab.GetComponent<EnemyController>().maxHp += 300;
            }
        }
    }

    private void PerformDetection()
    {
        foreach(EnemyDetector detector in detectors)
        {
            detector.Detect(enemyData);
        }

        float[] danger = new float[8];
        float[] interest = new float[8];

        foreach(EnemySteering steer in steering)
        {
            (danger, interest) = steer.GetSteering(danger, interest, enemyData);
        }
    }

    void Update()
    {
        EnemyAttack canAttack = GetComponentInChildren<EnemyAttack>();
        if(canAttack.canAttack && isBoss)
        {
            movement = Vector2.zero;
        }
        if(enemyData.currentTarget != null)
        {
            if(chase == false)
            {
                chase = true;
                StartCoroutine(ChaseAndAttack());
            }
            if(!isBoss)
            {
                Vector3 directionToPlayer = enemyData.currentTarget.position - weapon.transform.position;
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                weapon.transform.rotation = Quaternion.Euler(0f, 0f, angle);

                Vector3 weaponScale = weapon.transform.localScale;
                weaponScale.y = directionToPlayer.x < 0 ? -1f : 1f;
                weapon.transform.localScale = weaponScale;
            }
            if (isBoss)
            {
                playerIsEast = enemyData.currentTarget.position.x > transform.position.x;

                float rotationAngle = playerIsEast ? 180f : 0f;
                weapon.transform.rotation = Quaternion.Euler(0f, rotationAngle, 0f);
            }
        }
        else if(enemyData.GetTargetsCount() > 0)
        {
            enemyData.currentTarget = enemyData.targets[0];
        }
        transform.Translate(movement*speed*Time.deltaTime);
        animator.SetFloat("moveX", movement.x);
        animator.SetFloat("moveY", movement.y);
    }

    private IEnumerator ChaseAndAttack()
    {
        if(enemyData.currentTarget == null)
        {
            movement = Vector2.zero;
            chase = false;
            yield return null;
        }
        else
        {
            float dist = Vector2.Distance(enemyData.currentTarget.position, transform.position);

            if(dist < attackDistance)
            {
                movement = Vector2.zero;
                StartCoroutine(ChaseAndAttack());
            }
            else
            {
                movement = movementDir.GetDirection(steering, enemyData);
                yield return new WaitForSeconds(updateDelay);
                StartCoroutine(ChaseAndAttack());
            }
        }
    }

    public void ResetEnemy()
    {
        this.hp = this.maxHp;
        transform.Find("HealthBar/Bar").localScale = new Vector3(1f, 0.1f);
        transform.rotation = Quaternion.identity;
        movement = Vector2.zero;
        chase = false;
    }
}