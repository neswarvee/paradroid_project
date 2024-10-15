using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string id;
    [ContextMenu("Generate guid for id")]
    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }
    public List<Transform> patrolPoints; // Array to hold the patrol points
    private int currentPointIndex = 0; // Index to keep track of the current point
    private Transform currentPoint;
    private Rigidbody2D rb;
    private Transform target;
    public float moveSpeed = 3;
    public float chaseRange = 3;

    public GameObject patrolPointPrefab;

    public float collisionOffset = 0.05f;

    public ContactFilter2D movementFilter;

    public AudioManager audioManager;

    public Vector2 velocity;

    public GameObject bulletPrefab;

    private int bulletSpeed = 5;

    public double fireRate;

    private double fireTimer = 0;

    public double shootDistance = 10;

    private int aimInnacuracy = 20;

    public TextMeshProUGUI textMeshPro;

    public List<String> enemyVoicelines;

    private double timeLastDiagolue = 0;

    public Vector2[] patrolPointLocations;

    private bool dying = false;

    private bool tased = false;

    private DataPersistenceManager dataPersistenceManager;
    

    void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        foreach(Vector2 patrolPointLocation in patrolPointLocations)
        {
            GameObject patrolPoint = Instantiate(patrolPointPrefab, patrolPointLocation, Quaternion.identity);
            patrolPoints.Add(patrolPoint.transform);
        }
        currentPoint = patrolPoints[currentPointIndex];
        target = GameObject.FindGameObjectWithTag("Player").transform;

        DataPersistenceManager dataPersistenceManagerObject = FindObjectOfType<DataPersistenceManager>();
        dataPersistenceManager = dataPersistenceManagerObject.GetComponent<DataPersistenceManager>();

    }

    public void LoadData(GameData data)
    {

        // Debug.Log(PlayerPrefs.GetInt("isResetScene"));

        // Check if the enemyPositions dictionary contains the ID of this robot
        if (data.enemyPositions.ContainsKey(id))
        { 

            // Get the position from the dictionary using the ID
            Vector3 position;
            if (data.enemyPositions.TryGetValue(id, out position))
            {
                // Set the robot's position to the retrieved position
                transform.position = position;

                // Do the same for the current point index
                data.enemyCurrentPointIndexes.TryGetValue(id, out currentPointIndex);
                currentPoint = patrolPoints[currentPointIndex];
                // Do the same for the health
                data.enemyHealths.TryGetValue(id, out float health);
                HealthSystem healthSystem = GetComponent<HealthSystem>();
                healthSystem.SetHealth(health);
            }
            else
            {
                Debug.LogError("Failed to retrieve position for robot with ID: " + id);
            }
        }
        else
        {

            // Debug.LogError("Robot with ID: " + id + " not found in enemyPositions dictionary.");        
            if (PlayerPrefs.GetInt("isResetScene") == 0)
            {
                Destroy(gameObject);
            }
        }
    }
    public void SaveData(ref GameData data)
    {
        // Save the position
        Vector3 position = transform.position;
        if (data.enemyPositions.ContainsKey(id))
        {
            // Update the position for the existing ID
            data.enemyPositions[id] = position;
        }
        else
        {
            // Add a new entry for this robot's ID and position
            data.enemyPositions.Add(id, position);
        }

        // Save the current point index
        if (data.enemyCurrentPointIndexes.ContainsKey(id))
        {
            data.enemyCurrentPointIndexes[id] = currentPointIndex;
        }
        else
        {
            data.enemyCurrentPointIndexes.Add(id, currentPointIndex);
        }

        // Save the health
        HealthSystem healthSystem = GetComponent<HealthSystem>();
        float health = healthSystem.GetHealth();
        if (data.enemyHealths.ContainsKey(id))
        {
            data.enemyHealths[id] = health;
        }
        else
        {
            data.enemyHealths.Add(id, health);
        }
    
    }

    public bool GetDying()
    {
        // Me away from here I'm
        return dying;
    }

    public void SetDying(bool d)
    {
        dying = d;
    }

    public bool GetTased()
    {
        return tased;
    }

    public void SetTased(bool t)
    {
        tased = t;
    }


    private void Awake()
    {
        // Initialise the audio manager for enemy related sound effects
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    /* TryEnemyMovement, Attempt to move the enemy in the chosen direction by using Trymove, gliding when incident on walls at an angle is important
        So TryMove needs to be called twice unlike for bullet movement*/
    private void TryEnemyMovement()
    {
        if (velocity == Vector2.zero) {
            return;
        }

        Vector2 position = rb.position;
        
        // Try moving
        velocity = velocity.normalized;

        position = TryMove(position, new Vector2(velocity.x, 0));
        position = TryMove(position, new Vector2(0, velocity.y));

        rb.MovePosition(position);
    }

    /* TryMove, Attempt to move in the given direction, collide with anything in layers collidable with bullet (everything)
        Apart from apart from my own bullets which arer detected by the AllAreMyBullets function*/
    private Vector2 TryMove(Vector2 position, Vector2 direction)
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        int count = rb.Cast(
            direction,
            movementFilter,
            hits,
            moveSpeed * Time.fixedDeltaTime + collisionOffset);

        // Check if there are any collisions
        if (count > 0 && !AllAreMyBullets(hits))
        {
            // Play the sound effect for bumping into something
            //audioManager.PlaySFX(audioManager.bump);
        }

        if (count == 0 || AllAreMyBullets(hits))
        {
            position += direction * moveSpeed * Time.fixedDeltaTime;
        }
        return position;
    }

    /* AllAreMyBullets, takes a list of RayCastHit2Ds and checks whether all of them are bullets that were generated by this
        object, if they are then the enemy should be able to move anyway, this allows movement of the enemy through a cloud
        of potentially multiple of its own bullets as they are being shot instead of being stuck on them temporarily imparing
        movement. Returns boolean whether all of these objects were my own bullets.
    */
    private bool AllAreMyBullets(List<RaycastHit2D> hits)
    {
        foreach (RaycastHit2D hit in hits)
        {
            if (!hit.collider.gameObject.CompareTag("Bullet"))
            {
                return false;
            }
            else
            {
                BulletScript bulletScript = hit.collider.gameObject.GetComponent<BulletScript>();
                if (bulletScript != null && bulletScript.owner != gameObject)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Update, regular updates for the enemy that need to occur every frame
    void Update()
    {
        if (!dying && !tased)
        {
            // Update the firetimer so it accurately represents how long until the next bullet can be shot
            if (fireTimer > 0)
            {
                fireTimer -= Time.deltaTime;
                
                if (fireTimer < 0)
                {
                    fireTimer = 0;
                }
            }

            //Enemy Shooting Setup

            Vector2 position = rb.position;
            Vector2 playerPos = target.position;

            float distance = Vector2.Distance(position, playerPos);

            // Find the angle to the player to shoot it
            double angle = Math.Atan2(position.y - playerPos.y, position.x - playerPos.x) * (180.0 / Math.PI) + 180;

            // Add a bit of Aim Innacuracy to the enemy so it doesnt have perfect aimbot (10 degrees either side at random)
            double angleWithAimInnacuracy = angle + UnityEngine.Random.Range(-aimInnacuracy/2, aimInnacuracy/2);

            // Snap the angle to the nearest 40 degrees to provide a retro feel with shooting
            //int snappedAngle = (int) Math.Round(angleWithAimInnacuracy / 45) * 45;

            int intAngle = (int) angleWithAimInnacuracy;

            /* Shooting check - Check whether enough time has elapsed since the last shot for the "gun" to allow me to shoot again
                This is how a fire rate is implemented preventing a bullet being shot every frame*/
            if (fireTimer == 0 && distance < shootDistance)
            {
                audioManager.PlaySFX(audioManager.shoot);

                // Create a bullet and setup all of the neccessary properties in the bullet object using its setter functions
                GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(0f, 0f, intAngle));
                BulletScript bulletScript = bullet.GetComponent<BulletScript>();
                bulletScript.SetPosition(position.x, position.y);
                bulletScript.SetSpeed(bulletSpeed);
                bulletScript.SetDirection(intAngle);
                bulletScript.SetOwner(gameObject);

                /* Set the fire timer to the reciprocal of the fire rate so the correct amount of time must elapse before the next
                    bullet can be fired */
                fireTimer = 1 / fireRate;
            }


            /* Matthew's Enemy Movement logic implementing both patrolling and following behaviours (AI)
            */

            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            
            // Check if player is close to the enemy
            if (distanceToPlayer <= chaseRange)
            {   
                if (Time.timeAsDouble - timeLastDiagolue >= 4)
                    {
                        timeLastDiagolue = Time.timeAsDouble;
                        int index = GenerateRandomNum(enemyVoicelines);
                        textMeshPro.text = enemyVoicelines[index];
                    }

                    Vector2 playerDirection = (target.position - transform.position).normalized;
                    velocity = playerDirection * moveSpeed;
            }
            else
            {
                // Move towards the current point
                Vector2 patrolDirection = (currentPoint.position - transform.position).normalized;
                velocity = patrolDirection * moveSpeed;

                // Check if reached the current point, then switch to the next point
                if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
                {
                    currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count; // Move to the next point in the array
                    currentPoint = patrolPoints[currentPointIndex];
                }
            }

            if (Time.timeAsDouble - timeLastDiagolue >= 3)
            {
                textMeshPro.text = null;
            }

            // Call TryEnemyMovement to attempt to move in the chosen direction
            TryEnemyMovement();

        }
    }

        private int GenerateRandomNum(List<string> listOfStrings)
        {
            System.Random rand = new System.Random();

            int listIndex = rand.Next(0, listOfStrings.Count);

            return listIndex;
        }
}
