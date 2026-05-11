using UnityEngine;

/// <summary>
/// Vũng Lầy (Swamp): Spawn tại vị trí Player, slow mọi mob bước vào.
/// Prefab cần: SpriteRenderer (vũng lầy), CircleCollider2D (IsTrigger, bán kính lớn), script này.
/// </summary>
public class SwampBehavior : MonoBehaviour
{
    [Header("Stats")]
    public float slowMultiplier = 0.35f;
    public int cardStar = 1;

    private System.Collections.Generic.Dictionary<FollowTarget, float> originalSpeeds = new System.Collections.Generic.Dictionary<FollowTarget, float>();
    private System.Collections.Generic.Dictionary<FollowTarget, float> timeInSwamp = new System.Collections.Generic.Dictionary<FollowTarget, float>();

    public void Init(float slowPercent, int star)
    {
        slowMultiplier = 1f - slowPercent; 
        cardStar = star;
    }

    private void Update()
    {
        if (cardStar < 3) return;
        System.Collections.Generic.List<FollowTarget> mobs = new System.Collections.Generic.List<FollowTarget>(timeInSwamp.Keys);
        foreach (var mob in mobs)
        {
            if (mob != null)
            {
                timeInSwamp[mob] += Time.deltaTime;
                if (timeInSwamp[mob] >= 3f)
                {
                    mob.speed = 0.1f;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Mob") && !other.CompareTag("Boss")) return;

        FollowTarget mob = other.GetComponent<FollowTarget>();
        if (mob != null && !originalSpeeds.ContainsKey(mob))
        {
            originalSpeeds[mob] = mob.speed;
            timeInSwamp[mob] = 0f;
            mob.speed = mob.speed * slowMultiplier;
        }
        BallonBoss bb = other.GetComponent<BallonBoss>();
        if (bb != null)
        {
            bb.speed = 0.2f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Mob")) return;

        FollowTarget mob = other.GetComponent<FollowTarget>();
        if (mob != null && originalSpeeds.ContainsKey(mob))
        {
            mob.speed = originalSpeeds[mob];
            originalSpeeds.Remove(mob);
            timeInSwamp.Remove(mob);
        }
    }

    private void OnDestroy()
    {
        foreach (var pair in originalSpeeds)
        {
            if (pair.Key != null)
                pair.Key.speed = pair.Value;
        }
        originalSpeeds.Clear();
        timeInSwamp.Clear();
    }
}
