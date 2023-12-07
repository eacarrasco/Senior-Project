using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Transform = UnityEngine.Transform;
using Image = UnityEngine.UI.Image;

public class PlayerController : MonoBehaviour
{
    //HUD
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public Image[] charges;
    public Sprite fullCharge;
    public Sprite emptyCharge;

    //Particles
    public GameObject deathChunkParticle;
    public GameObject deathBloodParticle;

    //Stats
    private float health = 4f;
    private float maxHealth = 4f;
    private float attackDamage = 4f;

    //Inputs
    [SerializeField]
    private float horizontal;
    [SerializeField]
    private float vertical;

    //Player State
    private bool canMove = true;
    private bool isFacingRight = true;
    private bool isDashing = false;
    private bool isGrounded;
    [SerializeField]
    private float groundCheckRadius = 0.7f;
    private bool canTakeDamage = true;
    private bool isWalking;
    private bool isAttacking = false;
    private bool isTakingDamage = false;
    private bool isRecoveringFromDamage = false;

    //Movement
    private float moveSpeed = 15;

    //Jumping
    private int jumps = 0;
    private int maxJumps = 2;
    private float jumpPower = 20;

    //Dashing
    private float dashHorizontal;
    private float dashVertical;
    private float dashSpeed = 40;
    private float dashTime = 0.2f;
    private float currentDashTime;
    private int dashCharges = 0;
    private int maxDashCharges = 3;
    private int dashAttackMultiplier;
    private int normalDashAttackMultiplier = 3;
    private int maxDashAttackMultiplier = 20;
    [SerializeField]
    private float dashAttackRadius = 1.5f;
    private Vector3 lastImagePos;
    private float distanceBetweenImages = 0.1f;
    private Collider2D[] objectsHitByDash;

    //Attacking
    private float attackVertical;
    [SerializeField]
    private float attackRange = 0.2f;
    [SerializeField]
    private float attackRadius = 0.5f;
    private float lastAttack = Mathf.NegativeInfinity;
    private float attackCooldown = 0.2f;
    private float attackRecoil = 15f;
    private float[] damageParameters = new float[3];
    private Collider2D[] objectsHitByAttack;
    private bool contact = false;

    //Taking damage
    private float knockback = 15f;
    private float damageTimer = 0.3f;
    private float invulnerableTimer = 1f;
    private float lastDamage = Mathf.NegativeInfinity;

    //References to other components
    public Rigidbody2D rb;
    public Transform feet;
    public Transform playerOrigin;
    public LayerMask platforms;
    public LayerMask enemy;
    public Animator anim;
    public GameObject gameOverPanel;

    private void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        //Inputs
        if (canMove) 
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }

        //Flip model
        if (canMove && !isFacingRight && horizontal > 0)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0, 180, 0);
        }
        else if (canMove && isFacingRight && horizontal < 0)
        {
            isFacingRight = !isFacingRight;
            transform.Rotate(0, 180, 0);
        }

        //Walking
        if (rb.velocity.x != 0 && isGrounded)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        //Jumping
        //Reset jumps when grounded
        if (isGrounded && rb.velocity.y <= 0)
        {
            jumps = 0;
        }
        //If in air, can only jump once
        else if (!isGrounded && jumps == 0)
        {
            jumps = 1;
        }
        if (canMove && jumps < maxJumps && Input.GetKeyDown(KeyCode.Z))
        {
            jumps++;
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        }


        //Dashing
        if (Input.GetKeyDown(KeyCode.C)) 
        {
            if (canMove && dashCharges > 0)
            {
                isDashing = true;
                canTakeDamage = false;
                canMove = false;
                currentDashTime = dashTime;
                if (horizontal == 0 && vertical == 0)
                {
                    if (isFacingRight)
                    {
                        dashHorizontal = 1;
                    }
                    else
                    {
                        dashHorizontal = -1;
                    }
                }
                else
                {
                    dashHorizontal = horizontal;
                }
                dashVertical = vertical;

                //Change damage of dash based on dashCharges
                if (dashCharges == maxDashCharges)
                {
                    dashAttackMultiplier = maxDashAttackMultiplier;
                }
                else
                {
                    dashAttackMultiplier = normalDashAttackMultiplier;
                }

                //Get After Image from pool
                PlayerAfterImagePool.Instance.GetFromPool();
                lastImagePos = transform.position;
            }
        }
        //Continue dashing if already initiated
        if (isDashing)
        {
            if (currentDashTime > 0)
            {
                //Decrease dash timer
                currentDashTime -= Time.deltaTime;
                rb.velocity = new Vector2(dashSpeed * dashHorizontal, dashSpeed * dashVertical);

                //Cause enemies in the way of dash to take damage
                objectsHitByDash = Physics2D.OverlapCircleAll(playerOrigin.position, dashAttackRadius, enemy);
                if (objectsHitByDash.Length > 0)
                {
                    if (health < maxHealth)
                    {
                        hearts[(int)health].sprite = fullHeart;
                        health += 1;
                    }
                }

                foreach (Collider2D collider in objectsHitByDash)
                {
                    damageParameters[0] = attackDamage * dashAttackMultiplier;
                    damageParameters[1] = transform.position.x;
                    damageParameters[2] = transform.position.y;
                    collider.transform.parent.SendMessage("takeDamage", damageParameters);
                }

                //Dash After Image
                if (Vector3.Distance(transform.position, lastImagePos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImagePos = transform.position;
                }
            }
            else
            {
                //End dash
                canMove = true;
                isDashing = false;
                canTakeDamage = true;
                dashCharges = 0;
                charges[0].sprite = emptyCharge;
                charges[1].sprite = emptyCharge;
                charges[2].sprite = emptyCharge;
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }

        //Attacking
        if (Input.GetKeyDown(KeyCode.X))
        {
            //Check if can attack
            if (canMove && Time.time >= lastAttack + attackCooldown)
            {
                if (vertical == -1 && isGrounded)
                {
                    attackVertical = 0;
                }
                else
                {
                    attackVertical = vertical;
                }
                
                isAttacking = true;
            }
        }

        //Taking damage
        if (isTakingDamage)
        {
            if (Time.time >= lastDamage + damageTimer)
            {
                canMove = true;
                isTakingDamage = false;
            }
        }
        if (isRecoveringFromDamage)
        {
            if (Time.time >= lastDamage + invulnerableTimer)
            {
                if (!isDashing)
                {
                    canTakeDamage = true;
                }
                isRecoveringFromDamage = false;
            }
        }

        //Update animations
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isDashing", isDashing);
        anim.SetInteger("dashCharges", dashCharges);
        anim.SetBool("isTakingDamage", isTakingDamage);
        anim.SetBool("isAttacking", isAttacking);
        anim.SetFloat("attackVertical", attackVertical);
    }

    private void FixedUpdate()
    {
        //Move horizontal
        if (canMove)
        {
            rb.velocity = new Vector2(moveSpeed * horizontal, rb.velocity.y);
        }

        //Check if grounded
        isGrounded = Physics2D.OverlapCircle(feet.position, groundCheckRadius, platforms);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(new Vector2((playerOrigin.position.x + attackRange) * horizontal, (playerOrigin.position.y + attackRange) * vertical), attackRadius);
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
                Instantiate(deathChunkParticle, transform.position, deathChunkParticle.transform.rotation);
                Instantiate(deathBloodParticle, transform.position, deathBloodParticle.transform.rotation);
                gameOverPanel.SetActive(true);
                gameObject.SetActive(false);
            }
            hearts[(int)health].sprite = emptyHeart;
            isTakingDamage = true;
            isRecoveringFromDamage = true;
            lastDamage = Time.time;
            HitParticlePool.Instance.GetFromPool().GetComponent<Transform>().position = transform.position;
            if (damageParameters[1] > transform.position.x)
            {
                rb.velocity = new Vector2(-knockback, knockback);
            }
            else
            {
                rb.velocity = new Vector2(knockback, knockback);
            }
        }
    }

    public void startAttack()
    {
        canMove = false;
        if (isGrounded)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        attack();
    }

    public void attack()
    {
        //Where to attack based on inputs
        if (attackVertical != 0)
        {
            objectsHitByAttack = Physics2D.OverlapCircleAll(new Vector2(playerOrigin.position.x, playerOrigin.position.y + attackRange * attackVertical), attackRadius, enemy);
        }
        else
        {
            if (isFacingRight)
            {
                objectsHitByAttack = Physics2D.OverlapCircleAll(new Vector2(playerOrigin.position.x + attackRange, playerOrigin.position.y), attackRadius, enemy);
            }
            else
            {
                objectsHitByAttack = Physics2D.OverlapCircleAll(new Vector2(playerOrigin.position.x - attackRange, playerOrigin.position.y), attackRadius, enemy);
            }
        }

        //If attack hit something, increment dash charges, apply attack recoil, and send a message for enemies to take damage
        if (objectsHitByAttack.Length > 0)
        {
            if (!contact && dashCharges < maxDashCharges)
            {
                charges[dashCharges].sprite = fullCharge;
                dashCharges++;
                contact = true;
            }
            if (attackVertical != 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -attackRecoil * attackVertical);
            }
            else
            {
                if (isFacingRight)
                {
                    rb.velocity = new Vector2(-attackRecoil, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(attackRecoil, rb.velocity.y);
                }
            }
            foreach (Collider2D collider in objectsHitByAttack)
            {
                damageParameters[0] = attackDamage;
                damageParameters[1] = transform.position.x;
                damageParameters[2] = transform.position.y;
                collider.transform.parent.SendMessage("takeDamage", damageParameters);
            }
        }
    }

    public void endAttack()
    {
        attack();
        isAttacking = false;
        canMove = true;
        lastAttack = Time.time;
        contact = false;
    }
}
