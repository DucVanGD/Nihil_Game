using System.Collections;
using UnityEngine;

/// <summary>
/// Thiên Thạch (Meteor): Rơi từ trên xuống, hiện vòng cảnh báo, gây AOE dame sau khi chạm đất.
/// Prefab cần: SpriteRenderer, Rigidbody2D, script này.
/// DamageZone prefab riêng biệt được spawn tại điểm rơi.
/// </summary>
public class MeteorBehavior : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 40f;
    public float fallSpeed = 8f;        // Tốc độ rơi (unit/s)
    public float warningDuration = 1.2f; // Thời gian hiện vòng cảnh báo trước khi thiên thạch xuất hiện
    public bool hitMob = true;
    public bool hitPlayer = true; // Đổi thành true để gây dame cả player
    public bool hitBoss = true;
    public string sourceTag = "Player";

    [Header("Impact Zone")]
    public GameObject damageZonePrefab; // Prefab chứa DamageZone script
    public float zoneRadius = 1.5f;
    public float zoneLifetime = 2f;

    [Header("Spawn")]
    public float spawnHeightOffset = 6f; // Spawn từ trên bao nhiêu unit so với target

    private Vector3 targetPos;
    private bool hasImpacted = false;
    private DamageZone spawnedZone;

    /// <summary>
    /// Gọi từ UseCard sau khi spawn prefab này.
    /// </summary>
    public void SetTarget(Vector3 target)
    {
        targetPos = target;
        // Spawn thiên thạch ở trên cao
        transform.position = target + Vector3.up * spawnHeightOffset;
        // Spawn vòng cảnh báo ngay tại điểm rơi
        SpawnWarningZone();
        StartCoroutine(FallDown());
    }

    private void SpawnWarningZone()
    {
        if (damageZonePrefab == null)
        {
            Debug.LogWarning("[MeteorBehavior] Chưa gán damageZonePrefab!");
            return;
        }

        GameObject zoneObj = Instantiate(damageZonePrefab, targetPos, Quaternion.identity);
        zoneObj.transform.localScale = Vector3.one * (zoneRadius * 2f);
        Debug.Log("[Meteor] Spawned zoneObj: " + zoneObj.name);

        DamageZone zone = zoneObj.GetComponentInChildren<DamageZone>();
        if (zone != null)
        {
            Debug.Log("[Meteor] Found DamageZone component on: " + zone.gameObject.name);
            zone.damage = damage;
            zone.lifetime = zoneLifetime;
            zone.hitMob = hitMob;
            zone.hitPlayer = hitPlayer;
            zone.hitBoss = hitBoss;
            zone.sourceTag = sourceTag;
            zone.isActive = false;  // Chưa activate — chỉ hiện cảnh báo
        }
        else
        {
            Debug.LogWarning("[Meteor] DamageZone component NOT found on: " + zoneObj.name + " hoặc các con của nó!");
        }

        spawnedZone = zone;
    }

    private IEnumerator FallDown()
    {
        // Chờ một chút để vòng cảnh báo hiện ra trước
        yield return new WaitForSeconds(warningDuration * 0.3f);

        // Rơi về targetPos
        while (!hasImpacted)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                hasImpacted = true;
                OnImpact();
            }
            yield return null;
        }
    }

    private void OnImpact()
    {
        // Kích hoạt vùng sát thương khi chạm đất
        if (spawnedZone != null)
        {
            spawnedZone.Activate();
        }

        // Hiệu ứng nổ nhỏ (optional: thêm particle sau)
        Debug.Log("[Meteor] Impact tại: " + targetPos);
        Destroy(gameObject, 0.1f);
    }
}
