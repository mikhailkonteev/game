using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool IsGameOver { get; private set; }
    public static bool IsPaused { get; private set; }
    public static bool IsRoundIntroActive { get; private set; }
    public static bool IsInputBlocked => IsGameOver || IsPaused || IsRoundIntroActive;

    private const float HealthBarWidth = 320f;
    private const float HealthBarHeight = 24f;
    private const float HeatBarHeight = 9f;
    private const float HeatBarGap = 7f;
    private const float BonusIconGap = 6f;
    private const float BonusIconSize = 30f;
    private const float HealthBarTopPadding = 24f;
    private const float HealthBarSidePadding = 32f;
    private const int WinsToMatch = 2;
    public const string RoundMaxHealthPreference = "RoundMaxHealth";
    public const string RoundHealthMultiplierPreference = "RoundHealthMultiplier";

    public Health player1Health;
    public Health player2Health;
    public PlayerCombat player1Combat;
    public PlayerCombat player2Combat;

    public TextMeshProUGUI player1HPText;
    public TextMeshProUGUI player2HPText;
    public TextMeshProUGUI winnerText;

    private string winnerName;
    private Color winnerColor;
    private bool player1TookDamage;
    private bool player2TookDamage;
    private bool player1PickedBonus;
    private bool player2PickedBonus;
    private bool player1Overheated;
    private bool player2Overheated;
    private int player1ShieldBlocks;
    private int player2ShieldBlocks;
    private bool player1TookDamageThisMatch;
    private bool player2TookDamageThisMatch;
    private bool player1OverheatedThisMatch;
    private bool player2OverheatedThisMatch;
    private bool player1CriticalHealthPickup;
    private bool player2CriticalHealthPickup;
    private bool player1ShieldPickup;
    private bool player2ShieldPickup;
    private int player1MatchOverheats;
    private int player2MatchOverheats;
    private int player1HitStreak;
    private int player2HitStreak;
    private int player1PickupMask;
    private int player2PickupMask;
    private int lastPickupPlayer;
    private int consecutivePickupCount;
    private int previousRoundWinner;
    private float player1LastOverheatTime = -99f;
    private float player2LastOverheatTime = -99f;
    private float player1LastHealthPickupTime = -99f;
    private float player2LastHealthPickupTime = -99f;
    private int player1RoundWins;
    private int player2RoundWins;
    private int currentRound = 1;
    private bool isMatchOver;
    private Vector3 player1StartPosition;
    private Vector3 player2StartPosition;
    private Quaternion player1StartRotation;
    private Quaternion player2StartRotation;
    private Vector3 player1StartScale;
    private Vector3 player2StartScale;
    private Vector2 matchStatsScroll;
    private Vector2 pauseMenuScroll;
    private float roundIntroEndTime;
    private string roundIntroText;
    private Sprite shieldBonusSprite;
    private Sprite damageBonusSprite;
    private readonly RoundStats currentStats = new RoundStats();
    private readonly List<RoundSummary> roundSummaries = new List<RoundSummary>();

    void Awake()
    {
        IsGameOver = false;
        IsPaused = false;
        IsRoundIntroActive = false;
        winnerName = "";
        winnerColor = Color.white;
        currentRound = 1;
        isMatchOver = false;
        player1RoundWins = 0;
        player2RoundWins = 0;
        ResetAchievementRoundFlags();
        ResetAchievementMatchFlags();
        Time.timeScale = 1f;
        AudioManager.PlayFightMusic();
        shieldBonusSprite = Resources.Load<Sprite>("Boosts/shield");
        damageBonusSprite = Resources.Load<Sprite>("Boosts/damage");

        if (player1HPText != null)
            player1HPText.gameObject.SetActive(false);

        if (player2HPText != null)
            player2HPText.gameObject.SetActive(false);

        if (winnerText != null)
            winnerText.gameObject.SetActive(false);

        if (player1Combat == null && player1Health != null)
            player1Combat = player1Health.GetComponent<PlayerCombat>();

        if (player2Combat == null && player2Health != null)
            player2Combat = player2Health.GetComponent<PlayerCombat>();

        ApplyCharacterSettings();
        CachePlayerStarts();
        ShowRoundIntro();
    }

    void Update()
    {
        UpdateRoundIntro();

        if (!IsGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            SetPaused(!IsPaused);
        }

        if (!IsGameOver)
        {
            if (player1Health.currentHealth <= 0)
                FinishRound(2);
            else if (player2Health.currentHealth <= 0)
                FinishRound(1);
        }

        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            if (isMatchOver)
                RestartScene();
            else
                StartNextRound();
        }
    }

    void OnGUI()
    {
        DrawHealthBars();

        if (IsGameOver)
        {
            if (isMatchOver)
                DrawMatchPanel();
            else
                DrawRoundPanel();

            return;
        }

        if (!IsPaused)
        {
            if (IsRoundIntroActive)
                DrawRoundIntro();

            return;
        }

        DrawPauseMenu();
    }

    void DrawPauseMenu()
    {
        float menuWidth = Mathf.Min(Screen.width - 40f, 780f);
        float menuHeight = Mathf.Min(Screen.height - 40f, 620f);
        Rect menuRect = new Rect(
            (Screen.width - menuWidth) * 0.5f,
            (Screen.height - menuHeight) * 0.5f,
            menuWidth,
            menuHeight);

        DrawPanelBackground(menuRect);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 28,
            fontStyle = FontStyle.Bold
        };
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft
        };
        GUIStyle actionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17,
            alignment = TextAnchor.MiddleLeft
        };
        GUIStyle keyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight
        };

        GUI.color = Color.white;
        GUI.Label(new Rect(menuRect.x, menuRect.y + 18f, menuRect.width, 34f), "ПАУЗА / НАСТРОЙКИ", titleStyle);

        float buttonY = menuRect.y + 64f;
        float buttonWidth = 170f;
        float buttonGap = 18f;
        float firstButtonX = menuRect.center.x - buttonWidth * 1.5f - buttonGap;
        if (GUI.Button(new Rect(firstButtonX, buttonY, buttonWidth, 42f), "Continue"))
            SetPaused(false);
        if (GUI.Button(new Rect(firstButtonX + buttonWidth + buttonGap, buttonY, buttonWidth, 42f), "Restart"))
            RestartScene();
        if (GUI.Button(new Rect(firstButtonX + (buttonWidth + buttonGap) * 2f, buttonY, buttonWidth, 42f), "Quit"))
            QuitGame();

        Rect scrollViewport = new Rect(menuRect.x + 28f, menuRect.y + 122f, menuRect.width - 56f, menuRect.height - 150f);
        Rect scrollContent = new Rect(0f, 0f, scrollViewport.width - 18f, 520f);
        pauseMenuScroll = GUI.BeginScrollView(scrollViewport, pauseMenuScroll, scrollContent, false, true);

        float localSliderX = 82f;
        float localSliderWidth = scrollContent.width - 164f;
        float localMusicY = 36f;
        GUI.Label(new Rect(localSliderX, localMusicY - 24f, 170f, 24f), "Музыка", labelStyle);
        AudioManager.MusicVolume = GUI.HorizontalSlider(new Rect(localSliderX + 150f, localMusicY - 18f, localSliderWidth - 150f, 22f), AudioManager.MusicVolume, 0f, 1f);
        GUI.Label(new Rect(localSliderX, localMusicY + 22f, 170f, 24f), "Звуки", labelStyle);
        AudioManager.SfxVolume = GUI.HorizontalSlider(new Rect(localSliderX + 150f, localMusicY + 28f, localSliderWidth - 150f, 22f), AudioManager.SfxVolume, 0f, 1f);

        GUI.Label(new Rect(0f, 116f, scrollContent.width, 30f), "УПРАВЛЕНИЕ", titleStyle);
        DrawPauseControlColumn(new Rect(22f, 162f, (scrollContent.width - 60f) * 0.5f, 240f), true, actionStyle, keyStyle);
        DrawPauseControlColumn(new Rect((scrollContent.width - 60f) * 0.5f + 38f, 162f, (scrollContent.width - 60f) * 0.5f, 240f), false, actionStyle, keyStyle);
        GUI.EndScrollView();
    }

    void DrawPauseControlColumn(Rect rect, bool playerOne, GUIStyle actionStyle, GUIStyle keyStyle)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.03f, 0.03f, 0.035f, 0.72f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = playerOne ? new Color(0.25f, 0.5f, 1f, 1f) : new Color(1f, 0.28f, 0.22f, 1f);
        DrawBorder(rect, 2f);

        GUI.color = Color.white;
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Label(new Rect(rect.x, rect.y + 8f, rect.width, 28f), playerOne ? "PLAYER 1" : "PLAYER 2", headerStyle);

        ControlAction[] actions = playerOne
            ? new[]
            {
                ControlAction.Player1Left,
                ControlAction.Player1Right,
                ControlAction.Player1Jump,
                ControlAction.Player1Drop,
                ControlAction.Player1LightAttack,
                ControlAction.Player1HeavyAttack
            }
            : new[]
            {
                ControlAction.Player2Left,
                ControlAction.Player2Right,
                ControlAction.Player2Jump,
                ControlAction.Player2Drop,
                ControlAction.Player2LightAttack,
                ControlAction.Player2HeavyAttack
            };

        for (int i = 0; i < actions.Length; i++)
        {
            float y = rect.y + 48f + i * 30f;
            GUI.Label(new Rect(rect.x + 18f, y, rect.width - 125f, 26f), ControlBindings.GetShortLabel(actions[i]), actionStyle);
            GUI.Label(new Rect(rect.xMax - 104f, y, 84f, 26f), ControlBindings.GetKeyName(ControlBindings.Get(actions[i])), keyStyle);
        }

        GUI.color = previousColor;
    }
    void DrawRoundPanel()
    {
        float panelWidth = 430f;
        float panelHeight = 245f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        Color previousColor = GUI.color;
        DrawPanelBackground(panelRect);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 26,
            fontStyle = FontStyle.Bold
        };

        GUIStyle winnerStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 34,
            fontStyle = FontStyle.Bold
        };

        GUIStyle hintStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14
        };

        GUI.color = Color.white;
        GUI.Label(new Rect(panelRect.x, panelRect.y + 28f, panelRect.width, 34f), "\u0420\u0410\u0423\u041D\u0414 \u0417\u0410\u0412\u0415\u0420\u0428\u0415\u041D", titleStyle);
        GUI.color = winnerColor;
        GUI.Label(new Rect(panelRect.x, panelRect.y + 80f, panelRect.width, 48f), winnerName, winnerStyle);
        GUI.color = Color.white;
        GUI.Label(new Rect(panelRect.x, panelRect.y + 132f, panelRect.width, 24f), "\u0432\u044B\u0438\u0433\u0440\u044B\u0432\u0430\u0435\u0442 \u0440\u0430\u0443\u043D\u0434 " + currentRound, hintStyle);

        Rect nextButtonRect = new Rect(panelRect.x + 100f, panelRect.y + 175f, 230f, 42f);

        if (GUI.Button(nextButtonRect, "\u0421\u043B\u0435\u0434\u0443\u044E\u0449\u0438\u0439 \u0440\u0430\u0443\u043D\u0434  (R)"))
            StartNextRound();

        GUI.color = previousColor;
    }
    void DrawMatchPanel()
    {
        float panelWidth = Mathf.Min(Screen.width - 36f, 900f);
        float panelHeight = Mathf.Min(Screen.height - 24f, 920f);
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        Color previousColor = GUI.color;
        DrawPanelBackground(panelRect);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 28,
            fontStyle = FontStyle.Bold
        };

        GUIStyle winnerStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 32,
            fontStyle = FontStyle.Bold
        };

        GUI.color = Color.white;
        GUI.Label(new Rect(panelRect.x, panelRect.y + 20f, panelRect.width, 34f), "\u0424\u0418\u041D\u0410\u041B\u042C\u041D\u042B\u0419 \u0421\u0427\u0415\u0422", titleStyle);
        GUI.color = winnerColor;
        GUI.Label(new Rect(panelRect.x, panelRect.y + 60f, panelRect.width, 40f), winnerName + " \u041F\u041E\u0411\u0415\u0416\u0414\u0410\u0415\u0422", winnerStyle);
        GUI.color = Color.white;
        GUI.Label(new Rect(panelRect.x, panelRect.y + 101f, panelRect.width, 28f), "Player 1  " + player1RoundWins + " : " + player2RoundWins + "  Player 2", titleStyle);
        GUI.Label(new Rect(panelRect.x, panelRect.y + 126f, panelRect.width, 24f), GetMvpText(), titleStyle);

        float buttonY = panelRect.yMax - 64f;
        Rect statsViewport = new Rect(panelRect.x + 24f, panelRect.y + 158f, panelRect.width - 48f, buttonY - panelRect.y - 174f);
        float cardGap = 14f;
        float cardHeight = 162f;
        float contentHeight = Mathf.Max(statsViewport.height, roundSummaries.Count * (cardHeight + cardGap) - cardGap + 8f);
        Rect contentRect = new Rect(0f, 0f, statsViewport.width - 18f, contentHeight);

        GUI.color = new Color(0.02f, 0.018f, 0.018f, 0.35f);
        GUI.DrawTexture(statsViewport, Texture2D.whiteTexture);

        matchStatsScroll = GUI.BeginScrollView(statsViewport, matchStatsScroll, contentRect, false, true);
        float y = 0f;
        foreach (RoundSummary summary in roundSummaries)
        {
            Rect cardRect = new Rect(8f, y, contentRect.width - 16f, cardHeight);
            DrawRoundSummaryCard(cardRect, summary);
            y += cardHeight + cardGap;
        }
        GUI.EndScrollView();

        Rect menuButtonRect = new Rect(panelRect.x + 125f, buttonY, 250f, 44f);
        Rect restartButtonRect = new Rect(panelRect.xMax - 375f, buttonY, 250f, 44f);

        if (GUI.Button(menuButtonRect, "\u0413\u043B\u0430\u0432\u043D\u043E\u0435 \u043C\u0435\u043D\u044E"))
            QuitGame();

        if (GUI.Button(restartButtonRect, "\u0421\u044B\u0433\u0440\u0430\u0442\u044C \u0435\u0449\u0435  (R)"))
            RestartScene();

        GUI.color = previousColor;
    }

    string GetMvpText()
    {
        int player1Damage = 0;
        int player2Damage = 0;
        int player1PickupsTotal = 0;
        int player2PickupsTotal = 0;

        foreach (RoundSummary summary in roundSummaries)
        {
            player1Damage += summary.player1DamageDealt;
            player2Damage += summary.player2DamageDealt;
            player1PickupsTotal += summary.player1Pickups;
            player2PickupsTotal += summary.player2Pickups;
        }

        if (player1PickupsTotal >= player2PickupsTotal + 2)
            return "MVP: Player 1 - мастер бонусов";
        if (player2PickupsTotal >= player1PickupsTotal + 2)
            return "MVP: Player 2 - мастер бонусов";
        if (player1Damage >= player2Damage)
            return "MVP: Player 1 - самый агрессивный";

        return "MVP: Player 2 - самый агрессивный";
    }

    void DrawRoundSummaryCard(Rect cardRect, RoundSummary summary)
    {
        GUIStyle roundStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = cardRect.height < 145f ? 15 : 17,
            fontStyle = FontStyle.Bold
        };

        GUIStyle playerStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = cardRect.height < 145f ? 14 : 16,
            fontStyle = FontStyle.Bold
        };

        GUIStyle statStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = cardRect.height < 145f ? 13 : 15,
            wordWrap = true
        };

        GUI.color = new Color(0.08f, 0.065f, 0.05f, 0.96f);
        GUI.DrawTexture(cardRect, Texture2D.whiteTexture);
        GUI.color = new Color(0.95f, 0.55f, 0.16f, 1f);
        DrawBorder(cardRect, 2f);

        GUI.color = Color.white;
        GUI.Label(new Rect(cardRect.x, cardRect.y + 8f, cardRect.width, 22f), "\u0420\u0430\u0443\u043D\u0434 " + summary.roundNumber, roundStyle);
        GUI.Label(new Rect(cardRect.x, cardRect.y + 30f, cardRect.width, 22f), "\u041F\u043E\u0431\u0435\u0434\u0438\u0442\u0435\u043B\u044C: Player " + summary.winnerPlayer, roundStyle);

        float columnWidth = (cardRect.width - 70f) * 0.5f;
        Rect player1Rect = new Rect(cardRect.x + 28f, cardRect.y + 62f, columnWidth, cardRect.height - 70f);
        Rect player2Rect = new Rect(player1Rect.xMax + 14f, player1Rect.y, columnWidth, player1Rect.height);

        GUI.Label(new Rect(player1Rect.x, player1Rect.y, player1Rect.width, 22f), "Player 1", playerStyle);
        GUI.Label(new Rect(player2Rect.x, player2Rect.y, player2Rect.width, 22f), "Player 2", playerStyle);

        GUI.Label(
            new Rect(player1Rect.x, player1Rect.y + 30f, player1Rect.width, 74f),
            "\u041D\u0430\u043D\u0435\u0441\u0451\u043D\u043D\u044B\u0439 \u0443\u0440\u043E\u043D: " + summary.player1DamageDealt + "\n" +
            "\u041F\u043E\u043B\u0443\u0447\u0435\u043D\u043D\u044B\u0439 \u0443\u0440\u043E\u043D: " + summary.player1DamageTaken + "\n" +
            "\u041F\u043E\u0434\u043E\u0431\u0440\u0430\u043D\u043E \u0431\u043E\u043D\u0443\u0441\u043E\u0432: " + summary.player1Pickups,
            statStyle);

        GUI.Label(
            new Rect(player2Rect.x, player2Rect.y + 30f, player2Rect.width, 74f),
            "\u041D\u0430\u043D\u0435\u0441\u0451\u043D\u043D\u044B\u0439 \u0443\u0440\u043E\u043D: " + summary.player2DamageDealt + "\n" +
            "\u041F\u043E\u043B\u0443\u0447\u0435\u043D\u043D\u044B\u0439 \u0443\u0440\u043E\u043D: " + summary.player2DamageTaken + "\n" +
            "\u041F\u043E\u0434\u043E\u0431\u0440\u0430\u043D\u043E \u0431\u043E\u043D\u0443\u0441\u043E\u0432: " + summary.player2Pickups,
            statStyle);
    }
    void DrawPanelBackground(Rect panelRect)
    {
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

        GUI.color = new Color(0.05f, 0.04f, 0.035f, 0.96f);
        GUI.DrawTexture(panelRect, Texture2D.whiteTexture);

        GUI.color = new Color(0.95f, 0.55f, 0.16f, 1f);
        GUI.DrawTexture(new Rect(panelRect.x, panelRect.y, panelRect.width, 5f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x, panelRect.yMax - 5f, panelRect.width, 5f), Texture2D.whiteTexture);
    }

    void DrawBorder(Rect rect, float thickness)
    {
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
    }

    void DrawHealthBars()
    {
        DrawHealthBar(
            new Rect(HealthBarSidePadding, HealthBarTopPadding, HealthBarWidth, HealthBarHeight),
            player1Health,
            new Color(0.2f, 0.45f, 1f),
            "P1",
            player1RoundWins,
            false);
        Rect player1HeatRect = new Rect(HealthBarSidePadding, HealthBarTopPadding + HealthBarHeight + HeatBarGap, HealthBarWidth, HeatBarHeight);
        DrawHeatBar(player1HeatRect, player1Combat);
        DrawActiveBonusIcons(player1HeatRect, player1Health, player1Combat, false);

        DrawHealthBar(
            new Rect(Screen.width - HealthBarSidePadding - HealthBarWidth, HealthBarTopPadding, HealthBarWidth, HealthBarHeight),
            player2Health,
            new Color(1f, 0.25f, 0.2f),
            "P2",
            player2RoundWins,
            true);
        Rect player2HeatRect = new Rect(Screen.width - HealthBarSidePadding - HealthBarWidth, HealthBarTopPadding + HealthBarHeight + HeatBarGap, HealthBarWidth, HeatBarHeight);
        DrawHeatBar(player2HeatRect, player2Combat);
        DrawActiveBonusIcons(player2HeatRect, player2Health, player2Combat, true);
    }

    void DrawHealthBar(Rect rect, Health health, Color fillColor, string label, int roundWins, bool scoreOnLeft)
    {
        float fillAmount = 0f;

        if (health != null && health.maxHealth > 0)
            fillAmount = Mathf.Clamp01((float)health.currentHealth / health.maxHealth);

        Color previousColor = GUI.color;

        GUI.color = new Color(0.03f, 0.03f, 0.03f, 0.85f);
        GUI.DrawTexture(new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f), Texture2D.whiteTexture);

        GUI.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * fillAmount, rect.height), Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x, rect.y - 21f, rect.width, 20f), label);

        Rect scoreRect = scoreOnLeft
            ? new Rect(rect.x - 54f, rect.y - 5f, 42f, 34f)
            : new Rect(rect.xMax + 12f, rect.y - 5f, 42f, 34f);

        GUI.color = new Color(0.04f, 0.035f, 0.03f, 0.95f);
        GUI.DrawTexture(scoreRect, Texture2D.whiteTexture);

        GUI.color = fillColor;
        DrawBorder(scoreRect, 2f);

        GUI.color = Color.white;
        GUIStyle scoreStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };
        GUI.Label(scoreRect, roundWins.ToString(), scoreStyle);

        GUI.color = previousColor;
    }

    void DrawHeatBar(Rect rect, PlayerCombat combat)
    {
        float fillAmount = combat != null ? combat.HeatPercent : 0f;
        Color previousColor = GUI.color;

        GUI.color = new Color(0.03f, 0.03f, 0.03f, 0.75f);
        GUI.DrawTexture(new Rect(rect.x - 2f, rect.y - 2f, rect.width + 4f, rect.height + 4f), Texture2D.whiteTexture);

        bool critical = combat != null && combat.IsHeatCritical;
        float blink = critical ? Mathf.Abs(Mathf.Sin(Time.time * 8f)) : 0f;

        GUI.color = critical
            ? Color.Lerp(new Color(0.22f, 0.06f, 0.02f, 0.95f), new Color(0.55f, 0.12f, 0.02f, 0.95f), blink)
            : new Color(0.14f, 0.09f, 0.03f, 0.9f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = critical
            ? Color.Lerp(new Color(1f, 0.42f, 0.05f, 1f), new Color(1f, 0.86f, 0.05f, 1f), blink)
            : new Color(1f, 0.48f, 0.06f, 1f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * fillAmount, rect.height), Texture2D.whiteTexture);

        GUI.color = previousColor;
    }

    void DrawActiveBonusIcons(Rect heatRect, Health health, PlayerCombat combat, bool alignRight)
    {
        int activeCount = 0;
        if (health != null && health.HasDamageReduction)
            activeCount++;
        if (combat != null && combat.HasDamageBoost)
            activeCount++;

        if (activeCount == 0)
            return;

        float totalWidth = activeCount * BonusIconSize + (activeCount - 1) * BonusIconGap;
        float x = alignRight ? heatRect.xMax - totalWidth : heatRect.x;
        float y = heatRect.yMax + 7f;

        if (health != null && health.HasDamageReduction)
        {
            DrawBonusIcon(new Rect(x, y, BonusIconSize, BonusIconSize), shieldBonusSprite, new Color(0.35f, 0.72f, 1f, 1f));
            x += BonusIconSize + BonusIconGap;
        }

        if (combat != null && combat.HasDamageBoost)
            DrawBonusIcon(new Rect(x, y, BonusIconSize, BonusIconSize), damageBonusSprite, new Color(1f, 0.7f, 0.1f, 1f));
    }

    void DrawBonusIcon(Rect rect, Sprite sprite, Color fallbackColor)
    {
        Color previousColor = GUI.color;

        GUI.color = new Color(0.02f, 0.02f, 0.02f, 0.78f);
        GUI.DrawTexture(new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f), Texture2D.whiteTexture);

        GUI.color = fallbackColor;
        DrawBorder(new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f), 2f);

        GUI.color = Color.white;
        if (sprite != null && sprite.texture != null)
            GUI.DrawTexture(rect, sprite.texture, ScaleMode.ScaleToFit, true);
        else
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = previousColor;
    }

    void FinishRound(int winnerPlayerNumber)
    {
        IsGameOver = true;
        IsPaused = false;
        Time.timeScale = 1f;

        if (winnerPlayerNumber == 1)
        {
            player1RoundWins++;
            winnerName = "PLAYER 1";
            winnerColor = new Color(0.2f, 0.45f, 1f);
        }
        else
        {
            player2RoundWins++;
            winnerName = "PLAYER 2";
            winnerColor = new Color(1f, 0.25f, 0.2f);
        }

        roundSummaries.Add(new RoundSummary(currentRound, winnerPlayerNumber, currentStats));
        UnlockWinAchievements(winnerPlayerNumber);
        isMatchOver = player1RoundWins >= WinsToMatch || player2RoundWins >= WinsToMatch;
        if (isMatchOver)
            matchStatsScroll = Vector2.zero;

        if (isMatchOver)
            AudioManager.PlayMatchWin();
        else
            AudioManager.PlayRoundWin();

        if (previousRoundWinner != 0 && previousRoundWinner != winnerPlayerNumber)
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.CounterAttack);

        previousRoundWinner = winnerPlayerNumber;

        if (isMatchOver)
            UnlockMatchAchievements(winnerPlayerNumber);
    }

    void StartNextRound()
    {
        IsGameOver = false;
        IsPaused = false;
        Time.timeScale = 1f;
        currentRound++;
        winnerName = "";
        winnerColor = Color.white;
        currentStats.Reset();
        ResetAchievementRoundFlags();

        ResetPlayerForRound(player1Health, player1Combat, player1StartPosition, player1StartRotation, player1StartScale);
        ResetPlayerForRound(player2Health, player2Combat, player2StartPosition, player2StartRotation, player2StartScale);
        AudioManager.ResumeMusic();
        ShowRoundIntro();
    }

    void ShowRoundIntro()
    {
        IsRoundIntroActive = true;
        roundIntroEndTime = Time.time + 1.25f;
        roundIntroText = currentRound >= WinsToMatch ? "FINAL ROUND" : "ROUND " + currentRound;
    }

    void UpdateRoundIntro()
    {
        if (IsRoundIntroActive && Time.time >= roundIntroEndTime)
            IsRoundIntroActive = false;
    }

    void DrawRoundIntro()
    {
        float alpha = Mathf.Clamp01((roundIntroEndTime - Time.time) / 0.35f);
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 52,
            fontStyle = FontStyle.Bold
        };

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.36f * alpha);
        GUI.DrawTexture(new Rect(0f, Screen.height * 0.38f, Screen.width, Screen.height * 0.24f), Texture2D.whiteTexture);
        GUI.color = new Color(1f, 0.75f, 0.18f, alpha);
        GUI.Label(new Rect(0f, Screen.height * 0.43f, Screen.width, 72f), roundIntroText, style);
        GUI.color = previousColor;
    }

    string GetControlSummary(bool playerOne)
    {
        if (playerOne)
        {
            return ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player1Left)) + "/" +
                   ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player1Right)) + " движение, " +
                   ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player1Jump)) + " прыжок, " +
                   ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player1Drop)) + " вниз, " +
                   ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player1LightAttack)) + "/" +
                   ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player1HeavyAttack)) + " удары";
        }

        return ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player2Left)) + "/" +
               ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player2Right)) + " движение, " +
               ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player2Jump)) + " прыжок, " +
               ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player2Drop)) + " вниз, " +
               ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player2LightAttack)) + "/" +
               ControlBindings.GetKeyName(ControlBindings.Get(ControlAction.Player2HeavyAttack)) + " удары";
    }

    void ResetPlayerForRound(Health health, PlayerCombat combat, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (health == null)
            return;

        health.transform.SetPositionAndRotation(position, rotation);
        health.transform.localScale = scale;
        health.ResetHealth();

        if (combat != null)
            combat.ResetCombatState();

        PlayerController controller = health.GetComponent<PlayerController>();
        if (controller != null)
            controller.ResetRoundState();
    }

    void CachePlayerStarts()
    {
        if (player1Health != null)
        {
            player1StartPosition = player1Health.transform.position;
            player1StartRotation = player1Health.transform.rotation;
            player1StartScale = player1Health.transform.localScale;
        }

        if (player2Health != null)
        {
            player2StartPosition = player2Health.transform.position;
            player2StartRotation = player2Health.transform.rotation;
            player2StartScale = player2Health.transform.localScale;
        }
    }

    void ApplyCharacterSettings()
    {
        ApplyCharacterSettingsToPlayer(
            1,
            player1Health,
            player1Combat,
            player1Health != null ? player1Health.GetComponent<PlayerController>() : null);
        ApplyCharacterSettingsToPlayer(
            2,
            player2Health,
            player2Combat,
            player2Health != null ? player2Health.GetComponent<PlayerController>() : null);
    }

    void ApplyCharacterSettingsToPlayer(int playerNumber, Health health, PlayerCombat combat, PlayerController controller)
    {
        int selected = PlayerPrefs.GetInt(playerNumber == 1 ? "Player1Character" : "Player2Character", playerNumber == 1 ? 0 : 1);
        float healthMultiplier = Mathf.Max(0.1f, PlayerPrefs.GetFloat(RoundHealthMultiplierPreference, 1f));
        int maxHealth = Mathf.RoundToInt(GetCharacterBaseHealth(selected) * healthMultiplier);
        ApplyHealthSetting(health, maxHealth);

        if (controller != null)
            controller.moveSpeed = 5f * GetCharacterSpeedMultiplier(selected);

        if (combat != null)
        {
            combat.attackDamage = Mathf.Max(1, Mathf.RoundToInt(10f * GetCharacterDamageMultiplier(selected)));
            combat.heavyAttackDamage = Mathf.Max(1, Mathf.RoundToInt(18f * GetCharacterDamageMultiplier(selected)));
        }
    }

    int GetCharacterBaseHealth(int selected)
    {
        switch (selected)
        {
            case 1:
                return 125;
            case 2:
                return 85;
            default:
                return 100;
        }
    }

    float GetCharacterSpeedMultiplier(int selected)
    {
        switch (selected)
        {
            case 1:
                return 0.9f;
            case 2:
                return 1.22f;
            default:
                return 1f;
        }
    }

    float GetCharacterDamageMultiplier(int selected)
    {
        switch (selected)
        {
            case 1:
                return 1.22f;
            case 2:
                return 0.9f;
            default:
                return 1f;
        }
    }

    void ApplyHealthSetting(Health health, int maxHealth)
    {
        if (health == null)
            return;

        health.maxHealth = maxHealth;
        health.currentHealth = maxHealth;
    }

    void ResetAchievementRoundFlags()
    {
        player1TookDamage = false;
        player2TookDamage = false;
        player1PickedBonus = false;
        player2PickedBonus = false;
        player1Overheated = false;
        player2Overheated = false;
        player1ShieldBlocks = 0;
        player2ShieldBlocks = 0;
    }

    void ResetAchievementMatchFlags()
    {
        player1TookDamageThisMatch = false;
        player2TookDamageThisMatch = false;
        player1OverheatedThisMatch = false;
        player2OverheatedThisMatch = false;
        player1CriticalHealthPickup = false;
        player2CriticalHealthPickup = false;
        player1ShieldPickup = false;
        player2ShieldPickup = false;
        player1MatchOverheats = 0;
        player2MatchOverheats = 0;
        player1HitStreak = 0;
        player2HitStreak = 0;
        player1PickupMask = 0;
        player2PickupMask = 0;
        lastPickupPlayer = 0;
        consecutivePickupCount = 0;
        previousRoundWinner = 0;
        player1LastOverheatTime = -99f;
        player2LastOverheatTime = -99f;
        player1LastHealthPickupTime = -99f;
        player2LastHealthPickupTime = -99f;
    }

    void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (paused)
            AudioManager.PauseMusic();
        else
            AudioManager.ResumeMusic();
    }

    void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RegisterDamage(Health damagedHealth, int damageAmount)
    {
        if (damagedHealth == player1Health)
        {
            player1TookDamage = true;
            player1TookDamageThisMatch = true;
            player1HitStreak = 0;
            currentStats.player1DamageTaken += damageAmount;
            currentStats.player2DamageDealt += damageAmount;
            RegisterDamageAchievements(2, damageAmount);
            if (Time.time - player1LastHealthPickupTime <= 1f)
                AchievementProgress.Unlock(1, AchievementProgress.ItsATrap);
        }
        else if (damagedHealth == player2Health)
        {
            player2TookDamage = true;
            player2TookDamageThisMatch = true;
            player2HitStreak = 0;
            currentStats.player2DamageTaken += damageAmount;
            currentStats.player1DamageDealt += damageAmount;
            RegisterDamageAchievements(1, damageAmount);
            if (Time.time - player2LastHealthPickupTime <= 1f)
                AchievementProgress.Unlock(2, AchievementProgress.ItsATrap);
        }
    }

    public void RegisterPickup(Health health, PlayerCombat combat, PickupItem.PickupType pickupType, int healthBeforePickup = -1, float pickupAge = 99f)
    {
        int playerNumber = GetPlayerNumber(health, combat);
        if (playerNumber == 0)
            return;

        if (playerNumber == 1)
        {
            player1PickedBonus = true;
            currentStats.player1Pickups++;
            player1PickupMask |= GetPickupMask(pickupType);
        }
        else
        {
            player2PickedBonus = true;
            currentStats.player2Pickups++;
            player2PickupMask |= GetPickupMask(pickupType);
        }

        RegisterPickupAchievements(playerNumber, health, pickupType, healthBeforePickup, pickupAge);

        if (pickupType == PickupItem.PickupType.Coolant)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.CoolHead);
        else if (pickupType == PickupItem.PickupType.DamageBoost)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.DamageBoost);
    }

    public void RegisterOverheat(PlayerCombat combat)
    {
        int playerNumber = GetPlayerNumber(null, combat);
        if (playerNumber == 1)
        {
            player1Overheated = true;
            player1OverheatedThisMatch = true;
            player1MatchOverheats++;
            player1LastOverheatTime = Time.time;
        }
        else if (playerNumber == 2)
        {
            player2Overheated = true;
            player2OverheatedThisMatch = true;
            player2MatchOverheats++;
            player2LastOverheatTime = Time.time;
        }

        RegisterOverheatAchievements(playerNumber);

        if (Mathf.Abs(player1LastOverheatTime - player2LastOverheatTime) <= 0.6f)
        {
            AchievementProgress.Unlock(1, AchievementProgress.SystemOverload);
            AchievementProgress.Unlock(2, AchievementProgress.SystemOverload);
        }
    }

    public void RegisterAttackHit(PlayerCombat attacker, bool isHeavy, bool afterDoubleJump, bool defeatedEnemy)
    {
        int playerNumber = GetPlayerNumber(null, attacker);
        if (playerNumber == 0)
            return;

        if (afterDoubleJump)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.DoubleJumpHit);

        if (isHeavy && defeatedEnemy)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.HeavyFinish);

        if (playerNumber == 1)
        {
            player1HitStreak++;
            UnlockHitStreakAchievements(1, player1HitStreak);
        }
        else
        {
            player2HitStreak++;
            UnlockHitStreakAchievements(2, player2HitStreak);
        }
    }

    public void RegisterShieldBlock(Health health)
    {
        int playerNumber = GetPlayerNumber(health, null);
        if (playerNumber == 1)
        {
            player1ShieldBlocks++;
            if (player1ShieldBlocks >= 3)
                AchievementProgress.Unlock(1, AchievementProgress.ShieldMaster);
        }
        else if (playerNumber == 2)
        {
            player2ShieldBlocks++;
            if (player2ShieldBlocks >= 3)
                AchievementProgress.Unlock(2, AchievementProgress.ShieldMaster);
        }
    }

    public void RegisterPlatformDrop(PlayerController controller)
    {
        int playerNumber = GetPlayerNumber(controller);
        if (playerNumber != 0)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.PlatformDrop);
    }

    void UnlockWinAchievements(int winnerPlayerNumber)
    {
        AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.FirstRound);

        Health winnerHealth = winnerPlayerNumber == 1 ? player1Health : player2Health;
        bool winnerTookDamage = winnerPlayerNumber == 1 ? player1TookDamage : player2TookDamage;

        if (!winnerTookDamage)
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.CleanWin);

        if (winnerHealth != null && winnerHealth.currentHealth <= 10)
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.LastHp);

        if ((winnerPlayerNumber == 1 && player1PickedBonus) || (winnerPlayerNumber == 2 && player2PickedBonus))
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.BonusWin);

        if ((winnerPlayerNumber == 1 && player1Overheated) || (winnerPlayerNumber == 2 && player2Overheated))
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.OverheatSurvive);
    }

    void UnlockMatchAchievements(int winnerPlayerNumber)
    {
        int loserPlayerNumber = winnerPlayerNumber == 1 ? 2 : 1;
        Health winnerHealth = winnerPlayerNumber == 1 ? player1Health : player2Health;
        bool winnerTookDamage = winnerPlayerNumber == 1 ? player1TookDamageThisMatch : player2TookDamageThisMatch;
        bool winnerOverheated = winnerPlayerNumber == 1 ? player1OverheatedThisMatch : player2OverheatedThisMatch;
        bool loserOverheated = loserPlayerNumber == 1 ? player1OverheatedThisMatch : player2OverheatedThisMatch;
        int loserOverheats = loserPlayerNumber == 1 ? player1MatchOverheats : player2MatchOverheats;
        bool winnerHadCriticalPickup = winnerPlayerNumber == 1 ? player1CriticalHealthPickup : player2CriticalHealthPickup;
        bool winnerHadShieldPickup = winnerPlayerNumber == 1 ? player1ShieldPickup : player2ShieldPickup;

        AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.FirstWin);
        AchievementProgress.AddMatch(1);
        AchievementProgress.AddMatch(2);
        AchievementProgress.AddWin(winnerPlayerNumber);
        AchievementProgress.ResetWinStreak(loserPlayerNumber);

        if (!winnerOverheated)
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.CoolHeadMatch);

        if (loserOverheated)
            AchievementProgress.Unlock(loserPlayerNumber, AchievementProgress.Miscalculated);

        if (loserOverheats >= 3)
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.Terminator);

        if (!winnerTookDamage)
        {
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.PerfectFight);
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.AtticGhost);
        }

        if (winnerHealth != null && winnerHealth.maxHealth > 0 && winnerHealth.currentHealth <= winnerHealth.maxHealth * 0.1f)
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.LastHp);

        if (winnerHadCriticalPickup)
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.LastChance);

        if (winnerHadShieldPickup)
            AchievementProgress.Unlock(winnerPlayerNumber, AchievementProgress.ShieldWin);
    }

    void RegisterDamageAchievements(int attackerPlayerNumber, int damageAmount)
    {
        if (damageAmount <= 0)
            return;

        AchievementProgress.Unlock(attackerPlayerNumber, AchievementProgress.FirstBlood);
        int totalDamage = AchievementProgress.AddCounter(attackerPlayerNumber, AchievementProgress.KnockoutMachine, damageAmount);
        if (totalDamage >= 5000)
            AchievementProgress.Unlock(attackerPlayerNumber, AchievementProgress.KnockoutMachine);
    }

    void RegisterPickupAchievements(int playerNumber, Health health, PickupItem.PickupType pickupType, int healthBeforePickup, float pickupAge)
    {
        if (pickupAge <= 1f)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.HotPotato);

        int pickupCount = AchievementProgress.AddCounter(playerNumber, AchievementProgress.Collector);
        if (pickupCount >= 50)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.Collector);

        if (lastPickupPlayer == playerNumber)
            consecutivePickupCount++;
        else
            consecutivePickupCount = 1;

        lastPickupPlayer = playerNumber;
        if (consecutivePickupCount >= 3)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.Greed);

        int pickupMask = playerNumber == 1 ? player1PickupMask : player2PickupMask;
        if (pickupMask == 15)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.LootHunter);

        if (pickupType == PickupItem.PickupType.Health)
        {
            AchievementProgress.Unlock(playerNumber, AchievementProgress.Medic);
            if (playerNumber == 1)
                player1LastHealthPickupTime = Time.time;
            else
                player2LastHealthPickupTime = Time.time;

            if (health != null && health.maxHealth > 0 && healthBeforePickup >= 0 && healthBeforePickup <= health.maxHealth * 0.15f)
            {
                AchievementProgress.Unlock(playerNumber, AchievementProgress.LastHopePickup);
                if (playerNumber == 1)
                    player1CriticalHealthPickup = true;
                else
                    player2CriticalHealthPickup = true;
            }
        }
        else if (pickupType == PickupItem.PickupType.Shield)
        {
            AchievementProgress.Unlock(playerNumber, AchievementProgress.ShieldBearer);
            if (playerNumber == 1)
                player1ShieldPickup = true;
            else
                player2ShieldPickup = true;
        }
    }

    void RegisterOverheatAchievements(int playerNumber)
    {
        if (playerNumber == 0)
            return;

        AchievementProgress.Unlock(playerNumber, AchievementProgress.FirstOverheat);
        int overheats = AchievementProgress.AddCounter(playerNumber, AchievementProgress.Furnace);
        if (overheats >= 10)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.Furnace);
    }

    void UnlockHitStreakAchievements(int playerNumber, int hitStreak)
    {
        if (hitStreak >= 5)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.HitStreak5);

        if (hitStreak >= 10)
            AchievementProgress.Unlock(playerNumber, AchievementProgress.HitStreak10);
    }

    int GetPickupMask(PickupItem.PickupType pickupType)
    {
        return 1 << (int)pickupType;
    }

    int GetPlayerNumber(Health health, PlayerCombat combat)
    {
        if ((health != null && health == player1Health) || (combat != null && combat == player1Combat))
            return 1;

        if ((health != null && health == player2Health) || (combat != null && combat == player2Combat))
            return 2;

        return 0;
    }

    int GetPlayerNumber(PlayerController controller)
    {
        if (controller == null)
            return 0;

        if (player1Health != null && controller.gameObject == player1Health.gameObject)
            return 1;

        if (player2Health != null && controller.gameObject == player2Health.gameObject)
            return 2;

        return 0;
    }

    void QuitGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsRoundIntroActive = false;
        SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        IsRoundIntroActive = false;
    }

    private class RoundStats
    {
        public int player1DamageDealt;
        public int player2DamageDealt;
        public int player1DamageTaken;
        public int player2DamageTaken;
        public int player1Pickups;
        public int player2Pickups;

        public void Reset()
        {
            player1DamageDealt = 0;
            player2DamageDealt = 0;
            player1DamageTaken = 0;
            player2DamageTaken = 0;
            player1Pickups = 0;
            player2Pickups = 0;
        }
    }

    private class RoundSummary
    {
        public readonly int roundNumber;
        public readonly int winnerPlayer;
        public readonly int player1DamageDealt;
        public readonly int player2DamageDealt;
        public readonly int player1DamageTaken;
        public readonly int player2DamageTaken;
        public readonly int player1Pickups;
        public readonly int player2Pickups;

        public RoundSummary(int roundNumber, int winnerPlayer, RoundStats stats)
        {
            this.roundNumber = roundNumber;
            this.winnerPlayer = winnerPlayer;
            player1DamageDealt = stats.player1DamageDealt;
            player2DamageDealt = stats.player2DamageDealt;
            player1DamageTaken = stats.player1DamageTaken;
            player2DamageTaken = stats.player2DamageTaken;
            player1Pickups = stats.player1Pickups;
            player2Pickups = stats.player2Pickups;
        }
    }
}
