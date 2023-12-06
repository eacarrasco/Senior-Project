using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPaceController : MonoBehaviour
{
    private float maxHealth = 50f;
    private float health = 50f;
    private float attackDamage = 1f;
    private float lastAttack = Mathf.NegativeInfinity;
    private float attackCooldown = 0.2f;
    private float[] damageParameters = new float[3];
    private Collider2D objectHitByAttack;
    private bool canTakeDamage = true;
    private bool canMove = true;
    private bool isTakingDamage = false;
    private float knockback = 5f;
    private float lastDamage = Mathf.NegativeInfinity;
    private float invulnerableTimer = 0.2f;
    private float movespeed = 3f;
    private bool isFacingRight = true;
    private float damageWidth = 1.5f;
    private float damageHeight = 0.9f;
    private float groundCheckDistance = 0.5f;
    private float wallCheckDistance = 0.1f;
    private bool wallDetected;

    public Transform origin;
    public Rigidbody2D rb;
    public Animator anim;
    public GameObject alive;
    public GameObject deathChunkParticle;
    public GameObject deathBloodParticle;
    public LayerMask playerLayer;
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask platforms; 

    // This is called once when spawned
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFacingRight)
        {
            wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, platforms);
        }
        else
        {
            wallDetected = Physics2D.Raycast(wallCheck.position, -transform.right, wallCheckDistance, platforms);
        }
        if (canMove && (!Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, platforms) || wallDetected))
        {
            isFacingRight = !isFacingRight;
            alive.transform.Rotate(0, 180, 0);
        }

        if (canMove && Time.time >= lastAttack + attackCooldown)
        {
            objectHitByAttack = Physics2D.OverlapArea(new Vector2(origin.position.x - damageWidth / 2, origin.position.y - damageHeight / 2), new Vector2(origin.position.x + damageWidth / 2, origin.position.y + damageHeight / 2), playerLayer);

            if (objectHitByAttack)
            {
                lastAttack = Time.time;
                damageParameters[0] = attackDamage;
                damageParameters[1] = alive.transform.position.x;
                damageParameters[2] = alive.transform.position.y;
                objectHitByAttack.SendMessage("takeDamage", damageParameters);
            }
        }

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

        anim.SetBool("isTakingDamage", isTakingDamage);
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            if (isFacingRight)
            {
                rb.velocity = new Vector2(movespeed, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(-movespeed, rb.velocity.y);
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
                Instantiate(deathChunkParticle, alive.transform.position, deathChunkParticle.transform.rotation);
                Instantiate(deathBloodParticle, alive.transform.position, deathBloodParticle.transform.rotation);
                EnemyPool.Instance.AddToPool(gameObject);
            }
            isTakingDamage = true;
            lastDamage = Time.time;
            HitParticlePool.Instance.GetFromPool().GetComponent<Transform>().position = alive.transform.position;
            rb.velocity = new Vector2(damageParameters[1] - transform.position.x, damageParameters[2] - transform.position.y).normalized * -knockback;
        }
    }
}
