using UnityEngine;
using System.Collections;

public class BallonBoss : MonoBehaviour
{
    public Transform player;
    public GameManagerS GM;

    [Header("Stats")]
    public float hp;
    public float maxHp;
    public float atk;
    public float speed = 1f;

    private int killCount = 0;
    private bool isInvincible = false;
    private bool hasTriggered30 = false;
    public bool isDead = false;

    [Header("Prefabs")]
    public GameObject fireballPrefab;
    public GameObject meteorPrefab; 
    public GameObject damageZonePrefab; 
    private SimpleHPBar hpBar;

    [Header("Poison Debuff")]
    private float currentPoisonDuration = 0f;
    private float poisonHpPercent = 0f;
    private float poisonMaxDmg = 0f;
    private float totalPoisonDamage = 0f;

    private Animator anim;
    private float shootCooldown = 2f; // Bắn cầu lửa mỗi 2 giây
    private float lastShootTime = 0f;

    void Start()
    {
        GM = GameObject.Find("GameManager")?.GetComponent<GameManagerS>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        anim = GetComponent<Animator>();

        int lv = Mathf.FloorToInt(GM.timePlay / 300f);
        maxHp = 500f + 200f * lv;
        hp = maxHp;
        atk = 100f + 30f * lv + killCount;

        hpBar = GetComponent<SimpleHPBar>();
        if (hpBar == null) hpBar = gameObject.AddComponent<SimpleHPBar>();
        hpBar.yOffset = 1.0f; // Boss to nên đặt offset thấp lại tí vì scale đã x20
        hpBar.barWidth = 1.5f; 
        hpBar.barHeight = 0.15f;
    }

    public void OnMobKilled() 
    {
        killCount++;
        int lv = Mathf.FloorToInt(GM.timePlay / 300f);
        atk = 100f + 30f * lv + killCount;
    }

    void Update()
    {
        if (GM == null || GM.isPause || isDead || player == null) return;

        // Xử lý độc
        if (currentPoisonDuration > 0)
        {
            GetComponent<SpriteRenderer>().color = Color.green; // Đổi màu báo dính độc
            currentPoisonDuration -= Time.deltaTime;
            float dmg = (maxHp * poisonHpPercent) * Time.deltaTime; 
            if (totalPoisonDamage + dmg > poisonMaxDmg)
            {
                dmg = poisonMaxDmg - totalPoisonDamage;
                currentPoisonDuration = 0;
            }
            if (dmg > 0)
            {
                totalPoisonDamage += dmg;
                TakeDamage(dmg); 
            }
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white; // Trở lại bình thường
        }

        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        if (isInvincible) return; // Nếu đang xài chiêu có thể đứng lại

        float distance = Vector2.Distance(transform.position, player.position);
        Vector3 dir = (player.position - transform.position).normalized;

        if (distance > 5.1f)
        {
            transform.position += dir * speed * Time.deltaTime;
        }
        else if (distance < 4.9f)
        {
            transform.position -= dir * speed * Time.deltaTime;
        }

        // Lật mặt hướng về player
        if (dir.x > 0) transform.localScale = new Vector3(-18f, 20f, 1f);
        else transform.localScale = new Vector3(18f, 20f, 1f);
    }

    void HandleShooting()
    {
        if (isInvincible) return;

        if (Time.time - lastShootTime > shootCooldown)
        {
            lastShootTime = Time.time;
            ShootFireball();
        }
    }

    void ShootFireball()
    {
        if (fireballPrefab == null || player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        float alpha = 30f;

        // Giữa
        SpawnSingleFireball(dir);
        // Trái
        SpawnSingleFireball(Quaternion.Euler(0, 0, alpha) * dir);
        // Phải
        SpawnSingleFireball(Quaternion.Euler(0, 0, -alpha) * dir);
    }

    void SpawnSingleFireball(Vector3 dir)
    {
        GameObject fb = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        FireballBehavior fbb = fb.GetComponent<FireballBehavior>();
        if (fbb != null)
        {
            fbb.Init(dir, atk);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead) return;

        hp -= damage;
        if (hpBar != null) hpBar.UpdateHP(hp, maxHp);
        
        // Kích hoạt skill 30% HP
        if (hp <= maxHp * 0.3f && !hasTriggered30)
        {
            StartCoroutine(Trigger30PercentSkill());
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    public void ApplyPoison(float hpPercent, float duration, float maxDmg)
    {
        currentPoisonDuration = duration;
        poisonHpPercent = hpPercent;
        poisonMaxDmg = maxDmg;
        totalPoisonDamage = 0f;
    }

    private IEnumerator Trigger30PercentSkill()
    {
        hasTriggered30 = true;
        isInvincible = true; // Bất tử 2s
        
        // Gọi 5 thiên thạch
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = (Vector3)Random.insideUnitCircle * 5f;
            Vector3 targetPos = player.position + randomOffset;
            SpawnMeteor(targetPos);
            yield return new WaitForSeconds(0.4f); // 5 quả trải dài trong 2s
        }

        isInvincible = false;
    }

    private void SpawnMeteor(Vector3 targetPos)
    {
        if (meteorPrefab == null) return;
        GameObject m = Instantiate(meteorPrefab, targetPos, Quaternion.identity);
        MeteorBehavior mb = m.GetComponent<MeteorBehavior>();
        if (mb != null)
        {
            mb.damage = atk * 2.5f; // Sát thương 250% ATK
            mb.hitPlayer = true;    // Thiên thạch Boss gọi sẽ đánh Player
            mb.hitMob = true;       // Thiên thạch Boss gọi có thể đánh cả quái
            mb.hitBoss = false;     // Không đánh boss
            mb.sourceTag = "Boss";  // Sát thương từ boss
            mb.SetTarget(targetPos);
        }
    }

    private void Die()
    {
        isDead = true;
        if (anim != null) anim.SetBool("isDie", true);
        StartCoroutine(DeathExplosion());
    }

    private IEnumerator DeathExplosion()
    {
        yield return new WaitForSeconds(2f); // Chờ 2s chết
        
        if (damageZonePrefab != null)
        {
            GameObject dzObj = Instantiate(damageZonePrefab, transform.position, Quaternion.identity);
            DamageZone dz = dzObj.GetComponent<DamageZone>();
            if (dz != null)
            {
                dz.damage = atk * 3f;
                dz.lifetime = 0.5f;
                dz.tickInterval = 0f;
                dz.hitPlayer = true;
                dz.hitMob = true; // Sát thương lan ra cả quái
                dz.hitBoss = false;
                dz.sourceTag = "Boss";
            }
            
            // Bán kính 8 = scale 16 (vì r mặc định là 0.5)
            dzObj.transform.localScale = new Vector3(16f, 16f, 1f); 
            dz.Activate();
        }
        
        Destroy(gameObject);
    }
}
