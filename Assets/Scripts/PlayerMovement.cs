using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float visualScale = 0.5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.right;
    private Animator animator;
    private PlayerStatsRuntime playerStats;
    private bool isShooting;

    public Vector2 FacingDirection => facingDirection.sqrMagnitude > 0.01f ? facingDirection.normalized : Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStatsRuntime>();

        if (playerStats == null)
        {
            playerStats = gameObject.AddComponent<PlayerStatsRuntime>();
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input.normalized;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            facingDirection = moveInput;
        }
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * playerStats.MoveSpeed;

        if (facingDirection.x > 0.01f)
        {
            transform.localScale = new Vector3(visualScale, visualScale, 1f);
        }
        else if (facingDirection.x < -0.01f)
        {
            transform.localScale = new Vector3(-visualScale, visualScale, 1f);
        }
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        facingDirection = direction.normalized;
    }

    private void UpdateAnimation()
    {
        if (animator == null || isShooting)
        {
            return;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (isMoving)
        {
            if (!stateInfo.IsName("Archer_Run"))
            {
                animator.Play("Archer_Run");
            }
        }
        else if (!stateInfo.IsName("Archer_Idle"))
        {
            animator.Play("Archer_Idle");
        }
    }

    public void PlayShootAnimation()
    {
        if (animator == null)
        {
            return;
        }

        isShooting = true;
        animator.Play("Archer_Shoot");
        CancelInvoke(nameof(ResetShootFlag));
        Invoke(nameof(ResetShootFlag), 0.16f);
    }

    private void ResetShootFlag()
    {
        isShooting = false;
    }
}
