using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth;
    public float animationTime;
    private float currentHealth;
    Animator animator;

    private float unTransformAnimationTimer = 0;

    private ParadroidController paradroidController;

    private bool unTransforming;
    private bool hasDied;

    // Start is called before the first frame update
    void Start()
    {
        // If paradroid, set maxHealth to 500/difficulty
        if (gameObject.CompareTag("Player"))
        {
            maxHealth = 500 / PlayerPrefs.GetInt("Difficulty");
        }

        hasDied = false;
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        paradroidController = gameObject.GetComponent<ParadroidController>();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        animator.SetFloat("Speed", maxHealth/(currentHealth+0.0001f));
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public void SetHealth(float health)
    {
        currentHealth = health;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetMaxHealth(float m)
    {
        maxHealth = m;
    }

    void Die()
    {
        bool noDeath = false;
        if (gameObject.CompareTag("Player"))
        {
            if (paradroidController.GetTakenOver())
            {
                noDeath = true;
                animator.Play("Death");
                unTransforming = true;
            }
            else {
                PlayerPrefs.SetInt("Won", 0);
                UnityEngine.SceneManagement.SceneManager.LoadScene("YouWon");
            }
        }

        if (!noDeath)
        {
            if (gameObject.CompareTag("Enemy"))
            {
                gameObject.GetComponent<Enemy>().SetDying(true);
                if (hasDied == false)
                {
                    GameObject hudControllerObject = GameObject.FindGameObjectWithTag("HUDController");
                    //Auxiliary 85
                    //Legionary 150
                    //Centurion 300
                    //Imperator 500
                    if (maxHealth == 85)
                    {
                       hudControllerObject.SendMessage("UpdateScore", 100); 
                       hasDied = true;
                    }
                    if (maxHealth == 150)
                    {
                       hudControllerObject.SendMessage("UpdateScore", 200); 
                       hasDied = true;
                    }
                    if (maxHealth == 300)
                    {
                       hudControllerObject.SendMessage("UpdateScore", 300);
                       hasDied = true; 
                    }
                    if (maxHealth == 500)
                    {
                       hudControllerObject.SendMessage("UpdateScore", 400);
                       hasDied = true;
                    }
                }
            }
            animator.Play("Death");
            Destroy(gameObject, animationTime);
        }
    }

    void Update()
    {
        if (unTransformAnimationTimer > 1)
        {
            paradroidController.UnTransformToParasite();
            unTransformAnimationTimer = 0;
            unTransforming = false;

        }
        else if (unTransforming)
        {
            unTransformAnimationTimer += Time.deltaTime;
        }

        // Fix health if <0
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
