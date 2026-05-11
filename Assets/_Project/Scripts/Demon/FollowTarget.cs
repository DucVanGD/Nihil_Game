using UnityEngine;
using System.Collections;
using System;

public class FollowTarget : MonoBehaviour
{
    [Header("Target")]
    public Transform Ptarget;
    public UpdateStatePlayer Starget;
    private GameManagerS GM;

    [Header("Stats")]
    public float rangeatk = 1.5f;
    private Animator anim;

    private float HP;
    private float maxHP;
    private float ATK;
    private float Satk;
    public float speed;
    private SimpleHPBar hpBar;

    private bool isTacken = false;
    public float recoverSpeed = 5f;
    private Vector2 knockbackVelocity = Vector2.zero;
    private const float KnockbackDecay = 10f;

    [Header("Poison Debuff")]
    private float currentPoisonDuration = 0f;
    private float poisonHpPercent = 0f;
    private float poisonMaxDmg = 0f;
    private float totalPoisonDamage = 0f;

    void Start()
    {
        StatsMob SM = GetComponent<StatsMob>();
        Ptarget = GameObject.Find("Player").GetComponent<Transform>();
        Starget = GameObject.Find("Player").GetComponent<UpdateStatePlayer>();
        GM = GameObject.Find("GameManager").GetComponent<GameManagerS>();
        anim = GetComponent<Animator>();
        anim.SetBool("isRunning", true);
        anim.SetBool("isDie", false);
        // Tự tính toán chỉ số dựa trên hệ số static để tránh lỗi thứ tự chạy Start()
        HP = 50f * StatsMob.HpMultiplier;
        maxHP = HP;
        ATK = 15f * StatsMob.AtkMultiplier;
        Satk = 2f;
        speed = 3f * StatsMob.SpeMultiplier;

        if (SettingGame.Instance != null && SettingGame.Instance.mobPowerUp)
        {
            ATK *= 1.5f;
            speed *= 1.2f;
        }

        hpBar = GetComponent<SimpleHPBar>();
        if (hpBar == null) hpBar = gameObject.AddComponent<SimpleHPBar>();
        hpBar.yOffset = 1.0f; // Cao hơn quái 1 chút
    }

    void Update()
    {
        if (!GM.isPause)
        {
            Vector3 direction = Ptarget.position - transform.position;
            direction.z = 0;
            if (direction.magnitude > rangeatk)
            {
                anim.SetBool("isAttack", false);
                anim.SetBool("isRunning", true);
            }
            else
            {
                anim.SetBool("isRunning", false);
                anim.SetBool("isAttack", true);
                if (Satk <= 0)
                {
                    attacktarget();
                    Satk = 2f;
                }
                else
                {
                    Satk -= Time.deltaTime;
                }

            }
            direction.Normalize();
            transform.position += direction * speed * Time.deltaTime;

            // Knockback velocity decay — tự giảm dần về 0
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, Time.deltaTime * KnockbackDecay);
            transform.position += (Vector3)(knockbackVelocity * Time.deltaTime);

            if (isTacken)
            {
                float angle = Mathf.LerpAngle(transform.eulerAngles.z, 0f, Time.deltaTime * recoverSpeed);
                transform.rotation = Quaternion.Euler(0f, 0f, angle);

                if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0f)) < 1f)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    isTacken = false;
                }
            }

            if (currentPoisonDuration > 0)
            {
                GetComponent<SpriteRenderer>().color = Color.green; // Đổi màu xanh lá để báo hiệu đang dính độc
                currentPoisonDuration -= Time.deltaTime;
                float dmg = (maxHP * poisonHpPercent) * Time.deltaTime; 
                if (totalPoisonDamage + dmg > poisonMaxDmg)
                {
                    dmg = poisonMaxDmg - totalPoisonDamage;
                    currentPoisonDuration = 0;
                }
                if (dmg > 0)
                {
                    HP -= dmg;
                    totalPoisonDamage += dmg;
                    if (hpBar != null) hpBar.UpdateHP(HP, maxHP);
                    if (HP <= 0 && !anim.GetBool("isDie"))
                    {
                        anim.SetBool("isDie", true);
                        StartCoroutine(Die("Player"));
                    }
                }
            }
            else
            {
                GetComponent<SpriteRenderer>().color = Color.white; // Trở lại màu bình thường khi hết độc
            }
        }

    }


    // Được gọi từ Slashrun khi đòn chém trúng
    public void SetKnockedBack(Vector2 direction, float force)
    {
        isTacken = true;
        knockbackVelocity = direction.normalized * force;
    }

    public void ApplyPoison(float hpPercent, float duration, float maxDmg)
    {
        currentPoisonDuration = duration;
        poisonHpPercent = hpPercent;
        poisonMaxDmg = maxDmg;
        totalPoisonDamage = 0f;
    }

    public void TackenDame(float dame, string sourceTag = "Player")
    {
        HP -= dame;
        if (hpBar != null) hpBar.UpdateHP(HP, maxHP);
        if (HP <= 0)
        {
            anim.SetBool("isDie", true);
            StartCoroutine(Die(sourceTag));
        }
    }

    IEnumerator Die(string sourceTag)
    {
        yield return new WaitForSeconds(0.5f);
        if (sourceTag != "Boss")
        {
            int r = UnityEngine.Random.Range(0, 100);
            if (r < 10) Starget.ChangeHP(10);
            GM.mhp += 1;
        }
        else
        {
            BallonBoss boss = UnityEngine.Object.FindFirstObjectByType<BallonBoss>();
            if (boss != null) boss.OnMobKilled();
        }
        Destroy(gameObject);
    }
    void attacktarget()
    {
        if (anim.GetBool("isAttack"))
        {
            Starget.taken_damage(ATK);
        }
    }
}
