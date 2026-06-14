using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerArsenal : MonoBehaviour
{
    private static Sprite pulseSprite;

    private class WeaponState
    {
        public WeaponType Type;
        public int Level;
        public float Cooldown;
    }

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float autoAimRange = 20f;
    [SerializeField] private float spawnOffsetDistance = 0.9f;
    [SerializeField] private float spreadAngleStep = 10f;
    [SerializeField] private float orbitPulseVisualDuration = 0.16f;

    private readonly Dictionary<WeaponType, WeaponState> weapons = new Dictionary<WeaponType, WeaponState>();
    private readonly List<WeaponType> weaponOrder = new List<WeaponType>();

    private Collider2D playerCollider;
    private PlayerMovement playerMovement;
    private PlayerStatsRuntime playerStats;

    public int WeaponCount => weaponOrder.Count;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStatsRuntime>();
        if (playerStats == null)
        {
            playerStats = gameObject.AddComponent<PlayerStatsRuntime>();
        }

        playerCollider = GetComponent<Collider2D>();

        if (playerMovement != null && bulletPrefab == null)
        {
            bulletPrefab = playerMovement.bulletPrefab;
        }

        EnsureStarterWeapon();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsOverlayOpen)
        {
            return;
        }

        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStatsRuntime>();
            if (playerStats == null)
            {
                return;
            }
        }

        float deltaTime = Time.deltaTime;
        for (int i = 0; i < weaponOrder.Count; i++)
        {
            WeaponState state = weapons[weaponOrder[i]];
            state.Cooldown -= deltaTime;

            switch (state.Type)
            {
                case WeaponType.Bow:
                    UpdateBow(state);
                    break;
                case WeaponType.ArcBurst:
                    UpdateArcBurst(state);
                    break;
                case WeaponType.PierceShot:
                    UpdatePierceShot(state);
                    break;
                case WeaponType.AuraPulse:
                    UpdateAuraPulse(state);
                    break;
            }
        }
    }

    public bool HasWeapon(WeaponType weapon)
    {
        return weapons.ContainsKey(weapon);
    }

    public int GetWeaponLevel(WeaponType weapon)
    {
        return weapons.TryGetValue(weapon, out WeaponState state) ? state.Level : 0;
    }

    public bool CanOfferWeapon(WeaponType weapon)
    {
        return !HasWeapon(weapon) && playerStats != null && WeaponCount < playerStats.MaxWeaponSlots;
    }

    public bool CanUpgradeWeapon(WeaponType weapon)
    {
        return HasWeapon(weapon) && GetWeaponLevel(weapon) < GetMaxLevel(weapon);
    }

    public bool AddOrUpgradeWeapon(WeaponType weapon, int levels = 1)
    {
        if (!weapons.TryGetValue(weapon, out WeaponState state))
        {
            if (playerStats == null || WeaponCount >= playerStats.MaxWeaponSlots)
            {
                return false;
            }

            state = new WeaponState
            {
                Type = weapon,
                Level = 0,
                Cooldown = 0f
            };
            weapons[weapon] = state;
            weaponOrder.Add(weapon);
        }

        state.Level = Mathf.Clamp(state.Level + Mathf.Max(1, levels), 1, GetMaxLevel(weapon));
        return true;
    }

    public IReadOnlyList<WeaponType> GetWeaponOrder()
    {
        return weaponOrder;
    }

    public static string GetDisplayName(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Bow:
                return "Hunter Bow";
            case WeaponType.ArcBurst:
                return "Arc Burst";
            case WeaponType.PierceShot:
                return "Drill Shot";
            case WeaponType.AuraPulse:
                return "Aura Pulse";
            default:
                return weapon.ToString();
        }
    }

    public static string GetDescription(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Bow:
                return "fast auto-shot toward nearest enemy";
            case WeaponType.ArcBurst:
                return "fires a radial burst around the player";
            case WeaponType.PierceShot:
                return "heavy piercing shot through aligned enemies";
            case WeaponType.AuraPulse:
                return "pulse damage around the player";
            default:
                return "weapon upgrade";
        }
    }

    public static int GetMaxLevel(WeaponType weapon)
    {
        return weapon == WeaponType.Bow ? 6 : 5;
    }

    private void EnsureStarterWeapon()
    {
        if (!HasWeapon(WeaponType.Bow))
        {
            AddOrUpgradeWeapon(WeaponType.Bow, 1);
        }
    }

    private void UpdateBow(WeaponState state)
    {
        if (state.Cooldown > 0f)
        {
            return;
        }

        Vector2 direction = GetAimDirection();
        if (direction == Vector2.zero)
        {
            return;
        }

        int burstCount = 1 + playerStats.ProjectileCountBonus;
        if (state.Level >= 3)
        {
            burstCount++;
        }

        if (state.Level >= 5)
        {
            burstCount++;
        }

        float cooldown = Mathf.Max(0.16f, 0.46f * playerStats.CooldownMultiplier - (state.Level - 1) * 0.025f);
        state.Cooldown = cooldown;
        playerMovement?.PlayShootAnimation();

        ShootBurst(
            direction,
            burstCount,
            Mathf.RoundToInt((2f + (state.Level - 1) * 0.7f) * playerStats.DamageMultiplier),
            0.45f * playerStats.ProjectileSizeMultiplier,
            18f,
            0,
            2f,
            new Color(1f, 1f, 1f, 1f)
        );
    }

    private void UpdateArcBurst(WeaponState state)
    {
        if (state.Cooldown > 0f)
        {
            return;
        }

        int projectileCount = 6 + state.Level * 2 + playerStats.ProjectileCountBonus;
        int damage = Mathf.RoundToInt((2.4f + state.Level * 0.85f) * playerStats.DamageMultiplier);
        float scale = 0.33f * playerStats.ProjectileSizeMultiplier;
        float angleStep = 360f / projectileCount;
        state.Cooldown = Mathf.Max(0.7f, 2.4f * playerStats.CooldownMultiplier - state.Level * 0.12f);
        playerMovement?.PlayShootAnimation();

        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 direction = Quaternion.Euler(0f, 0f, i * angleStep) * Vector2.right;
            SpawnBullet(direction, damage, scale, 12.5f, 0, 1.2f, new Color(0.4f, 0.95f, 1f, 1f));
        }
    }

    private void UpdatePierceShot(WeaponState state)
    {
        if (state.Cooldown > 0f)
        {
            return;
        }

        Vector2 direction = GetAimDirection();
        if (direction == Vector2.zero)
        {
            return;
        }

        int lines = 1 + (state.Level >= 3 ? 1 : 0) + (state.Level >= 5 ? 1 : 0);
        int pierce = 2 + state.Level / 2;
        int damage = Mathf.RoundToInt((4.2f + state.Level * 1.3f) * playerStats.DamageMultiplier);
        float scale = 0.38f * playerStats.ProjectileSizeMultiplier;
        state.Cooldown = Mathf.Max(1.1f, 2.05f * playerStats.CooldownMultiplier - state.Level * 0.1f);
        playerMovement?.PlayShootAnimation();

        ShootBurst(
            direction,
            lines,
            damage,
            scale,
            22f,
            pierce,
            2.7f,
            new Color(1f, 0.86f, 0.35f, 1f)
        );
    }

    private void UpdateAuraPulse(WeaponState state)
    {
        if (state.Cooldown > 0f)
        {
            return;
        }

        float radius = (1.8f + state.Level * 0.4f) * playerStats.AreaMultiplier;
        int damage = Mathf.RoundToInt((2f + state.Level * 0.8f) * playerStats.DamageMultiplier);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        bool dealtDamage = false;

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].CompareTag("Enemy"))
            {
                continue;
            }

            Health health = hits[i].GetComponent<Health>();
            if (health == null)
            {
                continue;
            }

            health.TakeDamage(damage);
            dealtDamage = true;
        }

        if (dealtDamage)
        {
            SpawnPulseVisual(radius);
        }

        state.Cooldown = Mathf.Max(1.2f, 3.1f * playerStats.CooldownMultiplier - state.Level * 0.14f);
    }

    private void ShootBurst(Vector2 centerDirection, int projectileCount, int damage, float scale, float speed, int pierce, float lifetime, Color color)
    {
        float centerOffset = (projectileCount - 1) * 0.5f;
        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = (i - centerOffset) * spreadAngleStep;
            Vector2 direction = Quaternion.Euler(0f, 0f, angleOffset) * centerDirection;
            SpawnBullet(direction.normalized, damage, scale, speed, pierce, lifetime, color);
        }
    }

    private void SpawnBullet(Vector2 direction, int damage, float scale, float speed, int pierce, float lifetime, Color color)
    {
        if (bulletPrefab == null)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        Vector3 spawnPosition = transform.position + (Vector3)(direction * spawnOffsetDistance);
        GameObject projectile = Instantiate(bulletPrefab, spawnPosition, rotation);
        projectile.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 0f);

        Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
        if (projectileCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(projectileCollider, playerCollider);
        }

        Bullet bullet = projectile.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Configure(damage, scale, speed, pierce, lifetime, color);
            bullet.SetDirection(direction);
        }
    }

    private Vector2 GetAimDirection()
    {
        Transform target = FindNearestEnemy();
        if (target != null)
        {
            Vector2 aimDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            playerMovement?.SetFacingDirection(aimDirection);
            return aimDirection;
        }

        return playerMovement != null ? playerMovement.FacingDirection : Vector2.right;
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;
        float closestDistance = autoAimRange * autoAimRange;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null || !enemy.activeInHierarchy)
            {
                continue;
            }

            float sqrDistance = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance >= closestDistance)
            {
                continue;
            }

            closestDistance = sqrDistance;
            closestEnemy = enemy.transform;
        }

        return closestEnemy;
    }

    private void SpawnPulseVisual(float radius)
    {
        GameObject visual = new GameObject("AuraPulseVisual");
        visual.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPulseSprite();
        renderer.color = new Color(0.35f, 0.95f, 0.75f, 0.4f);
        renderer.sortingOrder = 8;
        visual.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
        Destroy(visual, orbitPulseVisualDuration);
    }

    private static Sprite GetPulseSprite()
    {
        if (pulseSprite != null)
        {
            return pulseSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color32[] pixels = new Color32[size * size];
        float center = (size - 1) * 0.5f;
        float outer = size * 0.42f;
        float inner = size * 0.3f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = dx * dx + dy * dy;
                bool ring = dist <= outer * outer && dist >= inner * inner;
                pixels[y * size + x] = ring ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        pulseSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return pulseSprite;
    }
}
