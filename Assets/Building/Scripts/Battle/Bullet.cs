using System;
using UnityEngine;

public class Bullet: MonoBehaviour
{
    public float damage = 1;
    public float speed = 1;
    
    private Rigidbody2D rb2D;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction)
    {
        rb2D.AddForce(direction.normalized * speed);
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        var damageable = other.gameObject.GetComponentInParent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
