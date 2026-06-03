using UnityEngine;

public static class AchievementProgress
{
    public const string FirstBlood = "first_blood";
    public const string FirstRound = "first_round";
    public const string FirstWin = "first_win";
    public const string AtticVeteran50 = "attic_veteran_50";
    public const string AtticRegular100 = "attic_regular_100";

    public const string FirstOverheat = "first_overheat";
    public const string Furnace = "furnace";
    public const string CoolHeadMatch = "cool_head_match";
    public const string Miscalculated = "miscalculated";
    public const string Terminator = "terminator";
    public const string SystemOverload = "system_overload";

    public const string Medic = "medic";
    public const string ShieldBearer = "shield_bearer";
    public const string Collector = "collector";
    public const string LootHunter = "loot_hunter";
    public const string LastHopePickup = "last_hope_pickup";
    public const string ShieldWin = "shield_win";
    public const string Greed = "greed";

    public const string Climber = "climber";
    public const string HeightKing = "height_king";
    public const string Acrobat = "acrobat";
    public const string ParkourRunner = "parkour_runner";
    public const string AtticLord = "attic_lord";
    public const string FloorIsLava = "floor_is_lava";

    public const string HitStreak5 = "hit_streak_5";
    public const string HitStreak10 = "hit_streak_10";
    public const string KnockoutMachine = "knockout_machine";
    public const string Sniper = "sniper";
    public const string CounterAttack = "counter_attack";
    public const string PerfectFight = "perfect_fight";

    public const string LastHp = "last_hp";
    public const string Comeback = "comeback";
    public const string Immortal = "immortal";
    public const string LastChance = "last_chance";

    public const string AtticGhost = "attic_ghost";
    public const string ProfessionalCoward = "professional_coward";
    public const string HotPotato = "hot_potato";
    public const string ItsATrap = "its_a_trap";

    public const string BonusWin = "bonus_win";
    public const string HeavyFinish = "heavy_finish";
    public const string DoubleJumpHit = "double_jump_hit";
    public const string ShieldMaster = "shield_master";
    public const string CoolHead = "cool_head";
    public const string DamageBoost = "damage_boost";
    public const string PlatformDrop = "platform_drop";
    public const string OverheatSurvive = "overheat_survive";
    public const string TenWins = "ten_wins";
    public const string CleanWin = "clean_win";

    private const string AchievementPrefix = "achievement_";
    private const string CounterPrefix = "achievement_counter_";
    private const string MatchCounterPrefix = "matches_p";
    private const string WinStreakPrefix = "win_streak_p";

    public static readonly string[] AllIds =
    {
        FirstBlood, FirstRound, FirstWin, AtticVeteran50, AtticRegular100,
        FirstOverheat, Furnace, CoolHeadMatch, Miscalculated, Terminator, SystemOverload,
        Medic, ShieldBearer, Collector, LootHunter, LastHopePickup, ShieldWin, Greed,
        Climber, HeightKing, Acrobat, ParkourRunner, AtticLord, FloorIsLava,
        HitStreak5, HitStreak10, KnockoutMachine, Sniper, CounterAttack, PerfectFight,
        LastHp, Comeback, Immortal, LastChance,
        AtticGhost, ProfessionalCoward, HotPotato, ItsATrap,
        BonusWin, HeavyFinish, DoubleJumpHit, ShieldMaster, CoolHead, DamageBoost,
        PlatformDrop, OverheatSurvive, TenWins, CleanWin
    };

    public static bool IsUnlocked(int playerNumber, string id)
    {
        return PlayerPrefs.GetInt(GetAchievementKey(playerNumber, id), 0) == 1;
    }

    public static void Unlock(int playerNumber, string id)
    {
        if (playerNumber < 1 || playerNumber > 2 || string.IsNullOrEmpty(id))
            return;

        PlayerPrefs.SetInt(GetAchievementKey(playerNumber, id), 1);
        PlayerPrefs.Save();
    }

    public static int AddCounter(int playerNumber, string id, int amount = 1)
    {
        string key = GetCounterKey(playerNumber, id);
        int value = PlayerPrefs.GetInt(key, 0) + amount;
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
        return value;
    }

    public static int AddMatch(int playerNumber)
    {
        string key = MatchCounterPrefix + playerNumber;
        int matches = PlayerPrefs.GetInt(key, 0) + 1;
        PlayerPrefs.SetInt(key, matches);

        if (matches >= 50)
            Unlock(playerNumber, AtticVeteran50);

        if (matches >= 100)
            Unlock(playerNumber, AtticRegular100);

        PlayerPrefs.Save();
        return matches;
    }

    public static int AddWin(int playerNumber)
    {
        string key = WinStreakPrefix + playerNumber;
        int wins = PlayerPrefs.GetInt(key, 0) + 1;
        PlayerPrefs.SetInt(key, wins);

        if (wins >= 5)
            Unlock(playerNumber, Immortal);

        PlayerPrefs.Save();
        return wins;
    }

    public static void ResetWinStreak(int playerNumber)
    {
        PlayerPrefs.SetInt(WinStreakPrefix + playerNumber, 0);
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        for (int playerNumber = 1; playerNumber <= 2; playerNumber++)
        {
            foreach (string id in AllIds)
            {
                PlayerPrefs.DeleteKey(GetAchievementKey(playerNumber, id));
                PlayerPrefs.DeleteKey(GetCounterKey(playerNumber, id));
            }

            PlayerPrefs.DeleteKey(MatchCounterPrefix + playerNumber);
            PlayerPrefs.DeleteKey(WinStreakPrefix + playerNumber);
        }

        PlayerPrefs.Save();
    }

    private static string GetAchievementKey(int playerNumber, string id)
    {
        return AchievementPrefix + "p" + playerNumber + "_" + id;
    }

    private static string GetCounterKey(int playerNumber, string id)
    {
        return CounterPrefix + "p" + playerNumber + "_" + id;
    }
}
