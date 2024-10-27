/**
 * Autor: David Zahálka
 *
 * Řízení směru nepřítele na základě překážek
 * 
 * Skript poskytuje logiku pro úpravu směrování nepřítele založeného na blízkých překážkách.
 * Vypočítává sílu a směr zájmu pro každou z osmi možných směrů pohybu, čímž pomáhá nepříteli
 * vyhnout se kolizím.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObstacleSteering : EnemySteering
{
    [SerializeField]
    private float radius = 2f, enemyColliderSize = 0.6f;
    [SerializeField]
    float[] dangersResult = null;

    // výpočet vlivu překážek na směr pohybu
    public override (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, EnemyData enemyData)
    {
        // průchod všemi překážkami
        foreach(Collider2D obstacleCollider in enemyData.obstacles)
        {
            Vector2 dir = obstacleCollider.ClosestPoint(transform.position) - (Vector2)transform.position;
            float dist = dir.magnitude;

            // váha na základě vzdálenosti od překážky
            float weight;
            if(dist <= enemyColliderSize)
            {
                weight = 1;
            }
            else
            {
                weight = (radius - dist) / radius;
            }

            dir = dir.normalized;

            // výpočet vlivu překážky na všech osm směrů pohybu
            for(int i = 0; i < Directions.eightDirections.Count; i++)
            {
                float result = Vector2.Dot(dir, Directions.eightDirections[i]);

                float value = result * weight;

                if(value > danger[i])
                {
                    danger[i] = value;
                }
            }
        }
        dangersResult = danger;
        return (danger, interest);
    }
}

// osm směrů pohybu
public static class Directions
{
    public static List<Vector2> eightDirections = new List<Vector2>
    {
        new Vector2(0,1).normalized,    // N
        new Vector2(1,0).normalized,    // E
        new Vector2(1,1).normalized,    // NE
        new Vector2(0,-1).normalized,   // S
        new Vector2(1,-1).normalized,   // SE
        new Vector2(-1,-1).normalized,  // SW
        new Vector2(-1,1).normalized,   // NW
        new Vector2(-1,0).normalized    // W
    };
}
