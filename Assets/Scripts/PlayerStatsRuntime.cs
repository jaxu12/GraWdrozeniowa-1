using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStatsRuntime : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseMoveSpeed = 16f;
    [SerializeField] private float baseCooldownMultiplier = 1f;
    [SerializeField] private float baseDamageMultiplier = 1f;
    [SerializeField] private float baseProjectileSizeMultiplier = 1f;
    [SerializeField] private int baseProjectileCountBonus = 0;
    [SerializeField] private int baseMaxHp = 5;
    [SerializeField] private float basePickupRange = 2.5f;
    [SerializeField] private float baseAreaMultiplier = 1f;
    [SerializeField] private int maxWeaponSlots = 4;
    [SerializeField] private int maxPassiveSlots = 4;

    private readonly Dictionary<SkillUpgrade, int> statUpgradeRanks = new Dictionary<SkillUpgrade, int>();
    private readonly Dictionary<PassiveType, int> passiveRanks = new Dictionary<PassiveType, int>();
    private float recoveryAccumulator;

    public float MoveSpeed { get; private set; }
    public float CooldownMultiplier { get; private set; }
    public float DamageMultiplier { get; private set; }
    public float ProjectileSizeMultiplier { get; private set; }
    public int ProjectileCountBonus { get; private set; }
    public int MaxHp { get; private set; }
    public float PickupRange { get; private set; }
    public float AreaMultiplier { get; private set; }
    public float RecoveryPerSecond { get; private set; }
    public int MaxWeaponSlots => Mathf.Max(1, maxWeaponSlots);
    public int MaxPassiveSlots => Mathf.Max(1, maxPassiveSlots);

    private void Awake()
    {
        ResetToBaseStats();
    }

    private void Update()
    {
        if (RecoveryPerSecond <= 0f || GameManager.Instance != null && GameManager.Instance.IsOverlayOpen)
        {
            return;
        }

        Health health = GetComponent<Health>();
        if (health == null || health.CurrentHp >= health.MaxHp)
        {
            return;
        }

        recoveryAccumulator += RecoveryPerSecond * Time.deltaTime;
        if (recoveryAccumulator < 1f)
        {
            return;
        }

        int healAmount = Mathf.FloorToInt(recoveryAccumulator);
        recoveryAccumulator -= healAmount;
        health.Heal(healAmount);
    }

    public void ResetToBaseStats()
    {
        MoveSpeed = baseMoveSpeed;
        CooldownMultiplier = baseCooldownMultiplier;
        DamageMultiplier = baseDamageMultiplier;
        ProjectileSizeMultiplier = baseProjectileSizeMultiplier;
        ProjectileCountBonus = baseProjectileCountBonus;
        MaxHp = baseMaxHp;
        PickupRange = basePickupRange;
        AreaMultiplier = baseAreaMultiplier;
        RecoveryPerSecond = 0f;
        recoveryAccumulator = 0f;

        statUpgradeRanks.Clear();
        passiveRanks.Clear();
    }

    public int GetStatRank(SkillUpgrade upgrade)
    {
        return statUpgradeRanks.TryGetValue(upgrade, out int rank) ? rank : 0;
    }

    public int GetPassiveRank(PassiveType passive)
    {
        return passiveRanks.TryGetValue(passive, out int rank) ? rank : 0;
    }

    public bool CanUpgradeStat(SkillUpgrade upgrade, int maxRank)
    {
        return GetStatRank(upgrade) < maxRank;
    }

    public bool HasPassive(PassiveType passive)
    {
        return passiveRanks.ContainsKey(passive);
    }

    public bool CanAddPassive(PassiveType passive)
    {
        return !HasPassive(passive) && passiveRanks.Count < MaxPassiveSlots;
    }

    public bool CanUpgradePassive(PassiveType passive, int maxRank)
    {
        if (!HasPassive(passive))
        {
            return passiveRanks.Count < MaxPassiveSlots;
        }

        return GetPassiveRank(passive) < maxRank;
    }

    public IReadOnlyDictionary<PassiveType, int> GetPassiveRanks()
    {
        return passiveRanks;
    }

    public void ApplyStatUpgrade(SkillUpgrade upgrade, int stacks = 1)
    {
        Health health = GetComponent<Health>();

        for (int i = 0; i < stacks; i++)
        {
            statUpgradeRanks[upgrade] = GetStatRank(upgrade) + 1;

            switch (upgrade)
            {
                case SkillUpgrade.DamageUp:
                    DamageMultiplier += 0.18f;
                    break;
                case SkillUpgrade.FireRateUp:
                    CooldownMultiplier = Mathf.Max(0.45f, CooldownMultiplier - 0.08f);
                    break;
                case SkillUpgrade.ProjectileSizeUp:
                    ProjectileSizeMultiplier += 0.12f;
                    break;
                case SkillUpgrade.ProjectileCountUp:
                    ProjectileCountBonus = Mathf.Min(4, ProjectileCountBonus + 1);
                    break;
                case SkillUpgrade.MoveSpeedUp:
                    MoveSpeed += 1.25f;
                    break;
                case SkillUpgrade.MaxHpUp:
                    MaxHp += 2;
                    if (health != null)
                    {
                        health.SetMaxHp(MaxHp, 2);
                    }
                    break;
                case SkillUpgrade.PickupRangeUp:
                    PickupRange += 1.1f;
                    break;
            }
        }
    }

    public void ApplyPassive(PassiveType passive, int stacks = 1)
    {
        Health health = GetComponent<Health>();

        for (int i = 0; i < stacks; i++)
        {
            passiveRanks[passive] = GetPassiveRank(passive) + 1;

            switch (passive)
            {
                case PassiveType.PowerCore:
                    DamageMultiplier += 0.12f;
                    break;
                case PassiveType.QuickHands:
                    CooldownMultiplier = Mathf.Max(0.35f, CooldownMultiplier - 0.07f);
                    break;
                case PassiveType.HeavyDraw:
                    ProjectileSizeMultiplier += 0.15f;
                    AreaMultiplier += 0.08f;
                    break;
                case PassiveType.SwiftBoots:
                    MoveSpeed += 1.1f;
                    break;
                case PassiveType.HollowHeart:
                    MaxHp += 3;
                    RecoveryPerSecond += 0.25f;
                    if (health != null)
                    {
                        health.SetMaxHp(MaxHp, 2);
                    }
                    break;
                case PassiveType.MagnetCharm:
                    PickupRange += 1.35f;
                    break;
            }
        }
    }
}
