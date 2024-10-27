/**
 * Autor: David Zahálka
 *
 * Ovládání projektilů
 * 
 * Řízení pohybu a interakce projektilů vytvořených nepřáteli ve hře.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireball : MonoBehaviour
{
    public float speed = 5.5f;
    private Vector3 direction;
    public int damage;
    public GameObject enemy;
    // Start is called before the first frame update
    void Start()
    {
        direction = PlayerMovement.instance.transform.position - transform.position;
        direction.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {     
        if(other.tag == "Player")
        {
            other.GetComponent<PlayerMovement>().hit(damage, enemy);
        }
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
