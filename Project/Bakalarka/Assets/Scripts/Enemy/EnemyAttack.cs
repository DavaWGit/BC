/**
 * Autor: David Zahálka
 *
 * Útočný systém nepřítele
 * 
 * Tento skript spravuje útočné chování nepřítele. Obsahuje
 * logiku pro spuštění animací útoků, kontrolu dostupnosti útoku a správu zpoždění mezi jednotlivými útoky.
 * Skript také určuje, zda je nepřítel v dosahu pro zahájení útoku na hráče.
 *
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

    public Animator animator;
    public Transform attackPoint;
    public float delay = 0.3f;
    public bool canAttack = false;
    public bool isRanged = false;
    public float attackRange;
    public int damage;
    public GameObject fireball;
    private EnemyController enemyController;
    // Start is called before the first frame update
    void Start()
    {
        enemyController = GetComponentInParent<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!canAttack)
        {
            foreach(Collider2D collider in Physics2D.OverlapCircleAll(attackPoint.position, attackRange))
            {
                if(collider.CompareTag("Player"))
                {
                    Attack();
                }
            }
        }
        
    }

    void Attack()
    {
        if(canAttack)
        {
            return;
        }

        if(!isRanged)
        {
            if(enemyController.isBoss)
            {
                animator.SetBool("isAttacking", true);
                bool isPlayerRight = enemyController.playerIsEast;
                Debug.Log("is player right: " + isPlayerRight);
                // Set the Animator parameter
                animator.SetBool("isFacingRight", isPlayerRight);
            }
            else
            {
                animator.SetTrigger("Attack");
            }
            canAttack = true;
            StartCoroutine(DelayAttack());
        }
        else
        {
            delay = 1.0f;
            Instantiate(fireball, attackPoint.position, attackPoint.rotation);
            canAttack = true;
            StartCoroutine(DelayAttack());
        }    
    }

    public void ResetAttack()
    {
        animator.SetBool("isAttacking", false);
        Debug.Log("resetting attack");
    }

    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(delay);
        canAttack = false;
    }


    public void IsInRange()
    {
        if(enemyController.isBoss)
        {
             PolygonCollider2D attackCollider = GetComponent<PolygonCollider2D>();
            Collider2D[] hitColliders = new Collider2D[10]; // Adjust size based on expected number of hits
            ContactFilter2D filter = new ContactFilter2D().NoFilter();
            int colliderCount = attackCollider.OverlapCollider(filter, hitColliders);

            for (int i = 0; i < colliderCount; i++)
            {
                Collider2D collider = hitColliders[i];
                PlayerMovement playerMovement = collider.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.hit(damage, transform.parent.gameObject);
                    break; // If you want to attack only one player, keep this line. Otherwise, remove it.
                }
            }
        }
        else
        {
            foreach(Collider2D collider in Physics2D.OverlapCircleAll(attackPoint.position, attackRange))
            {
                PlayerMovement playerMovement = collider.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.hit(damage, transform.parent.gameObject);
                }
            }  
        }
        
    }
}
