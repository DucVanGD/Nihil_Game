using System.Collections;
using UnityEngine;

/// <summary>
/// Lốc xoáy (Tornado): Bám theo Player, xoay tròn, gây dame liên tục cho mob tiếp xúc.
/// Prefab cần: SpriteRenderer, CircleCollider2D (IsTrigger), script này.
/// </summary>
public class TornadoBehavior : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 15f;
    public float tickInterval = 0.4f; 
    public float moveSpeed = 6f;      
    public float orbitRadius = 4f;  
    public float duration = 5f;
    public float knockbackForce = 6f;  // Lực đẩy lùi

    [Header("Spin")]
    public float spinSpeed = 360f;     // Độ/giây tự xoay của sprite

    private Transform playerTransform;
    private float angle = 0f;
    private float tickTimer = 0f;
    private float lifeTimer = 0f;

    public void Init(float dmg, float time)
    {
        damage = dmg;
        duration = time;
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        // Xoay quanh player
        if (playerTransform != null)
        {
            angle += moveSpeed * Time.deltaTime; // Tốc độ xoay (tính bằng Radian/giây)
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * orbitRadius;
            transform.position = playerTransform.position + offset;
        }

        // Xoay sprite
        transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

        // Tick dame
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            DamageNearby();
        }
    }

    private void DamageNearby()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        float radius = col != null ? col.radius * transform.localScale.x : 0.8f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Mob"))
            {
                FollowTarget mob = hit.GetComponent<FollowTarget>();
                if (mob != null)
                {
                    mob.TackenDame(damage);
                    // Hướng đẩy lùi từ lốc xoáy đến quái
                    Vector2 pushDir = (hit.transform.position - transform.position).normalized;
                    mob.SetKnockedBack(pushDir, knockbackForce);
                }
            }
            if (hit.CompareTag("Boss"))
            {
                BallonBoss bb = hit.GetComponent<BallonBoss>();
                if (bb != null)
                {
                    bb.TakeDamage(damage*0.9f);
                }
            }
        }
    }
}
