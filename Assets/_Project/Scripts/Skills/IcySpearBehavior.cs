using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Giáo Băng (Icicle Spear): Bay về phía con trỏ chuột, gây dame + slow khi trúng mob.
/// Prefab cần: SpriteRenderer, Rigidbody2D (Kinematic), CircleCollider2D (IsTrigger), script này.
/// </summary>
public class IcySpearBehavior : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 50f;
    public float speed = 12f;
    public float slowMultiplier = 0.4f;  // Làm chậm mob còn 40% tốc độ
    public float slowDuration = 2f;
    public float lifetime = 4f;
    public float explosionRadius = 2f;

    private Vector2 direction;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    public void Init(float dmg, float slow)
    {
        damage = dmg;
        slowMultiplier = 1f - slow; // Giảm tốc X% -> Tốc độ còn (1 - X)
    }

    private void Start()
    {
        // Đảm bảo là Trigger để đi xuyên vật thể
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // Tính hướng từ player đến chuột
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            direction = (mouseWorld - transform.position).normalized;
        }
        else
        {
            direction = Vector2.right;
        }

        // Xoay sprite theo hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mob") || other.CompareTag("Boss"))
        {
            if (!hitEnemies.Contains(other))
            {
                hitEnemies.Add(other);
                
                FollowTarget mob = other.GetComponent<FollowTarget>();
                if (mob != null)
                {
                    mob.TackenDame(damage);
                    StartCoroutine(SlowMob(mob));
                }
                
                Explode(); // Gây nổ ở mỗi quái đi qua (nếu không muốn nổ, hãy comment dòng này)
            }
        }
    }

    private void Explode()
    {
        // Gây sát thương nổ (một nửa) trong phạm vi
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Mob"))
            {
                FollowTarget mob = hit.GetComponent<FollowTarget>();
                if (mob != null)
                {
                    mob.TackenDame(damage / 2f);
                }
            }
        }
    }

    private System.Collections.IEnumerator SlowMob(FollowTarget mob)
    {
        if (mob == null) yield break;

        float originalSpeed = mob.speed;
        mob.speed = originalSpeed * slowMultiplier;
        yield return new WaitForSeconds(slowDuration);
        if (mob != null)
            mob.speed = originalSpeed;
    }
}
