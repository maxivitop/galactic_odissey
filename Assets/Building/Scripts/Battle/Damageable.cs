using System;
using UnityEngine;
using UnityEngine.Events;

public class Damageable: MonoBehaviour
{
    public float maxHp;
    public float hp;
    public UnityEvent onDeath;
    public bool isAlive = true;
    
    private void Awake()
    {
        hp = maxHp;
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (isAlive && hp < 0)
        {
            onDeath.Invoke();
            isAlive = false;
        }
    }
}
