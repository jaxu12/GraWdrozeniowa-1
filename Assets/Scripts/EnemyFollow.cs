using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour
{
    public float speed = 2f;
    public float facingThreshold = 0.01f;

    private Transform player;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }
    }

    private void FixedUpdate()
    {
        if (player == null) 
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // POPRAWKA: Poruszamy potwora przez Rigidbody, a nie przez transform.position!
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * speed;

        UpdateFacing();
    }

    private void UpdateFacing()
    {
        if (player == null) return;
        
        float deltaX = player.position.x - transform.position.x;

        if (Mathf.Abs(deltaX) < facingThreshold) return;

        transform.rotation = Quaternion.Euler(
            0f,
            deltaX < 0f ? 180f : 0f,
            0f
        );
    }
}