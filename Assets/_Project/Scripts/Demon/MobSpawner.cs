using UnityEngine;
using UnityEngine.UIElements;


public class MobSpawner : MonoBehaviour
{
    public GameObject[] Mobs;
    public Transform player;
    public float maxmob = 20;
    private float respawn_time;
    private float zoneX = 30;
    private float zoneY = 16;
    private float time = 0;
    private float nextDecreaseTime = 300f;
    private float starttime;
    public GameManagerS GM;

    [Header("Boss Settings")]
    public GameObject bossPrefab;
    private float nextBossSpawnTime = 300f;

    void Start()
    {
        if (GM == null) GM = FindFirstObjectByType<GameManagerS>();
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        starttime = Time.time;
        float sTime = SettingGame.Instance != null ? SettingGame.Instance.spawnTime : 0.5f;
        respawn_time = Mathf.Max(0.8f, 4f - sTime);
    }

void Update()
{
    if (GM.isPause == false)
    {
        if (GM.timePlay > nextDecreaseTime && respawn_time > 0.5f)
        {
            respawn_time *= 0.95f;
            nextDecreaseTime += 5f;
            respawn_time = Mathf.Max(respawn_time, 0.5f);
        }

        if (GM.timePlay >= nextBossSpawnTime)
        {
            nextBossSpawnTime += 300f;
            SpawnBoss();
        }

        SpawnMob();
    }
}

public void SpawnMob()
{
    if (GM.isPause || Mobs.Length == 0) return;

    if (GM.timePlay - time > respawn_time)
    {
        time = GM.timePlay;
        GameObject[] currentMobs = GameObject.FindGameObjectsWithTag("Mob");
        if (currentMobs.Length >= maxmob) return;
        int spawnCount = 3 + Mathf.FloorToInt(GM.timePlay / 60f);
        spawnCount = Mathf.Clamp(spawnCount, 3, 15);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 spawnPosition = ZoneSpawn();

            GameObject mobPrefab = Mobs[Random.Range(0, Mobs.Length)];

            GameObject mob = Instantiate(
                mobPrefab,
                spawnPosition,
                Quaternion.identity
            );

            mob.GetComponent<FollowTarget>().Ptarget = player.transform;
        }
    }
}

public void SpawnBoss()
{
    if (bossPrefab == null || player == null) return;

    Vector2 spawnPosition = ZoneSpawn();
    Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
}

    private Vector3 ZoneSpawn()
    {
        Vector3 res= new Vector3();
        int attempts = 0;
        do
        {
            float x = Random.Range(0f, zoneX);
            float y = Random.Range(0f, zoneY);
            res = new Vector2(x, y);

            attempts++;
            if (attempts > 100)
            {
                break;
            }

        } while (Vector2.Distance(res, player.position) <5f);
        return res;
    }

}
