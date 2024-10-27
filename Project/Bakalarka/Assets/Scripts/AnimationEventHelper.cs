using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelper : MonoBehaviour
{
    public UnityEvent OnAttackPerformed;
    public EnemyAttack enemyAttack;

    public void TriggerAttack()
    {
        OnAttackPerformed?.Invoke();
    }

    public void ResetAttack()
    {
        enemyAttack.ResetAttack();
    }
}
