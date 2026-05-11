using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UpdateStatePlayer : MonoBehaviour
{
    [Header("Stats")]
    public float BaseHp = 150f;
    public float BaseAtk = 30f;
    public float BaseMp = 120f;
    public float BaseSatk = 2f;    
    public float BaseSpeed = 4f;

    [Header("Current State")]
    public float hp;
    public float maxhp;
    public float mp;
    public float atk;
    public float satk;
    public float speed;
    private float damageTakenMultiplier = 1f;

    [Header("Poison Weapon State")]
    public bool isPoisonWeapon = false;
    public float poisonHpPercent = 0.05f;
    public float poisonDuration = 0f;
    public float poisonMaxDmg = 0f;
    public bool isLifesteal = false;
    private Coroutine poisonRoutine;
    private Coroutine burnRoutine;

    [Header("UI Document")]
    public UIDocument document;

    private ProgressBar hpBar;
    private ProgressBar mpBar;
    private VisualElement root;
    private RandomStats RS;

    public void AddStats()
    {
        RandomStats();
        maxhp = BaseHp;
        hp = maxhp;
        atk = BaseAtk;
        mp = 0f;
        satk = BaseSatk;
        speed = BaseSpeed;
    }

    void Start()
    {
        RS = GetComponent<RandomStats>();
        AddStats();
        if (document != null)
        {
            root = document.rootVisualElement;

            hpBar = root.Q<ProgressBar>("HPBar");
            mpBar = root.Q<ProgressBar>("MPBar");

            UpdateHPBar();
            UpdateMPBar();
        } 

    }

    void Update()
    {
        UpdateHPBar();
        UpdateMPBar();
    }

    private void RandomStats()
    {
        SettingGame SG = SettingGame.Instance;
        if (SG == null) Debug.Log("Khong co SG");
        if (SG != null && SG.randomstats && RS != null)
        {
            RS.GenerateRandomStats();
            BaseHp = RS.NewHp;
            BaseAtk = RS.NewAtk;
            BaseMp = RS.NewMp;
        }
    }
    public void ChangeHP(float amount)
    {
        hp = Mathf.Clamp(hp + amount, 0, BaseHp);
        UpdateHPBar();
    }

    public void ChangeMP(float amount)
    {
        mp = Mathf.Clamp(mp + amount, 0, BaseMp);
        UpdateMPBar();
    }

    public void SetSpeed(float value)
    {
        speed = value;
    }

    public void SetAttackSpeed(float value)
    {
        satk = value;
    }

    private void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.value = (hp / BaseHp) * 100f;
            hpBar.title = $"HP: {Mathf.RoundToInt(hp)}";
        }
    }

    private void UpdateMPBar()
    {
        if (mpBar != null)
        {
            mpBar.value = (mp / BaseMp) * 100f;
            mpBar.title = $"MP: {Mathf.RoundToInt(mp)}";
        }
    }

    public void taken_damage(float damage)
    {
        ChangeHP(-damage * damageTakenMultiplier);
    }

    public void SetDamageTakenMultiplier(float multiplier)
    {
        damageTakenMultiplier = Mathf.Max(0f, multiplier);
    }
    public void RegenMP()
    {
        ChangeMP(BaseMp / 60);
    }

    public void PlayerUp()
    {
        Debug.Log("LV uppu");
        BaseHp *= 1.09f;
        BaseSatk *= 0.95f;
        BaseSpeed *= 1.02f;
        BaseAtk *= 1.1f;
        ChangeHP(0.08f * BaseHp);
    }

    public void SetPoisonWeapon(float duration, float maxDmg)
    {
        if (poisonRoutine != null) StopCoroutine(poisonRoutine);
        poisonRoutine = StartCoroutine(PoisonWeaponTimer(duration, maxDmg));
    }

    private System.Collections.IEnumerator PoisonWeaponTimer(float duration, float maxDmg)
    {
        isPoisonWeapon = true;
        poisonDuration = duration;
        poisonMaxDmg = maxDmg;
        yield return new WaitForSeconds(duration);
        isPoisonWeapon = false;
    }

    public void ApplyFireballBurn(float duration)
    {
        if (burnRoutine != null) StopCoroutine(burnRoutine);
        burnRoutine = StartCoroutine(FireballBurnCoroutine(duration));
    }

    private System.Collections.IEnumerator FireballBurnCoroutine(float duration)
    {
        float elapsed = 0f;
        float tickTimer = 0f;
        
        UseCard uc = GetComponent<UseCard>();
        if (uc != null && uc.Burn != null)
        {
            uc.ShowEffectSprite(uc.Burn, duration);
        }

        while (elapsed < duration)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            tickTimer += dt;
            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                // Trừ 3% HP tối đa
                ChangeHP(-(maxhp * 0.03f));
            }
            yield return null;
        }
    }
}
