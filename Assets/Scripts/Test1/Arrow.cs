using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage;
    public float power;
    private Rigidbody2D rb;

    // 스프라이트 기본 방향이 velocity 반대일 때 보정 (Arrow_0는 왼쪽을 향함)
    private const float RotationOffset = 180f;

    private void FixedUpdate()
    {
        ApplyRotation(rb.linearVelocity);
    }

    private void ApplyRotation(Vector2 velocity)
    {
        if (velocity.sqrMagnitude < 0.01f) return;
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + RotationOffset;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void Shoot(float damage, float power, float direction)
    {
        this.damage = damage;
        this.power = power;
        float powerY = power > 50 ? 1f : 2f; // 빠른 화살은 높이 낮게
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        Vector2 velocity = new Vector2(power * direction, powerY);
        rb.linearVelocity = velocity;
        ApplyRotation(velocity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerBattle>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    public void ArrowStop()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        ArrowManager.instance.NewArrow(this);
    }
}
