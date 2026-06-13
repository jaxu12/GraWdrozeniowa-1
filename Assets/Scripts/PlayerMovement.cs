using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject bulletPrefab; 

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator; 
    private Collider2D playerCollider; 
    
    private bool isShooting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        playerCollider = GetComponent<Collider2D>(); 
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input.normalized;
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;

        if (!isShooting)
        {
            if (moveInput.x > 0) 
            {
                transform.localScale = new Vector3(0.7f, 0.7f, 1);
            }
            else if (moveInput.x < 0) 
            {
                transform.localScale = new Vector3(-0.7f, 0.7f, 1);
            }
        }
    }

    private void UpdateAnimation()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !isShooting)
        {
            isShooting = true;
            animator.Play("Archer_Shoot");
            ShootArrow();
            Invoke("ResetShooting", 0.4f); 
        }

        if (isShooting) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (isMoving)
        {
            if (!stateInfo.IsName("Archer_Run")) animator.Play("Archer_Run");
        }
        else
        {
            if (!stateInfo.IsName("Archer_Idle")) animator.Play("Archer_Idle");
        }
    }

    private void ShootArrow()
    {
        if (bulletPrefab == null) return;

        // 1. Pobieramy pozycję myszy
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = 10f; 
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // 2. Obliczamy kierunek
        Vector2 shootDirection = (mouseWorldPosition - transform.position).normalized;
        if (shootDirection == Vector2.zero) shootDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // 3. Obliczamy rotację
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        Quaternion arrowRotation = Quaternion.Euler(0, 0, angle);

        // --- KLUCZOWA POPRAWKA: Odsunięcie punktu spawnu o 1 jednostkę przed gracza ---
        Vector3 spawnOffset = (Vector3)shootDirection * 1.0f;
        Vector3 spawnPosition = transform.position + spawnOffset;

        // 4. Tworzymy strzałę w bezpiecznym miejscu
        GameObject arrowElement = Instantiate(bulletPrefab, spawnPosition, arrowRotation);
        arrowElement.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 0f);

        // Wyłączamy kolizję na wszelki wypadek
        Collider2D arrowCollider = arrowElement.GetComponent<Collider2D>();
        if (arrowCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(arrowCollider, playerCollider);
        }

        // 5. Nadajemy prędkość
        Bullet bulletScript = arrowElement.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDirection);
        }
        
        // 6. Obrót gracza
        if (shootDirection.x > 0) transform.localScale = new Vector3(0.7f, 0.7f, 1);
        else if (shootDirection.x < 0) transform.localScale = new Vector3(-0.7f, 0.7f, 1);
    }

    private void ResetShooting()
    {
        isShooting = false;
    }
}