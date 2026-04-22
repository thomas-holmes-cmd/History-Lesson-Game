using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float baseKnockbackForce = 5f;
    private float movespeed;
    private Vector2 direction;
    private Player owner;

    public void InitializeProjectile(Vector2 shootdirection, float movespeed, Player shooter)
    {
        this.direction = shootdirection.normalized;
        this.movespeed = movespeed;
        this.owner = shooter;

        if (shootdirection.x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }

        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * movespeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player hitPlayer = collision.gameObject.GetComponent<Player>();

        if (hitPlayer != null && hitPlayer != owner)
        {
            Vector2 knockbackDirection = (hitPlayer.transform.position - transform.position).normalized;
            hitPlayer.TakeDamage(damage, knockbackDirection, baseKnockbackForce);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}