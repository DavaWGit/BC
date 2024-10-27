/**
 * Autor: David Zahálka
 *
 * Umělá inteligence nepřítele
 * 
 * Skript určuje směr pohybu nepřítele na základě kombinace chování, která určují jak
 * zájem tak nebezpečí v prostředí.
 *
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    Vector2 final = Vector2.zero;

    public Vector2 GetDirection(List<EnemySteering> behaviours, EnemyData enemyData)
    {
        // pole vektorů všech osmi směrů
        float[] danger = new float[8];
        float[] interest = new float[8];

        foreach(var behaviour in behaviours)
        {
            (danger, interest) = behaviour.GetSteering(danger, interest, enemyData);
        }

        // odečtení hodnot nebezpečí od cílů
        for(int i = 0; i < 8; i++)
        {
            interest[i] = Mathf.Clamp01(interest[i] - danger[i]);
        }

        // výpočet finálního směru pohybu
        Vector2 direction = Vector2.zero;
        for(int i = 0; i < 8; i++)
        {
            direction += Directions.eightDirections[i] * interest[i];
        }
        direction.Normalize();

        final = direction;

        return final;
    }
}
