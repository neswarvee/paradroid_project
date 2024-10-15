using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParadroidController : MonoBehaviour, IDataPersistence
{   
    public AudioManager audioManager;
    public float moveSpeed = 1f;

    public float collisionOffset = 0.05f;

    public ContactFilter2D movementFilter;

    public GameObject bulletPrefab;

    private int bulletSpeed = 5;

    private double fireRate = 5;

    private double fireTimer = 0;

    private  GameObject tasedEnemy;

    private double taserTime = 1;
    private double taserTimer = 0;

    private bool tasingEnemy = false;

    public GameObject taserPrefab;

    private GameObject taserInstance;

    private float taserSizeConstant = 100;

    private float timeSinceTakeOver = 0.1f;

    private int tickNumber = 0;

    private bool takenOver = false;

    private HealthSystem healthSystem;

    private float tickDamage = 0;

    private float timeElapsedThisRound = 0;

    public TextMeshProUGUI timeElapsedText;


    Vector2 movementInput;
    Rigidbody2D rb;

    private SpriteRenderer parasiteRenderer;
    private Animator parasiteAnimator;

    private float parasiteHealth = 500;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthSystem = gameObject.GetComponent<HealthSystem>();
        StoreParasiteSpriteInformation();

        timeElapsedText.SetText("Time Elapsed: " + PlayerPrefs.GetInt("TimeCounter").ToString());

        PlayerPrefs.SetInt("CanContinue", 1);
    }

    public bool GetTakenOver()
    {
        return takenOver;
    }

    public void StoreParasiteSpriteInformation()
    {
        parasiteRenderer = gameObject.transform.Find("ParasiteSpriteInfo").GetComponent<SpriteRenderer>();
        parasiteAnimator = gameObject.transform.Find("ParasiteSpriteInfo").GetComponent<Animator>();
    }

    public void UnTransformToParasite()
    {
        SpriteRenderer playerRenderer = gameObject.GetComponent<SpriteRenderer>();
        playerRenderer.sprite = parasiteRenderer.sprite;
        playerRenderer.color = parasiteRenderer.color;
        playerRenderer.size = parasiteRenderer.size;

        Animator playerAnimator = gameObject.GetComponent<Animator>();

        if (!playerAnimator){
            playerAnimator = gameObject.AddComponent<Animator>();
        }

        playerAnimator.runtimeAnimatorController = parasiteAnimator.runtimeAnimatorController;
        playerAnimator.applyRootMotion = parasiteAnimator.applyRootMotion;
        playerAnimator.updateMode = parasiteAnimator.updateMode;
        playerAnimator.cullingMode = parasiteAnimator.cullingMode;
        playerAnimator.avatar = parasiteAnimator.avatar;

        int difficulty = PlayerPrefs.GetInt("Difficulty");
        int newHealth = 500 / difficulty;
        healthSystem.SetHealth(parasiteHealth);
        healthSystem.SetMaxHealth(newHealth);

        takenOver = false;
    }

    private void FixedUpdate()
    {
        if (timeSinceTakeOver > 0.1) {
            TryPlayerMovement();
        }
    }
    public void LoadData(GameData data)
    {
        transform.position = data.playerPosition;
        // If floor name is Floor5 and position is exactly (0,0), then move player to (22,0)
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Floor5" && transform.position == new Vector3(0, 0, 0))
        {
            transform.position = new Vector3(22, 0, 0);
        }
    }
    public void SaveData(ref GameData data)
    {
        data.playerPosition = transform.position;
    }
    private void Awake()
    {
        // Initialise the audio manager for enemy related sound effects
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        // Initialise PauseCounter to 0
        PlayerPrefs.SetInt("PauseCounter", 0);
    }

    /* TryPlayerMovement, Attempt to move the player in the chosen direction by using Trymove, gliding when incident on walls at an angle is important
        So TryMove needs to be called twice unlike for bullet movement*/
    private void TryPlayerMovement()
    {
        if (movementInput == Vector2.zero) {
            return;
        }

        Vector2 position = rb.position;
        
        // Try moving
        movementInput = movementInput.normalized;
        position = TryMove(position, new Vector2(movementInput.x, 0));
        position = TryMove(position, new Vector2(0, movementInput.y));

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
            audioManager.PlaySFX(audioManager.bump);
        }

        if (count == 0 || AllAreMyBullets(hits))
        {
            position += direction * moveSpeed * Time.fixedDeltaTime;
        }
        return position;
    }

    /* AllAreMyBullets, takes a list of RayCastHit2Ds and checks whether all of them are bullets that were generated by this
            object, if they are then the player should be able to move anyway, this allows movement of the player through a cloud
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

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    public void TaseEnemy(GameObject enemy)
    {
        tasedEnemy = enemy;
        taserTimer = taserTime;
        tasedEnemy.GetComponent<Enemy>().SetTased(true);
        tasingEnemy = true;
        Vector3 spawnPosition = new Vector3(0f, 0f, 0f); // Adjust the position as needed
        Quaternion spawnRotation = Quaternion.identity; // No rotation
        taserInstance = Instantiate(taserPrefab, spawnPosition, spawnRotation);
        SpriteRenderer taserRenderer = taserInstance.GetComponent<SpriteRenderer>();
        if (taserRenderer != null)
        {
            taserRenderer.sortingLayerName = "UI";
        }
    }

    private void UpdateTaserInstance()
    {

        Vector2 position = rb.position;
        Vector2 enemyPosition = tasedEnemy.transform.position;
        Vector2 midpoint = new Vector2((position.x  + enemyPosition.x)/2, (position.y + enemyPosition.y)/2);
        taserInstance.transform.position = midpoint;

        Vector2 direction = tasedEnemy.transform.position - gameObject.transform.position;
        float angleRadians = Mathf.Atan2(direction.y, direction.x);
        float angleDegrees = angleRadians * Mathf.Rad2Deg + 45;
        taserInstance.transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

        double distance = Math.Sqrt(Math.Pow(position.x - enemyPosition.x, 2) + Math.Pow(position.x - enemyPosition.x,2));
        float scaleFactor = (float) (taserSizeConstant * distance / (Math.Sqrt(2) * 32));
        Vector3 newScale = new Vector3(scaleFactor, scaleFactor, 1f);
        taserInstance.transform.localScale = newScale;
    }

    public void TakeOverEnemy() {

        if (!takenOver)
        {
            parasiteHealth = healthSystem.GetHealth();
        }

        rb.MovePosition(tasedEnemy.transform.position);
        SpriteRenderer enemyRenderer = tasedEnemy.GetComponent<SpriteRenderer>();
        SpriteRenderer playerRenderer = gameObject.GetComponent<SpriteRenderer>();

        Animator enemyAnimator = tasedEnemy.GetComponent<Animator>();
        Animator playerAnimator = gameObject.GetComponent<Animator>();

        if (!playerAnimator){
            playerAnimator = gameObject.AddComponent<Animator>();
        }

        playerRenderer.sprite = enemyRenderer.sprite;
        playerRenderer.color = enemyRenderer.color;
        playerRenderer.size = enemyRenderer.size;

        if (enemyAnimator) {
            playerAnimator.runtimeAnimatorController = enemyAnimator.runtimeAnimatorController;
            playerAnimator.applyRootMotion = enemyAnimator.applyRootMotion;
            playerAnimator.updateMode = enemyAnimator.updateMode;
            playerAnimator.cullingMode = enemyAnimator.cullingMode;
            playerAnimator.avatar = enemyAnimator.avatar;
        }

        HealthSystem enemyHealthSystem = tasedEnemy.GetComponent<HealthSystem>();
        healthSystem.SetHealth(Math.Max(enemyHealthSystem.GetHealth(), 100));
        healthSystem.SetMaxHealth(enemyHealthSystem.GetMaxHealth());

        float enemyMaxHealth = enemyHealthSystem.GetMaxHealth();
        GameObject hudControllerObject = GameObject.FindGameObjectWithTag("HUDController");
        hudControllerObject.SendMessage("UpdateScore", 200);
        // Switch on the max enemy health to set tickDamage
        // switch maxHP -> hpTickPerSecond
        // 85 -> 85/60
        // 150 -> 150/53
        // 300 -> 300/40
        // 500 -> 500/20
        
        switch ((int) enemyMaxHealth)
        {
            case 85:
                tickDamage = 85f/60f;
                break;
            case 150:
                tickDamage = 150f/53f;
                break;
            case 300:
                tickDamage = 300f/40f;
                break;
            case 500:
                tickDamage = 500f/20f;
                break;
            default:
                Debug.LogWarning("Enemy max health not recognised, defaulting to 2 damage per second");
                tickDamage = 2;
                break;
        }
        Debug.Log(tickDamage);
        Debug.Log(enemyMaxHealth);
        Debug.Log(healthSystem.GetHealth());
        Debug.Log("");


        takenOver = true;

        Destroy(tasedEnemy, 0f);
    }

    // Update, regular updates for the player that need to occur every frame
    void Update()
    {
        PlayerPrefs.SetFloat("TimeCounter", PlayerPrefs.GetFloat("TimeCounter") + Time.deltaTime);
        timeElapsedText.SetText("Time Elapsed: " + PlayerPrefs.GetFloat("TimeCounter").ToString("F1"));

        // Update the firetimer so it accurately represents how long until the next bullet can be shot
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
            
            if (fireTimer < 0)
            {
                fireTimer = 0;
            }
        }

        if (taserTimer > 0)
        {
            if (taserInstance != null)
            {
                UpdateTaserInstance();
            }
            taserTimer -= Time.deltaTime;
            
            if (taserTimer < 0)
            {
                taserTimer = 0;
            }
        }

        timeSinceTakeOver += Time.deltaTime;

        if (tasingEnemy && taserTimer == 0)
        {
            tasingEnemy = false;
            Destroy(taserInstance, 0f);
            taserInstance = null;
            TakeOverEnemy();
            timeSinceTakeOver = 0;
            tickNumber = 0;
        }

        if ((tickNumber <= timeSinceTakeOver) && takenOver)
        {
            tickNumber += 1;
            healthSystem.TakeDamage(tickDamage);
        }


        //Player Shooting Setup
        
        Vector2 position = rb.position;
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        // Find the angle that the player is aiming in to shoot at this angle
        double angle = Math.Atan2(position.y - worldMousePos.y, position.x - worldMousePos.x) * (180.0 / Math.PI) + 180;

        // Snap the angle to the nearest 40 degrees to provide a retro feel with shooting
        //int snappedAngle = (int) Math.Round(angle / 45) * 45;

        int intAngle = (int) angle;

        /* Shooting check - Check whether enough time has elapsed since the last shot for the "gun" to allow me to shoot again
            This is how a fire rate is implemented preventing a bullet being shot every frame*/
        if (Input.GetMouseButtonDown(0) && (fireTimer == 0))
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
    }
}