using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemySteering : MonoBehaviour
{
    public abstract (float[] danger, float[] interest) GetSteering(float[] danger, float[] interest, EnemyData enemyData);
}
