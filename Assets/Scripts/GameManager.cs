using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private enum ChoiceKind
    {
        NewWeapon,
        UpgradeWeapon,
        NewPassive,
        UpgradePassive,
        StatUpgrade
    }

    private struct ChoiceOption
    {
        public ChoiceKind Kind;
        public WeaponType Weapon;
        public PassiveType Passive;
        public SkillUpgrade Stat;
        public string Title;
        public string Description;
        public int NextRank;
        public int MaxRank;
        public int Stacks;
    }

    public static GameManager Instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string mainMenuSceneName = "Main";
    [SerializeField] private float baseLevelXp = 5f;
    [SerializeField] private float levelXpGrowth = 3f;
    [SerializeField] private int maxActiveStagePickups = 4;
    [SerializeField] private float normalConsumableDropChance = 0.035f;
    [SerializeField] private float eliteConsumableDropChance = 0.08f;

    public bool IsOverlayOpen => isGameOver || isLevelUpOpen || isChestRewardOpen || PauseMenu.Instance != null && PauseMenu.Instance.IsPaused;
    public float RunTime => runTime;

    private readonly List<ChoiceOption> statDefinitions = new List<ChoiceOption>();

    private PlayerHudController hud;
    private PlayerStatsRuntime playerStats;
    private PlayerArsenal playerArsenal;
    private Health playerHealth;
    private int score;
    private int level = 1;
    private int pendingLevelUps;
    private float currentXp;
    private float nextLevelXp;
    private float runTime;
    private bool isGameOver;
    private bool isLevelUpOpen;
    private bool isChestRewardOpen;
    private ChoiceOption[] currentChoices = new ChoiceOption[0];
    private ChoiceOption? pendingChestReward;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        nextLevelXp = baseLevelXp;
        BuildStatDefinitions();

        hud = GetComponent<PlayerHudController>();
        if (hud == null)
        {
            hud = gameObject.AddComponent<PlayerHudController>();
        }

        if (GetComponent<PauseMenu>() == null)
        {
            gameObject.AddComponent<PauseMenu>();
        }
    }

    private void Start()
    {
        if (scoreText == null)
        {
            scoreText = FindScoreText();
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerStats = playerObject.GetComponent<PlayerStatsRuntime>();
            if (playerStats == null)
            {
                playerStats = playerObject.AddComponent<PlayerStatsRuntime>();
            }

            playerArsenal = playerObject.GetComponent<PlayerArsenal>();
            if (playerArsenal == null)
            {
                playerArsenal = playerObject.AddComponent<PlayerArsenal>();
            }

            playerHealth = playerObject.GetComponent<Health>();
        }

        hud.Initialize();
        RefreshHud();
    }

    private void Update()
    {
        if (isGameOver || IsOverlayOpen)
        {
            return;
        }

        runTime += Time.deltaTime;
        hud.SetRunTime(runTime);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void HandleEnemyDeath(Vector3 position, int xpReward, bool dropChest, EnemyRank rank)
    {
        if (isGameOver)
        {
            return;
        }

        score++;
        UpdateScoreUI();
        ExperiencePickup.Create(position, xpReward);

        if (dropChest)
        {
            ChestPickup.Create(position + Vector3.right * 0.35f);
            return;
        }

        TrySpawnStagePickup(position, rank);
    }

    public void AddExperience(int amount)
    {
        if (isGameOver || amount <= 0)
        {
            return;
        }

        currentXp += amount;

        while (currentXp >= nextLevelXp)
        {
            currentXp -= nextLevelXp;
            level++;
            nextLevelXp = CalculateNextLevelXp(level);
            pendingLevelUps++;
        }

        RefreshHud();

        if (!isLevelUpOpen && !isChestRewardOpen && pendingLevelUps > 0)
        {
            OpenLevelUp();
        }
    }

    public void OpenChestReward()
    {
        ChoiceOption? reward = GetChestReward();
        if (!reward.HasValue)
        {
            return;
        }

        isChestRewardOpen = true;
        pendingChestReward = reward;
        Time.timeScale = 0f;
        hud.ShowChestReward("Chest reward:\n" + BuildChoiceLabel(reward.Value) + "\nmajor upgrade");
    }

    public void NotifyPlayerHealthChanged(int currentHp, int maxHp)
    {
        hud.SetHp(currentHp, maxHp);
    }

    public void HandlePlayerDeath()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        Time.timeScale = 0f;
        if (PauseMenu.Instance != null)
        {
            PauseMenu.Instance.ShowGameOver();
        }
        hud.ShowGameOver(score, level);
    }

    public void SelectUpgradeOption(int index)
    {
        SelectUpgrade(index);
    }

    public void ContinueChestReward()
    {
        ResolveChestReward();
    }

    public void RestartFromUi()
    {
        RestartLevel();
    }

    public void LoadMainMenuFromUi()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OpenLevelUp()
    {
        pendingLevelUps = Mathf.Max(0, pendingLevelUps - 1);
        currentChoices = BuildUpgradeChoices();

        if (currentChoices.Length == 0)
        {
            ResumeGameplayIfPossible();
            return;
        }

        isLevelUpOpen = true;
        Time.timeScale = 0f;

        string[] labels = new string[currentChoices.Length];
        for (int i = 0; i < currentChoices.Length; i++)
        {
            labels[i] = BuildChoiceLabel(currentChoices[i]);
        }

        hud.ShowLevelUp(labels);
    }

    private void SelectUpgrade(int index)
    {
        if (!isLevelUpOpen || index < 0 || index >= currentChoices.Length)
        {
            return;
        }

        ApplyChoice(currentChoices[index]);
        isLevelUpOpen = false;
        currentChoices = new ChoiceOption[0];
        hud.HideLevelUp();
        RefreshHud();
        ResumeGameplayIfPossible();
    }

    private void ResolveChestReward()
    {
        if (!pendingChestReward.HasValue)
        {
            return;
        }

        ApplyChoice(pendingChestReward.Value);
        pendingChestReward = null;
        isChestRewardOpen = false;
        hud.HideChestReward();
        RefreshHud();
        ResumeGameplayIfPossible();
    }

    private void ResumeGameplayIfPossible()
    {
        if (isGameOver)
        {
            return;
        }

        if (pendingLevelUps > 0)
        {
            OpenLevelUp();
            return;
        }

        Time.timeScale = 1f;
    }

    private void ApplyChoice(ChoiceOption option)
    {
        if (playerStats == null || playerArsenal == null)
        {
            return;
        }

        switch (option.Kind)
        {
            case ChoiceKind.NewWeapon:
            case ChoiceKind.UpgradeWeapon:
                playerArsenal.AddOrUpgradeWeapon(option.Weapon, option.Stacks);
                break;
            case ChoiceKind.NewPassive:
            case ChoiceKind.UpgradePassive:
                playerStats.ApplyPassive(option.Passive, option.Stacks);
                break;
            case ChoiceKind.StatUpgrade:
                playerStats.ApplyStatUpgrade(option.Stat, option.Stacks);
                break;
        }

        if (playerHealth != null)
        {
            hud.SetHp(playerHealth.CurrentHp, playerHealth.MaxHp);
        }
    }

    private ChoiceOption[] BuildUpgradeChoices()
    {
        List<ChoiceOption> pool = BuildChoicePool();
        List<ChoiceOption> result = new List<ChoiceOption>();
        int count = Mathf.Min(3, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int pickedIndex = Random.Range(0, pool.Count);
            result.Add(pool[pickedIndex]);
            pool.RemoveAt(pickedIndex);
        }

        return result.ToArray();
    }

    private List<ChoiceOption> BuildChoicePool()
    {
        List<ChoiceOption> pool = new List<ChoiceOption>();

        foreach (WeaponType weapon in System.Enum.GetValues(typeof(WeaponType)))
        {
            if (playerArsenal.CanOfferWeapon(weapon))
            {
                pool.Add(new ChoiceOption
                {
                    Kind = ChoiceKind.NewWeapon,
                    Weapon = weapon,
                    Title = PlayerArsenal.GetDisplayName(weapon),
                    Description = PlayerArsenal.GetDescription(weapon),
                    NextRank = 1,
                    MaxRank = PlayerArsenal.GetMaxLevel(weapon),
                    Stacks = 1
                });
            }
            else if (playerArsenal.CanUpgradeWeapon(weapon))
            {
                int nextRank = playerArsenal.GetWeaponLevel(weapon) + 1;
                pool.Add(new ChoiceOption
                {
                    Kind = ChoiceKind.UpgradeWeapon,
                    Weapon = weapon,
                    Title = PlayerArsenal.GetDisplayName(weapon),
                    Description = "upgrade current weapon",
                    NextRank = nextRank,
                    MaxRank = PlayerArsenal.GetMaxLevel(weapon),
                    Stacks = 1
                });
            }
        }

        foreach (PassiveType passive in System.Enum.GetValues(typeof(PassiveType)))
        {
            if (!playerStats.HasPassive(passive) && playerStats.CanAddPassive(passive))
            {
                pool.Add(new ChoiceOption
                {
                    Kind = ChoiceKind.NewPassive,
                    Passive = passive,
                    Title = GetPassiveDisplayName(passive),
                    Description = GetPassiveDescription(passive),
                    NextRank = 1,
                    MaxRank = GetPassiveMaxRank(passive),
                    Stacks = 1
                });
            }
            else if (playerStats.HasPassive(passive) && playerStats.CanUpgradePassive(passive, GetPassiveMaxRank(passive)))
            {
                pool.Add(new ChoiceOption
                {
                    Kind = ChoiceKind.UpgradePassive,
                    Passive = passive,
                    Title = GetPassiveDisplayName(passive),
                    Description = GetPassiveDescription(passive),
                    NextRank = playerStats.GetPassiveRank(passive) + 1,
                    MaxRank = GetPassiveMaxRank(passive),
                    Stacks = 1
                });
            }
        }

        for (int i = 0; i < statDefinitions.Count; i++)
        {
            ChoiceOption definition = statDefinitions[i];
            if (!playerStats.CanUpgradeStat(definition.Stat, definition.MaxRank))
            {
                continue;
            }

            definition.NextRank = playerStats.GetStatRank(definition.Stat) + 1;
            pool.Add(definition);
        }

        return pool;
    }

    private ChoiceOption? GetChestReward()
    {
        List<ChoiceOption> majorRewards = new List<ChoiceOption>();

        foreach (WeaponType weapon in playerArsenal.GetWeaponOrder())
        {
            if (playerArsenal.CanUpgradeWeapon(weapon))
            {
                majorRewards.Add(new ChoiceOption
                {
                    Kind = ChoiceKind.UpgradeWeapon,
                    Weapon = weapon,
                    Title = PlayerArsenal.GetDisplayName(weapon),
                    Description = "major chest upgrade",
                    NextRank = playerArsenal.GetWeaponLevel(weapon) + 1,
                    MaxRank = PlayerArsenal.GetMaxLevel(weapon),
                    Stacks = 2
                });
            }
        }

        if (majorRewards.Count > 0)
        {
            return majorRewards[Random.Range(0, majorRewards.Count)];
        }

        foreach (PassiveType passive in System.Enum.GetValues(typeof(PassiveType)))
        {
            if (playerStats.HasPassive(passive) && playerStats.CanUpgradePassive(passive, GetPassiveMaxRank(passive)))
            {
                majorRewards.Add(new ChoiceOption
                {
                    Kind = ChoiceKind.UpgradePassive,
                    Passive = passive,
                    Title = GetPassiveDisplayName(passive),
                    Description = "major chest upgrade",
                    NextRank = playerStats.GetPassiveRank(passive) + 1,
                    MaxRank = GetPassiveMaxRank(passive),
                    Stacks = 2
                });
            }
        }

        if (majorRewards.Count > 0)
        {
            return majorRewards[Random.Range(0, majorRewards.Count)];
        }

        List<ChoiceOption> statRewards = new List<ChoiceOption>();
        for (int i = 0; i < statDefinitions.Count; i++)
        {
            if (!playerStats.CanUpgradeStat(statDefinitions[i].Stat, statDefinitions[i].MaxRank))
            {
                continue;
            }

            ChoiceOption option = statDefinitions[i];
            option.NextRank = playerStats.GetStatRank(option.Stat) + 1;
            statRewards.Add(option);
        }

        if (statRewards.Count == 0)
        {
            return null;
        }

        return statRewards[Random.Range(0, statRewards.Count)];
    }

    private void TrySpawnStagePickup(Vector3 position, EnemyRank rank)
    {
        if (StagePickup.ActiveCount >= maxActiveStagePickups)
        {
            return;
        }

        float chance = rank == EnemyRank.Elite ? eliteConsumableDropChance : normalConsumableDropChance;
        if (Random.value > chance)
        {
            return;
        }

        StagePickup.PickupType pickupType = ChoosePickupType();
        StagePickup.Create(position + Vector3.up * 0.25f, pickupType);
    }

    private StagePickup.PickupType ChoosePickupType()
    {
        bool needsHealing = playerHealth != null && playerHealth.CurrentHp <= Mathf.Max(2, playerHealth.MaxHp / 2);
        if (needsHealing && Random.value < 0.65f)
        {
            return StagePickup.PickupType.Heal;
        }

        return Random.value < 0.7f ? StagePickup.PickupType.Magnet : StagePickup.PickupType.Heal;
    }

    private string BuildChoiceLabel(ChoiceOption option)
    {
        return option.Title + " [" + option.NextRank + "/" + option.MaxRank + "]\n" + option.Description;
    }

    private void RefreshHud()
    {
        UpdateScoreUI();
        hud.SetLevel(level);
        hud.SetXp(currentXp, nextLevelXp);
        hud.SetRunTime(runTime);
        hud.SetWeapons(BuildWeaponHudLines());
        hud.SetPassives(BuildPassiveHudLines());

        if (playerHealth != null)
        {
            hud.SetHp(playerHealth.CurrentHp, playerHealth.MaxHp);
        }
    }

    private string[] BuildWeaponHudLines()
    {
        if (playerArsenal == null)
        {
            return new string[0];
        }

        IReadOnlyList<WeaponType> weapons = playerArsenal.GetWeaponOrder();
        string[] labels = new string[weapons.Count];
        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponType weapon = weapons[i];
            labels[i] = PlayerArsenal.GetDisplayName(weapon) + " Lv." + playerArsenal.GetWeaponLevel(weapon);
        }

        return labels;
    }

    private string[] BuildPassiveHudLines()
    {
        if (playerStats == null)
        {
            return new string[0];
        }

        List<string> labels = new List<string>();
        foreach (KeyValuePair<PassiveType, int> pair in playerStats.GetPassiveRanks())
        {
            labels.Add(GetPassiveDisplayName(pair.Key) + " Lv." + pair.Value);
        }

        return labels.ToArray();
    }

    private void UpdateScoreUI()
    {
        if (scoreText == null)
        {
            scoreText = FindScoreText();
        }

        if (scoreText != null)
        {
            scoreText.text = "Kills: " + score;
        }

        hud.SetScore(score);
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private float CalculateNextLevelXp(int currentLevel)
    {
        return baseLevelXp + (currentLevel - 1) * levelXpGrowth;
    }

    private void BuildStatDefinitions()
    {
        statDefinitions.Clear();
        statDefinitions.Add(new ChoiceOption { Kind = ChoiceKind.StatUpgrade, Stat = SkillUpgrade.DamageUp, Title = "Damage Up", Description = "+global weapon damage", MaxRank = 5, Stacks = 1 });
        statDefinitions.Add(new ChoiceOption { Kind = ChoiceKind.StatUpgrade, Stat = SkillUpgrade.FireRateUp, Title = "Fire Rate Up", Description = "lower all weapon cooldowns", MaxRank = 5, Stacks = 1 });
        statDefinitions.Add(new ChoiceOption { Kind = ChoiceKind.StatUpgrade, Stat = SkillUpgrade.ProjectileSizeUp, Title = "Projectile Size Up", Description = "bigger projectiles and hit feel", MaxRank = 4, Stacks = 1 });
        statDefinitions.Add(new ChoiceOption { Kind = ChoiceKind.StatUpgrade, Stat = SkillUpgrade.ProjectileCountUp, Title = "Projectile Count Up", Description = "more projectiles for compatible weapons", MaxRank = 4, Stacks = 1 });
        statDefinitions.Add(new ChoiceOption { Kind = ChoiceKind.StatUpgrade, Stat = SkillUpgrade.MoveSpeedUp, Title = "Move Speed Up", Description = "move faster", MaxRank = 5, Stacks = 1 });
        statDefinitions.Add(new ChoiceOption { Kind = ChoiceKind.StatUpgrade, Stat = SkillUpgrade.MaxHpUp, Title = "Max HP Up", Description = "+2 max HP and heal 2", MaxRank = 5, Stacks = 1 });
        statDefinitions.Add(new ChoiceOption { Kind = ChoiceKind.StatUpgrade, Stat = SkillUpgrade.PickupRangeUp, Title = "Pickup Range Up", Description = "XP pulls from farther away", MaxRank = 4, Stacks = 1 });
    }

    private static string GetPassiveDisplayName(PassiveType passive)
    {
        switch (passive)
        {
            case PassiveType.PowerCore:
                return "Power Core";
            case PassiveType.QuickHands:
                return "Quick Hands";
            case PassiveType.HeavyDraw:
                return "Heavy Draw";
            case PassiveType.SwiftBoots:
                return "Swift Boots";
            case PassiveType.HollowHeart:
                return "Hollow Heart";
            case PassiveType.MagnetCharm:
                return "Magnet Charm";
            default:
                return passive.ToString();
        }
    }

    private static string GetPassiveDescription(PassiveType passive)
    {
        switch (passive)
        {
            case PassiveType.PowerCore:
                return "+damage for all weapons";
            case PassiveType.QuickHands:
                return "+attack speed for all weapons";
            case PassiveType.HeavyDraw:
                return "+projectile size and area";
            case PassiveType.SwiftBoots:
                return "+move speed";
            case PassiveType.HollowHeart:
                return "+max HP and light regen";
            case PassiveType.MagnetCharm:
                return "+pickup range";
            default:
                return "passive upgrade";
        }
    }

    private static int GetPassiveMaxRank(PassiveType passive)
    {
        return passive == PassiveType.HollowHeart ? 4 : 5;
    }

    private TextMeshProUGUI FindScoreText()
    {
        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text != null && text.name == "ScoreText")
            {
                return text;
            }
        }

        return null;
    }
}
