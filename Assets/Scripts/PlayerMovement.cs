using UnityEngine;
using UnityEngine.InputSystem; // Nowy system sterowania

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    
    // --- Miejsce na Twój prefab strzały ---
    public GameObject bulletPrefab; 

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator; 
    private Collider2D playerCollider; // Zapamiętamy collider gracza, żeby strzały w nas nie bębniły
    
    // Zmienne do obsługi strzelania
    private bool isShooting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        playerCollider = GetComponent<Collider2D>(); // Pobieramy collider łucznika
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
        // 1. Ruch (Zostaje w FixedUpdate, bo to fizyka)
        rb.linearVelocity = moveInput * moveSpeed;

        // 2. Obracanie (Flip)
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
        // 1. Strzał przy użyciu NOWEGO Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !isShooting)
        {
            isShooting = true;
            animator.Play("Archer_Shoot");
            
            // --- WYWOŁANIE STRZAŁU ---
            ShootArrow();
            
            // Wywołujemy funkcję resetującą strzał po 0.4 sekundy
            Invoke("ResetShooting", 0.4f); 
        }

        // 2. Jeśli strzelamy, blokujemy animacje biegu/stania
        if (isShooting) return;

        // 3. Normalny ruch i stanie
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (isMoving)
        {
            if (!stateInfo.IsName("Archer_Run"))
            {
                animator.Play("Archer_Run");
            }
        }
        else
        {
            if (!stateInfo.IsName("Archer_Idle"))
            {
                animator.Play("Archer_Idle");
            }
        }
    }

    // --- Logika celowania i wystrzału ---
    private void ShootArrow()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Nie przypisałeś prefabu strzały (Arrow) do skryptu PlayerMovement!");
            return;
        }

        // 1. Pobieramy pozycję myszy na ekranie i zamieniamy ją na pozycję w świecie gry
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = 10f; 
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // 2. Obliczamy kierunek od łucznika do myszki
        Vector2 shootDirection = (mouseWorldPosition - transform.position).normalized;

        // 3. Obliczamy kąt obrotu strzały w osi Z
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        Quaternion arrowRotation = Quaternion.Euler(0, 0, angle);

        // 4. Tworzymy fizycznie strzałę na pozycji gracza
        GameObject arrowElement = Instantiate(bulletPrefab, transform.position, arrowRotation);

        // --- WYMUSZAMY IDEALNE Z = 0 ---
        arrowElement.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        // --- KLUCZOWA POPRAWKA: Ignorowanie zderzeń między strzałą a graczem ---
        Collider2D arrowCollider = arrowElement.GetComponent<Collider2D>();
        if (arrowCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(arrowCollider, playerCollider);
        }

        // 5. Przekazujemy kierunek lotu do skryptu Bullet
        Bullet bulletScript = arrowElement.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDirection(shootDirection);
        }
        else
        {
            // Próba ratunkowa, jeśli edytor zgubił komponent
            bulletScript = arrowElement.AddComponent<Bullet>();
            bulletScript.SetDirection(shootDirection);
        }
        
        // 6. Obróć automatycznie postać gracza w stronę, w którą strzela
        if (shootDirection.x > 0)
        {
            transform.localScale = new Vector3(0.7f, 0.7f, 1);
        }
        else if (shootDirection.x < 0)
        {
            transform.localScale = new Vector3(-0.7f, 0.7f, 1);
        }
    }

    private void ResetShooting()
    {
        isShooting = false;
    }
}