/**
 * Autor: David Zahálka
 *
 * Detektor hráče nepřítele
 * 
 * Tento skript slouží k detekci hráče v blízkosti nepřítele. Používá kruhovou oblast pro zjištění,
 * zda se hráč nachází v dosahu. V případě detekce hráče skript ověřuje, zda mezi hráčem a nepřítelem
 * neexistuje překážka.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlayerDetector : EnemyDetector
{
    [SerializeField]
    private float targetDetectionRange = 5;
    [SerializeField]
    private LayerMask obstaclesLayerMask, playerLayerMask;
    [SerializeField]
    private List<Transform> scannedTargets;
    
    public override void Detect(EnemyData enemyData)
    {
        // pokus nalezení hráče
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, targetDetectionRange, playerLayerMask);

        // pokud byl hráč detekován, ověřuje se, že mezi ním a nepřítelem není žádná překážka
        if(playerCollider != null)
        {
            Vector2 dir = (playerCollider.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, targetDetectionRange, obstaclesLayerMask);

            if(hit.collider != null && (playerLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                // pokud není překážka je přidán do listu cílů
                scannedTargets = new List<Transform>()
                {
                    playerCollider.transform
                };
            }
            else
            {
                scannedTargets = null; // vypráždnění listu pokud existuje překážka
            }
        }
        else
        {
            scannedTargets = null;  // vyprázdnění listu pokud je hráč mimo dosah
        }
        enemyData.targets = scannedTargets;
    }

}
