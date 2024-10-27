/**
 * Autor: David Zahálka
 *
 * Hledání a následování hráče nepřítelem
 * 
 * Skript řídí hledání a následování hráče nepřítelem. Používá data o aktuálních cílech a na základě
 * toho upravuje směr pohybu nepřítele. Tento systém umožňuje nepříteli pronásledovat hráče
 * a reagovat na změny v polohách cílů.
 */


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySeekSteering : EnemySteering
{
    [SerializeField]
    private float targetReachedThreshold = 0.5f;
    [SerializeField]
    bool reachedLastTarget = true;

    private Vector2 targetPositionCached;
    private float[] tmpInterest;

    // upravení směru pohybu na základě polohy cíle
    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, EnemyData enemyData)
    {
        if(reachedLastTarget)
        {
            if(enemyData.targets == null || enemyData.targets.Count <= 0)
            {
                enemyData.currentTarget = null;
                return(danger, interest);   // pokud nejsou cíle, nepřítel nemá směr
            }
            else
            {
                // výběr nejbližšího cíle
                reachedLastTarget = false;
                enemyData.currentTarget = enemyData.targets.OrderBy(target => Vector2.Distance(target.position, transform.position)).FirstOrDefault();
            }
        }

        // aktualizace pozice cíle
        if(enemyData.currentTarget != null && enemyData.targets != null && enemyData.targets.Contains(enemyData.currentTarget))
        {
            targetPositionCached = enemyData.currentTarget.position;
        }

        // kontrola jestli byl cíl dosáhnut
        if(Vector2.Distance(transform.position, targetPositionCached) < targetReachedThreshold)
        {
            reachedLastTarget = true;
            enemyData.currentTarget = null;
            return(danger, interest);
        }

        // aktualizace cíle pokud je blíže (například hráče znova zaznamená na pozici bližší než poslední viděná pozice)
        Vector2 dir = (targetPositionCached - (Vector2)transform.position);
        for(int i = 0; i < interest.Length; i++)
        {
            float result = Vector2.Dot(dir.normalized, Directions.eightDirections[i]);
            if(result > 0)
            {
                float value = result;
                if(value > interest[i])
                {
                    interest[i] = value;
                }
            }
        }
        tmpInterest = interest;
        return(danger, interest);
    }
}