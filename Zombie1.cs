using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie1 : MonoBehaviour
{
    private Rigidbody2D physic;

    public int maxHealth = 100;
    int currentHealth;

    public Transform player;

    public float speed;
    public float agroDistance;

    public Animator animator;

    public bool hunting;

    void Start()
    {
        physic = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if(distToPlayer < agroDistance && hunting == true)
        {
            StartHunting();
        }
        else
        {
            StopHunting();
        }
    }

    void StartHunting()
    {
        animator.SetFloat("moveX", Mathf.Abs(player.position.x));

        hunting = true;

        if(player.position.x < transform.position.x)
        {
            physic.velocity = new Vector2(-speed, 0);
            transform.localScale = new Vector2(-1, 1);
        }
        else if (player.position.x > transform.position.x)
        {
            physic.velocity = new Vector2(speed, 0);
            transform.localScale = new Vector2(1, 1);
        }
    }

    void StopHunting()

    {
        physic.velocity = new Vector2(0, 0);
        animator.Play("idle");

        hunting = false;
    }

    private float damage = 0.25f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        { 
            physic.velocity = new Vector2(-1, 0); 
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name == "CHARACTER")
        {
            other.GetComponent<PlayerMove>().TakeDamage(damage);
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        animator.SetTrigger("damage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        Debug.Log("Enemy Died!");

        animator.SetBool("IsDead", true);
        
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
