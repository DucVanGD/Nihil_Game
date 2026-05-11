using UnityEngine;

public class FireballBehavior : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 0f;
    private Vector3 direction;

    public void Init(Vector3 dir, float dmg)
    {
        direction = dir.normalized;
        damage = dmg;
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        
        // Cập nhật hướng xoay của fireball
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UpdateStatePlayer player = other.GetComponent<UpdateStatePlayer>();
            if (player != null)
            {
                player.taken_damage(damage);
                player.ApplyFireballBurn(2f);
            }
            Destroy(gameObject);
        }
    }
}
