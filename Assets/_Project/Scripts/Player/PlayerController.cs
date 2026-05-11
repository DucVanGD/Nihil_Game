using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private Animator am;
    private float nexttime = 0f;
    private bool isDead = false;

    public Vector3 velocity;
    public GameObject bulletPrefab;

    private float map_x = 32;
    private float map_y = 18;

    private UpdateStatePlayer state;
    private GameManagerS GM;


    void Start()
    {
        am = GetComponent<Animator>();
        transform.localScale = new Vector3(4f, 4f, 1f);
        state = GetComponent<UpdateStatePlayer>();
        
        GM = FindFirstObjectByType<GameManagerS>();
        if (GM == null)
        {
            Debug.LogWarning("[PlayerController] Không tìm thấy GameManagerS!");
        }
        
        StartCoroutine(RegenMPCoroutine());
    }

    public void ResetPlayer()
    {
        isDead = false;
        if (am != null)
        {
            am.SetBool("isDie", false);
            am.SetBool("isRunning", false);
            am.SetBool("isAttack", false);
        }
    }

    void Update()
    {
        if (!Die() && !GM.isPause)
        {
            bool run = am.GetBool("isRunning");
            HandleMovement();

            bool overUI = GameManagerS.IsPointerOverUI;
            if (!overUI)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (!run && GM.timePlay > nexttime)
                {
                    am.SetBool("isAttack", true);
                    StartCoroutine(Attack(mousePosition));
                    nexttime = GM.timePlay + state.satk;
                }
                RotateToMouse(mousePosition);
            }
        }
    }


    void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 velocity = new Vector3(inputX, inputY, 0).normalized * state.speed;

        Running(velocity);
        Vector3 newPos = transform.position + velocity * Time.deltaTime;

        newPos.x = Mathf.Clamp(newPos.x, -map_x, map_x);
        newPos.y = Mathf.Clamp(newPos.y, -map_y, map_y);

        transform.position = newPos;
    }

    void Running(Vector3 velocity)
    {
        am.SetBool("isRunning", velocity.magnitude > 0);
    }

    void RotateToMouse(Vector3 mousePosition)
    {
        if (mousePosition.x < transform.position.x)
        {
            transform.localScale = new Vector3(-4f, 4f, 1f);
        }
        else if (mousePosition.x > transform.position.x)
        {
            transform.localScale = new Vector3(4f, 4f, 1f);
        }
    }

    IEnumerator Attack(Vector3 mousePosition)
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Shot");
        // Zero out z trước khi tính hướng — ScreenToWorldPoint trả về z ≈ -9.7
        // khiến normalized vector có z-component làm giảm tốc độ 2D thực tế
        mousePosition.z = 0f;
        Vector3 fireDirection = (mousePosition - transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        // Độc chỉ kéo dài 3 giây (3 phát giật) trên quái, không dùng poisonDuration của buff
        bullet.GetComponent<Slashrun>().SetDirection(fireDirection, state.atk, state.isPoisonWeapon, state.poisonHpPercent, 3f, state.poisonMaxDmg, state.isLifesteal);
        am.SetBool("isAttack", false);
    }

    private bool Die()
    {
        if (state.hp <= 0)
        {
            if (!isDead)
            {
                isDead = true;
                am.SetBool("isDie", true);
                StartCoroutine(WaitToDie());
            }
            return true;
        }
        return false;
    }

    private IEnumerator WaitToDie()
    {
        yield return new WaitForSeconds(2f);
        if (GM != null) GM.isEnd = true;
    }

    private IEnumerator RegenMPCoroutine()
    {
        while (true)
        {
            if (!GM.isPause)
            {
                state.RegenMP();
            }

            yield return new WaitForSeconds(1f);
        }
    }
}
