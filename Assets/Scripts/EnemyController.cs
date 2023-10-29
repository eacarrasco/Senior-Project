using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private float health = 100f;
    private float attackDamage = 4f;
    private bool canTakeDamage = true;
    private bool canMove = true;
    private bool isTakingDamage = false;
    private float knockback = 1f;
    private float lastDamage = Mathf.NegativeInfinity;
    private float invulnerableTimer = 1.5f;

    public Rigidbody2D rb;
    
    // This is called once when spawned
    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Taking damage
        if (isTakingDamage)
        {
            if (Time.time >= lastDamage + invulnerableTimer)
            {
                canTakeDamage = true;
                canMove = true;
                isTakingDamage = false;
            }
        }
    }

    public void takeDamage(float[] damageParameters)
    {
        if (canTakeDamage)
        {
            canTakeDamage = false;
            canMove = false;
            health -= damageParameters[0];
            if (health <= 0)
            {
                EnemyPool.Instance.AddToPool(gameObject);
            }
            isTakingDamage = true;
            lastDamage = Time.time;
            rb.velocity = new Vector2(damageParameters[1] - transform.position.x, damageParameters[2] - transform.position.y).normalized * -knockback;
        }
    }
}
