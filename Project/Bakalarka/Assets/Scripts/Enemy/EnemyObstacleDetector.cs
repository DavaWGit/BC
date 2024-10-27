/**
 * Autor: David Zahálka
 *
 * Detektor překážek nepřítele
 * 
 * Tento skript se používá k detekci překážek v okolí nepřítele. Využívá fyzikální kolize k určení,
 * které objekty se nacházejí v detekčním rozsahu. Zjištěné překážky jsou následně
 * předány do dat nepřítele pro další zpracování jinými komponentami AI systému.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObstacleDetector : EnemyDetector
{
    [SerializeField]
    private float detectionRadius = 2;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    Collider2D[] colliders;

    public override void Detect(EnemyData enemyData)
    {
        colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, layerMask);
        enemyData.obstacles = colliders;
    }

}
