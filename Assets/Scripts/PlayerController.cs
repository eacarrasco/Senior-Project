using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Stats
    private int health = 4;
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
    private bool isJumping;

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
    private int dashCharges = 3; //Change to 0
    private int maxDashCharges = 3;
    private int dashAttackMultiplier;
    private int normalDashAttackMultiplier = 3;
    private int maxDashAttackMultiplier = 20;
    [SerializeField]
    private float dashAttackRadius = 1.5f;
    private Vector3 lastImagePos;
    private float distanceBetweenImages = 0.1f;

    //Attacking
    [SerializeField]
    private float attackRange = 3f;
    [SerializeField]
    private float attackRadius = 0.5f;
    private float lastAttack = Mathf.NegativeInfinity;
    private float attackCooldown = 0.2f;
    private float attackRecoil = 5;

    //Taking damage
    private float knockback = 1f;
    private float invulnerableTimer = 1.5f;
    private float lastDamage = Mathf.NegativeInfinity;

    //References to other components
    public Rigidbody2D rb;
    public Transform feet;
    public Transform playerOrigin;
    public LayerMask platforms;
    public LayerMask enemy;
    public Animator anim;

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
        Collider2D[] objectsHitByDash;
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
                foreach (Collider2D collider in objectsHitByDash)
                {
                    collider.transform.parent.SendMessage("Damage", attackDamage * dashAttackMultiplier);
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
                dashCharges = 1; //Change to 0
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }

        //Attacking
        Collider2D[] objectsHitByAttack;
        if (Input.GetKeyDown(KeyCode.X))
        {
            //Check if can attack
            if (canMove && Time.time >= lastAttack + attackCooldown)
            {
                lastAttack = Time.time;

                //Where to attack based on inputs
                if (vertical != 0)
                {
                    objectsHitByAttack = Physics2D.OverlapCircleAll(new Vector2(playerOrigin.position.x, (playerOrigin.position.y + attackRange) * vertical), attackRadius, enemy);
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
                    if (dashCharges < maxDashCharges)
                    {
                        dashCharges++;
                    }
                    if (horizontal == 0 && vertical == 0)
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
                    else
                    {
                        rb.velocity = new Vector2(-attackRecoil * horizontal, -attackRecoil * vertical);
                    }
                    foreach (Collider2D collider in objectsHitByAttack)
                    {
                        collider.transform.parent.SendMessage("Damage", attackDamage);
                    }
                }
            }
        }

        //Update animations
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isDashing", isDashing);
        anim.SetInteger("dashCharges", dashCharges);
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

    public bool takeDamage()
    {
        if (canTakeDamage)
        {
            return true;
        }
        return false;
    }
}
