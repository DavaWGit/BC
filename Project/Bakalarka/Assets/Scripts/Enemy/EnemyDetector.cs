using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyDetector : MonoBehaviour
{
    public abstract void Detect(EnemyData enemyData);
}
