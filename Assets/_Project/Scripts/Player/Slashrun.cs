using UnityEngine;

public class Slashrun : MonoBehaviour
{
    public float lifeTime = 1f;
    private float speed = 8f;
    private Vector3 direction;
    public float v0Magnitude = 50f;
    public Vector2 mapBounds = new Vector2(32f, 18f);
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private float playerAtk = 0f;
    private bool isRun = false;
    private int hitMobCount = 5;
    private readonly System.Collections.Generic.HashSet<int> hitEnemies = new();
    
    private bool isPoisoned = false;
    private float poisonHpPercent = 0f;
    private float poisonDuration = 0f;
    private float poisonMaxDmg = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Đổi sang Dynamic để linearVelocity và collision callback hoạt động đúng
            // Kinematic không đảm bảo OnCollisionEnter2D fire trên enemy
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.mass = 0.1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        if (lifeTime > 0f)
        {
            Destroy(gameObject, lifeTime);
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            Collider2D playerCol = playerObj.GetComponent<Collider2D>();
            Collider2D myCol = GetComponent<Collider2D>();
            if (playerCol != null && myCol != null)
                Physics2D.IgnoreCollision(myCol, playerCol);
        }
    }

    void FixedUpdate()
    {
        if (!isRun) return;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            transform.Translate(direction * speed * Time.fixedDeltaTime, Space.World);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (Vector3.Distance(startPosition, transform.position) >= v0Magnitude)
        {
            Destroy(gameObject);
            return;
        }

        if (Mathf.Abs(transform.position.x) > mapBounds.x || Mathf.Abs(transform.position.y) > mapBounds.y)
        {
            Destroy(gameObject);
        }
    }

    private bool isLifesteal = false;

    // Nhận hướng bay và ATK của người chơi từ PlayerController
    public void SetDirection(Vector3 dir, float atk, bool pIsPoisoned = false, float pHpPercent = 0f, float pDuration = 0f, float pMaxDmg = 0f, bool pIsLifesteal = false)
    {
        direction = dir.normalized;
        playerAtk = atk;
        startPosition = transform.position;
        isRun = true;
        
        isPoisoned = pIsPoisoned;
        poisonHpPercent = pHpPercent;
        poisonDuration = pDuration;
        poisonMaxDmg = pMaxDmg;
        isLifesteal = pIsLifesteal;
    }

    // Trigger thay Collision — slash xuyên qua, không destroy khi trúng mob
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Mob") && !other.CompareTag("Boss")) return;

        int id = other.gameObject.GetInstanceID();
        if (hitEnemies.Contains(id)) return;
        hitEnemies.Add(id);

        if (other.CompareTag("Mob"))
        {
            FollowTarget enemy = other.gameObject.GetComponent<FollowTarget>();
            if (enemy != null)
            {
                enemy.TackenDame(playerAtk);
                if (isPoisoned) 
                {
                    enemy.ApplyPoison(poisonHpPercent, poisonDuration, poisonMaxDmg);
                }
                enemy.SetKnockedBack((Vector2)direction, 5f);
            }
        }
        else if (other.CompareTag("Boss"))
        {
            BallonBoss boss = other.gameObject.GetComponent<BallonBoss>();
            if (boss != null)
            {
                boss.TakeDamage(playerAtk);
                if (isPoisoned) 
                {
                    boss.ApplyPoison(poisonHpPercent, poisonDuration, poisonMaxDmg);
                }
            }
        }

        if (isLifesteal)
        {
            UpdateStatePlayer player = GameObject.FindWithTag("Player")?.GetComponent<UpdateStatePlayer>();
            if (player != null)
            {
                player.ChangeHP(player.maxhp * 0.02f);
            }
        }

        hitMobCount--;
        if (hitMobCount <= 0) Destroy(gameObject);
    }
}
