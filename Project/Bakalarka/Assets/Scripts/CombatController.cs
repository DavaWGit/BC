/**
 * Autor: David Zahálka
 *
 * Souboj kontroler
 * 
 * Řídí bojové interakce postavy, spravuje vizuální efekty a zvuky související s bojem a detekci zásahů.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint;   // bod odkud utoky vychazeji
    public GameObject hitEffect;
    public Transform weaponTransform;

    public float attackRange = 0.5f;
    public float attackSpeed = 1f;
    public float knockbackStrength;
    public int damage;

    public LayerMask enemyLayers;
    public Vector2 PointerPosition { get; set; }

    private float nextAttackTime = 0;

    public AudioSource attackSound; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
                nextAttackTime = Time.time + (1f/attackSpeed);
            }
        }
        // otočení zbraně směrem k pozici kurzoru
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference.Normalize();
        float rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation_z);

        Vector2 scale = transform.localScale;
        if(difference.x < 0)
        {
            scale.y = -1;
        }
        else if(difference.x > 0)
        {
            scale.y = 1f;
        }
        transform.localScale = scale;
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        attackSound.Play();
    }

    public void inRangePlayer()
    {
        // detekuje nepřítele v dosahu útoku
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if(enemyCollider is BoxCollider2D)
            {
                knockback(enemyCollider);
                enemyCollider.GetComponent<EnemyController>().hit(damage, transform.parent.gameObject);
                EnemyHit(enemyCollider.transform);
            }    
        }
    }
    private void knockback(Collider2D enemyCollider)
    {
        if(enemyCollider is BoxCollider2D)
        {
            Transform enemyTransform = enemyCollider.transform;
            Vector2 direction = (enemyTransform.position - attackPoint.position).normalized;
            enemyCollider.GetComponent<Rigidbody2D>().AddForce(direction * knockbackStrength, ForceMode2D.Impulse);
        }
    }
    // vytvoření efektu úderu nepřítele
    void EnemyHit(Transform enemyTransform)
    {
        Vector2 playerPosition = transform.position;

        Vector2 enemyPosition = enemyTransform.position;
        Vector2 direction = (enemyPosition - playerPosition).normalized;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject hitEffectTmp = Instantiate(hitEffect, enemyTransform.position,Quaternion.identity);
        hitEffectTmp.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle-90));
        Destroy(hitEffectTmp, 2);
    }
}
