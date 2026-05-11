using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UseCard : MonoBehaviour
{
    private const string DefaultCardName = "No Card";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameManagerS GM;
    private UpdateStatePlayer player;

    [Header("Game Object")]
    public GameObject Tornado;
    public GameObject IcySpear;
    public GameObject Meteor;
    public GameObject Swamp;

    [Header("Effect")]
    public Sprite Burn;
    public Sprite Stornger;
    public Sprite Healling;
    public Sprite SpeedUp;
    public Sprite Barrier;

    private Coroutine speedRoutine;
    private Coroutine atkRoutine;
    private Coroutine barrierRoutine;
    private GameObject currentSwamp;
    private Label statusLabel;
    private readonly Dictionary<string, float> effectTimers = new Dictionary<string, float>();

    void Start()
    {
        ResolveReferences();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEffectTimers();
    }

    public bool CastCard( Card c)
    {
        if (c == null || c.nameCard == DefaultCardName)
        {
            return false;
        }

        ResolveReferences();

        if (!TrySpendMp(c))
        {
            return false;
        }

            float[] stats = GetCardStats(c);
        PlaySkillAnimation();
        switch (c.nameCard)
        {
            case "Healing":
                SetEffectTimer("Hồi máu", stats.Length > 1 ? stats[1] : 0f);
                ShowEffectSprite(Healling, stats.Length > 1 ? stats[1] : 2f);
                StartCoroutine(HealOverTime(stats, c.lvStar));
                break;
            case "SpeedBoots":
                SetEffectTimer("Tăng tốc", stats.Length > 1 ? stats[1] : 0f);
                ShowEffectSprite(SpeedUp, stats.Length > 1 ? stats[1] : 2f);
                ApplySpeedBoost(stats);
                break;
            case "PowerUp":
                SetEffectTimer("Cường hóa", stats.Length > 1 ? stats[1] : 0f);
                ShowEffectSprite(Stornger, stats.Length > 1 ? stats[1] : 2f);
                ApplyPowerUp(stats, c.lvStar);
                break;
            case "Barrier":
                SetEffectTimer("Lá chắn", stats.Length > 1 ? stats[1] : 0f);
                ShowEffectSprite(Barrier, stats.Length > 1 ? stats[1] : 2f);
                ApplyBarrier(stats);
                break;
            case "Tornado":
                SpawnTornado(stats);
                break;
            case "Icicle Spear":
                SpawnIcySpear(stats);
                break;
            case "Meteor":
                SpawnMeteor(stats);
                break;
            case "Swamp":
                SpawnSwamp(stats, c.lvStar);
                break;
            case "Burnning Body":
                SetEffectTimer("Bốc cháy", stats.Length > 2 ? stats[2] : 0f);
                ShowEffectSprite(Burn, stats.Length > 2 ? stats[2] : 2f);
                StartCoroutine(BurnningBody(stats, c.lvStar));
                break;
            case "StarUp":
                ApplyStarUp(stats);
                break;
            case "Poison":
                SetEffectTimer("Tẩm độc", stats.Length > 0 ? stats[0] : 3f);
                ApplyPoisonWeapon(stats);
                break;
            default:
                Debug.LogWarning($"Không nhận ra thẻ: {c.nameCard}");
                break;
        }

        return true;
    } 

    private void PlaySkillAnimation()
    {
        if (player == null) return;
        Animator animator = player.GetComponent<Animator>();
        if (animator == null) return;

        int stateHash = Animator.StringToHash("superstar");
        if (animator.HasState(0, stateHash))
        {
            animator.Play(stateHash, 0, 0f);
            // Animator không có transition thoát — force reset sau khi clip kết thúc
            StartCoroutine(ResetAnimAfterSkill(animator, 0.6f));
        }
    }

    private System.Collections.IEnumerator ResetAnimAfterSkill(Animator anim, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (anim == null) yield break;
        anim.SetBool("isAttack", false);
        anim.SetBool("isRunning", false);
    }

    public void ShowEffectSprite(Sprite effSprite, float duration)
    {
        if (effSprite == null || player == null) return;

        GameObject effObj = new GameObject("CardEffectOverlay");
        effObj.transform.SetParent(player.transform);
        effObj.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = effObj.AddComponent<SpriteRenderer>();
        sr.sprite = effSprite;
        sr.sortingOrder = 5;

        // Tự động tính toán để thu nhỏ ảnh vừa với nhân vật
        SpriteRenderer playerSr = player.GetComponent<SpriteRenderer>();
        if (playerSr != null && playerSr.sprite != null && effSprite.bounds.size.y > 0)
        {
            // Bóp ảnh hiệu ứng sao cho chiều cao của nó bằng 1.5 lần chiều cao ảnh nhân vật gốc
            float scaleRatio = (playerSr.sprite.bounds.size.y * 1.5f) / effSprite.bounds.size.y;
            effObj.transform.localScale = new Vector3(scaleRatio, scaleRatio, 1f);
        }
        else
        {
            effObj.transform.localScale = Vector3.one;
        }
        sr.sortingOrder = 5; 

        Destroy(effObj, duration);
    }

    private bool TrySpendMp(Card c)
    {
        if (player.mp < c.mpRequest)
        {
            GM.cardInfoLabel.text = string.Format("Thẻ này cần thêm {0} MP để sử dụng", c.mpRequest - player.mp);
            return false;
        }

        player.ChangeMP(-c.mpRequest);
        return true;
    }

    private float[] GetCardStats(Card c)
    {
        if (c.CardStats == null || c.CardStats.Count == 0)
        {
            return new float[0];
        }

        int index = Mathf.Clamp(c.lvStar - 1, 0, c.CardStats.Count - 1);
        return c.CardStats[index].values ?? new float[0];
    }

    private void ResolveReferences()
    {
        if (player == null)
        {
            player = GetComponent<UpdateStatePlayer>();
        }

        if (GM == null)
        {
            GM = GameObject.Find("GameManager")?.GetComponent<GameManagerS>();
        }

        if (statusLabel == null)
        {
            UIDocument doc = GM != null ? GM.documentP : null;
            if (doc != null)
            {
                statusLabel = doc.rootVisualElement.Q<Label>("StatusText");
            }
        }

        if (player == null)
        {
            Debug.LogWarning("Player reference is missing.");
        }
    }

    private void SetEffectTimer(string effectName, float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        effectTimers[effectName] = duration;
        UpdateStatusLabel();
    }

    private void UpdateEffectTimers()
    {
        if (effectTimers.Count == 0)
        {
            return;
        }

        List<string> keys = new List<string>(effectTimers.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            effectTimers[key] -= Time.deltaTime;
            if (effectTimers[key] <= 0f)
            {
                effectTimers.Remove(key);
            }
        }

        UpdateStatusLabel();
    }

    private void UpdateStatusLabel()
    {
        if (statusLabel == null)
        {
            return;
        }

        if (effectTimers.Count == 0)
        {
            statusLabel.text = string.Empty;
            return;
        }

        List<string> parts = new List<string>();
        foreach (KeyValuePair<string, float> pair in effectTimers)
        {
            int seconds = Mathf.CeilToInt(pair.Value);
            parts.Add($"{pair.Key}: {seconds}s");
        }

        statusLabel.text = string.Join(" | ", parts);
    }

    private IEnumerator HealOverTime(float[] stats, int lvStar)
    {
        if (stats.Length < 2)
        {
            yield break;
        }

        float percentPerSecond = stats[0];
        float duration = stats[1];
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (GM != null && GM.isPause)
            {
                yield return null;
                continue;
            }

            float delta = Time.deltaTime;
            player.ChangeHP(percentPerSecond * player.maxhp * delta);
            elapsed += delta;
            yield return null;
        }
    }

    private void ApplySpeedBoost(float[] stats)
    {
        if (stats.Length < 2)
        {
            return;
        }

        float multiplier = stats[0];
        float duration = stats[1];

        if (speedRoutine != null)
        {
            StopCoroutine(speedRoutine);
        }

        speedRoutine = StartCoroutine(TempSpeed(multiplier, duration));
    }

    private IEnumerator TempSpeed(float multiplier, float duration)
    {
        float original = player.speed;
        player.SetSpeed(original * multiplier);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (GM == null || !GM.isPause)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        player.SetSpeed(original);
    }

    private void ApplyPowerUp(float[] stats, int lvStar)
    {
        if (stats.Length < 2)
        {
            return;
        }

        float atkMultiplier = stats[0];
        float duration = stats[1];

        if (atkRoutine != null)
        {
            StopCoroutine(atkRoutine);
        }

        atkRoutine = StartCoroutine(TempPowerUp(atkMultiplier, duration, lvStar));
    }

    private IEnumerator TempPowerUp(float atkMultiplier, float duration, int lvStar)
    {
        float originalAtk = player.atk;
        float originalSatk = player.satk;

        player.atk = originalAtk * atkMultiplier;
        player.SetAttackSpeed(Mathf.Max(0.05f, originalSatk / 3f));
        
        if (lvStar >= 3)
        {
            player.isLifesteal = true;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (GM == null || !GM.isPause)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        player.atk = originalAtk;
        player.SetAttackSpeed(originalSatk);
        if (lvStar >= 3)
        {
            player.isLifesteal = false;
        }
    }

    private void ApplyBarrier(float[] stats)
    {
        if (stats.Length < 2)
        {
            return;
        }

        float reduction = stats[0];
        float duration = stats[1];

        if (barrierRoutine != null)
        {
            StopCoroutine(barrierRoutine);
        }

        barrierRoutine = StartCoroutine(TempBarrier(reduction, duration));
    }

    private IEnumerator TempBarrier(float reduction, float duration)
    {
        float multiplier = Mathf.Clamp01(1f - reduction);
        player.SetDamageTakenMultiplier(multiplier);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (GM == null || !GM.isPause)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        player.SetDamageTakenMultiplier(1f);
    }

    private void SpawnTornado(float[] stats)
    {
        if (Tornado == null)
        {
            Debug.LogWarning("Tornado prefab is missing.");
            return;
        }

        float duration = stats.Length > 1 ? stats[1] : 5f;
        float dmg = stats.Length > 0 ? stats[0] : 80f; // Lấy trực tiếp từ JSON

        GameObject instance = Instantiate(Tornado, player.transform.position, Quaternion.identity);
        TornadoBehavior tornado = instance.GetComponent<TornadoBehavior>();
        if (tornado != null)
        {
            tornado.Init(dmg, duration);
        }
        else
        {
            Destroy(instance, duration); // fallback
        }
    }

    private void SpawnIcySpear(float[] stats)
    {
        if (IcySpear == null)
        {
            Debug.LogWarning("Icy Spear prefab is missing.");
            return;
        }

        float dmg = stats.Length > 0 ? stats[0] : 30f;

        // IcySpearBehavior tự tính hướng từ mouse trong Start()
        GameObject instance = Instantiate(IcySpear, player.transform.position, Quaternion.identity);
        IcySpearBehavior spear = instance.GetComponent<IcySpearBehavior>();
        if (spear != null)
        {
            spear.Init(dmg, stats.Length > 1 ? stats[1] : 0.4f);
        }
    }

    private void SpawnMeteor(float[] stats)
    {
        if (Meteor == null)
        {
            Debug.LogWarning("Meteor prefab is missing.");
            return;
        }

        int count = stats.Length > 0 ? Mathf.RoundToInt(stats[0]) : 3;
        float dmg  = stats.Length > 1 ? stats[1] * player.atk : 40f; // 3,4,5 = 300%, 400%, 500% atk

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 4f;
            Vector3 targetPos = player.transform.position + new Vector3(offset.x, offset.y, 0f);

            GameObject instance = Instantiate(Meteor, targetPos + Vector3.up * 6f, Quaternion.identity);
            MeteorBehavior meteor = instance.GetComponent<MeteorBehavior>();
            if (meteor != null)
            {
                meteor.damage = dmg;
                meteor.SetTarget(targetPos);
            }
            else
            {
                // Fallback nếu chưa gán script
                Destroy(instance, 3f);
            }
        }
    }

    private void SpawnSwamp(float[] stats, int lvStar)
    {
        if (Swamp == null)
        {
            Debug.LogWarning("Swamp prefab is missing.");
            return;
        }

        if (currentSwamp != null)
        {
            Destroy(currentSwamp);
        }

        float slowPercent = stats.Length > 0 ? stats[0] : 0.5f;

        currentSwamp = Instantiate(Swamp, player.transform.position, Quaternion.identity);
        SwampBehavior swamp = currentSwamp.GetComponent<SwampBehavior>();
        if (swamp != null)
        {
            swamp.Init(slowPercent, lvStar);
        }
    }

    private IEnumerator BurnningBody(float[] stats, int lvStar)
    {
        if (stats.Length < 3)
        {
            yield break;
        }

        float selfDrainPercent = stats[0];
        float baseDmg = stats[1];
        float duration = stats[2];
        float elapsed = 0f;
        float tickTimer = 0f;

        while (elapsed < duration)
        {
            if (GM == null || !GM.isPause)
            {
                float delta = Time.deltaTime;
                elapsed += delta;
                tickTimer += delta;

                if (tickTimer >= 1f)
                {
                    tickTimer -= 1f;

                    if (player.hp <= player.maxhp * 0.02f)
                    {
                        break;
                    }

                    player.ChangeHP(-(player.hp * selfDrainPercent));

                    float totalDmg = baseDmg + (player.maxhp * 0.02f); // Giảm xuống 2% theo JSON mới
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Mob");
                    foreach (var e in enemies)
                    {
                        FollowTarget mob = e.GetComponent<FollowTarget>();
                        if (mob != null)
                        {
                            mob.TackenDame(totalDmg);
                        }
                    }
                }
            }
            yield return null;
        }
    }

    private void ApplyStarUp(float[] stats)
    {
        if (stats.Length < 2 || GM == null)
        {
            return;
        }

        int count = Mathf.RoundToInt(stats[0]);
        int stars = Mathf.RoundToInt(stats[1]);
        GM.ApplyStarUp(count, stars);
    }

    private void ApplyPoisonWeapon(float[] stats)
    {
        if (stats.Length < 2 || player == null) return;
        float duration = stats[0];
        float maxDmg = stats[1];
        player.SetPoisonWeapon(duration, maxDmg);
    }
}
