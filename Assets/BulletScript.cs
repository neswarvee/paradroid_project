using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    int direction = 0;
    double speed = 5;

    double x = 0;

    double y = 0;
    Rigidbody2D rb;

    public GameObject owner = null;

    public ContactFilter2D movementFilter;

    public float collisionOffset = 0.05f;

    public void SetSpeed(double speed)
    {
        this.speed = speed;
    }

    public void SetDirection(int direction)
    {
        this.direction = direction;
    }

    public void SetPosition(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void FixedUpdate()
    {
        TryBulletMovement();
    }

    /* TryBulletMovement, Attempt to move the bullet in the forward direction by using Trymove, gliding when incident on walls at an angle is unimportant
        So TryMove only needs to be called once unlike for player and enemy movement*/
    private void TryBulletMovement()
    {

        if (speed == 0)
        {
            return;
        }
        else
        {
            Vector2 position = rb.position;

             float directionRadians = direction * Mathf.Deg2Rad;
             Vector2 directionVector = new Vector2(Mathf.Cos(directionRadians), Mathf.Sin(directionRadians));

            position = TryMove(position, directionVector);
            rb.MovePosition(position);
        }
    }


    /* TryMove, Attempt to move in the given direction, collide with anything in layers collidable with bullet (everything)
        Apart from player and enemy objects which are exclided by this script so it doesnt collide with the foot hitbox
        Collisions with the player are detected in OnTriggerEnter2D*/
    private Vector2 TryMove(Vector2 position, Vector2 direction)
    {

        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        rb.Cast(
                direction,
                movementFilter,
                hits,
                (float) speed * Time.fixedDeltaTime + collisionOffset);

        int i = 0;
        while (i < hits.Count)
        {
            if (hits[i].collider.gameObject.CompareTag("Player") || hits[i].collider.gameObject.CompareTag("Enemy"))
            {
                hits.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }

        int count = hits.Count;

        if (count == 0) {
            position += direction * (float) speed * Time.fixedDeltaTime;
        }
        else {
            Destroy(gameObject, 0f);
        }
        return position;
    }


    /* OnTriggerEnter2D, detech collisions with the player and enemy as long as it isn't this bullet's owner using
        the BoxCollider2D of the player or enemy's child object which has isTrigger active*/
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BulletHitbox"))
        {
            GameObject characterShot = other.transform.parent.gameObject;
            if (characterShot != owner)
            {
                HealthSystem healthSystem = characterShot.GetComponent<HealthSystem>();

                if (healthSystem != null)
                {
                    // If healthSystem belongs to a player, deal 30 damage. Otherwise deal 30/difficulty damage
                    if (characterShot.CompareTag("Player"))
                    {
                        healthSystem.TakeDamage(30f);
                    }
                    else
                    {
                        if (!PlayerPrefs.HasKey("Difficulty"))
                        {
                            PlayerPrefs.SetInt("Difficulty", 1);
                            Debug.LogWarning("Difficulty not set, defaulting to 1");
                        }
                        healthSystem.TakeDamage(30f / PlayerPrefs.GetInt("Difficulty"));
                    }
                }
                Destroy(gameObject);
            }
        }
    }

}
