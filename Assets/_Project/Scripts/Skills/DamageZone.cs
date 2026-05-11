using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vùng gây sát thương tái sử dụng được — dùng cho Thiên Thạch, Boss skill, etc.
/// Yêu cầu: GameObject có CircleCollider2D (IsTrigger = true) và SpriteRenderer (optional).
/// </summary>
public class DamageZone : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 30f;
    public float lifetime = 2f;          // Tồn tại bao lâu sau khi kích hoạt
    public float tickInterval = 0f;      // 0 = chỉ gây dame 1 lần khi enter; > 0 = gây dame theo chu kì
    public bool hitPlayer = true;        // Có gây dame player không (boss skill sẽ bật cái này)
    public bool hitMob = false;          // Có gây dame mob không (meteor của player thì true)
    public bool hitBoss = false;
    public string sourceTag = "Player";  // Nguồn gốc sát thương (Player hoặc Boss)

    [Header("Visual")]
    private SpriteRenderer zoneRenderer;
    public Color warningColor;
    public Color activeColor = new Color(1f, 0f, 0f, 0.6f);      // Màu khi đang active

    [Header("State")]
    public bool isActive = false;        // Nếu false = đang là vùng cảnh báo, chưa gây dame

    private HashSet<Collider2D> hitOnce = new HashSet<Collider2D>(); // Tránh hit 2 lần nếu tickInterval = 0
    private float tickTimer = 0f;

    private void Start()
    {
        zoneRenderer = GetComponent<SpriteRenderer>();
        if (zoneRenderer == null) zoneRenderer = GetComponentInChildren<SpriteRenderer>();

        // Hiển thị màu cảnh báo ban đầu
        if (zoneRenderer != null)
            warningColor = zoneRenderer.color;
    }

    /// <summary>
    /// Gọi để kích hoạt vùng gây dame (ví dụ: thiên thạch đã rơi xuống)
    /// </summary>
    public void Activate()
    {
        Debug.Log("[DamageZone] Activate called! Changing color and damaging.");
        isActive = true;
        hitOnce.Clear();

        if (zoneRenderer != null)
            zoneRenderer.color = activeColor;

        // Gây sát thương ngay lập tức cho những đối tượng đang đứng sẵn trong vùng
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null) col = GetComponentInChildren<CircleCollider2D>();
        if (col != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, col.radius * transform.localScale.x);
            foreach (var hit in hits)
            {
                HandleHit(hit);
            }
        }

        StartCoroutine(ExpireAfter(lifetime));
    }

    private void Update()
    {
        if (!isActive || tickInterval <= 0f) return;

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            ApplyDamageInZone();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[DamageZone] OnTriggerEnter2D with: " + other.name + " | isActive: " + isActive);
        if (!isActive) return;
        if (tickInterval > 0f) return; // Dùng tick thay vì enter

        HandleHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive || tickInterval <= 0f) return;
        // tick đã xử lý trong Update → bỏ qua
    }

    private void HandleHit(Collider2D other)
    {
        if (hitOnce.Contains(other)) return;

        if (hitPlayer && other.CompareTag("Player"))
        {
            UpdateStatePlayer player = other.GetComponent<UpdateStatePlayer>();
            if (player != null)
            {
                player.taken_damage(damage*0.8f);
                hitOnce.Add(other);
            }
        }

        if (hitMob && other.CompareTag("Mob"))
        {
            FollowTarget mob = other.GetComponent<FollowTarget>();
            if (mob != null)
            {
                mob.TackenDame(damage, sourceTag);
                hitOnce.Add(other);
            }
        }

        if (hitBoss && other.CompareTag("Boss"))
        {
            BallonBoss boss = other.GetComponent<BallonBoss>();
            if (boss != null)
            {
                boss.TakeDamage(damage*0.8f);
                hitOnce.Add(other);
            }
        }
    }

    private void ApplyDamageInZone()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, col.radius * transform.localScale.x);
        foreach (var hit in hits)
        {
            HandleHit(hit);
        }
        hitOnce.Clear(); // Cho phép hit lại trong tick tiếp theo
    }

    private IEnumerator ExpireAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
