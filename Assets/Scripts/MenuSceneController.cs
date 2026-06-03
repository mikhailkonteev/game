using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneController : MonoBehaviour
{
    public enum MenuScreen
    {
        MainMenu,
        Settings,
        Achievements,
        Shop
    }

    [SerializeField] private MenuScreen screen;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string version = "v0.1.0";

    private const string MainMenuScene = "MainMenu";
    private const string SettingsScene = "SettingsScene";
    private const string AchievementsScene = "AchievementsScene";
    private const string ShopScene = "ShopScene";
    private const string LargeArenaScene = "LargeArenaScene";

    private readonly Color buttonHover = new Color(1.18f, 1.12f, 0.98f, 1f);
    private readonly Color buttonPressed = new Color(0.72f, 0.72f, 0.72f, 1f);
    private readonly Color panelColor = new Color(0.05f, 0.045f, 0.05f, 0.78f);
    private readonly Color textLight = new Color(0.96f, 0.91f, 0.8f, 1f);
    private readonly Dictionary<ControlAction, TextMeshProUGUI> keyLabels = new Dictionary<ControlAction, TextMeshProUGUI>();

    private Canvas canvas;
    private Image fadeImage;
    private TextMeshProUGUI bindingHint;
    private ControlAction? waitingForBinding;
    private float nextUiClickSoundTime;
    private GameObject arenaSelectionOverlay;
    private GameObject characterSelectionOverlay;
    private string selectedArenaScene;
    private int selectedPlayer1Character;
    private int selectedPlayer2Character = 1;
    private bool slowRoundSelected;
    private Toggle smallArenaToggle;
    private Toggle largeArenaToggle;
    private Toggle fastRoundToggle;
    private Toggle slowRoundToggle;
    private int menuButtonCreateIndex;

    private AchievementInfo[] achievements;

    private void Awake()
    {
        Time.timeScale = 1f;
        AudioManager.Ensure();
        EnsureEventSystem();
        CreateRenderCamera();
        CreateCanvas();
        CreateBackground();
        BuildScreen();
        CreateVersionLabel();
        CreateFadeOverlay();
    }

    private IEnumerator Start()
    {
        AudioManager.PlayMenuMusic();
        yield return Fade(1f, 0f, 0.45f);
    }

    private void Update()
    {
        PlayUiMouseClick();

        if (!waitingForBinding.HasValue || !Input.anyKeyDown)
            return;

        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(key) || IsMouseKey(key))
                continue;

            ControlBindings.Set(waitingForBinding.Value, key);
            waitingForBinding = null;
            RefreshKeyLabels();
            if (bindingHint != null)
                bindingHint.text = "Выберите действие и нажмите новую клавишу.";
            PlayClick();
            return;
        }
    }

    private void BuildScreen()
    {
        switch (screen)
        {
            case MenuScreen.MainMenu:
                BuildMainMenu();
                break;
            case MenuScreen.Settings:
                BuildSettings();
                break;
            case MenuScreen.Achievements:
                BuildAchievements();
                break;
            case MenuScreen.Shop:
                BuildShop();
                break;
        }
    }

    private void PlayUiMouseClick()
    {
        if (!Input.GetMouseButtonDown(0) || Time.unscaledTime < nextUiClickSoundTime)
            return;

        if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            return;

        AudioManager.PlayClick();
        nextUiClickSoundTime = Time.unscaledTime + 0.05f;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("MenuCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private void CreateRenderCamera()
    {
        if (Camera.main != null)
        {
            if (FindObjectOfType<AudioListener>() == null)
                Camera.main.gameObject.AddComponent<AudioListener>();
            return;
        }

        GameObject cameraObject = new GameObject("MenuCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.depth = -100f;
        cameraObject.tag = "MainCamera";

        if (FindObjectOfType<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();
    }

    private void CreateBackground()
    {
        GameObject backgroundObject = new GameObject("AtticBackground");
        backgroundObject.transform.SetParent(canvas.transform, false);

        Image background = backgroundObject.AddComponent<Image>();
        background.sprite = Resources.Load<Sprite>("Menu/attic_menu_background");
        background.color = background.sprite != null ? Color.white : new Color(0.02f, 0.018f, 0.02f, 1f);
        background.preserveAspect = false;

        RectTransform rect = background.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void BuildMainMenu()
    {
        CreateTitle(new Vector2(280f, -118f), 112f);

        RectTransform buttonsRoot = CreateEmptyRoot("MainButtons", new Vector2(700f, 710f), new Vector2(-575f, -122f));
        VerticalLayoutGroup layout = buttonsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 22f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        CreateMenuButton(buttonsRoot, "\u041D\u0410\u0427\u0410\u0422\u042C \u0418\u0413\u0420\u0423", "button_start", ShowCharacterSelection, new Vector2(690f, 120f), Vector2.zero);
        CreateMenuButton(buttonsRoot, "НАСТРОЙКИ", "button_settings", () => TransitionTo(SettingsScene), new Vector2(690f, 120f), Vector2.zero);
        CreateMenuButton(buttonsRoot, "ДОСТИЖЕНИЯ", "button_achievements", () => TransitionTo(AchievementsScene), new Vector2(690f, 120f), Vector2.zero);
        CreateMenuButton(buttonsRoot, "МАГАЗИН", "button_shop", () => TransitionTo(ShopScene), new Vector2(690f, 120f), Vector2.zero);
        CreateMenuButton(buttonsRoot, "ВЫХОД", "button_exit", QuitGame, new Vector2(690f, 120f), Vector2.zero);
    }

    private void ShowCharacterSelection()
    {
        bool firstOpen = characterSelectionOverlay == null;
        if (characterSelectionOverlay != null)
            Destroy(characterSelectionOverlay);

        if (firstOpen)
        {
            selectedPlayer1Character = PlayerPrefs.GetInt("Player1Character", 0);
            selectedPlayer2Character = PlayerPrefs.GetInt("Player2Character", 1);
        }

        characterSelectionOverlay = new GameObject("CharacterSelectionOverlay");
        characterSelectionOverlay.transform.SetParent(canvas.transform, false);
        Image overlay = characterSelectionOverlay.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.62f);
        RectTransform overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        RectTransform panel = CreatePanel("CharacterSelectionPanel", new Vector2(1180f, 650f), Vector2.zero, 0.94f);
        panel.SetParent(characterSelectionOverlay.transform, false);

        CreateText(panel, "ВЫБОР ПЕРСОНАЖЕЙ", 46f, new Vector2(0f, 260f), new Vector2(760f, 60f), TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
        CreateText(panel, "PLAYER 1", 34f, new Vector2(-315f, 198f), new Vector2(260f, 44f), TextAlignmentOptions.Center).color = new Color(0.35f, 0.56f, 1f, 1f);
        CreateText(panel, "PLAYER 2", 34f, new Vector2(315f, 198f), new Vector2(260f, 44f), TextAlignmentOptions.Center).color = new Color(1f, 0.35f, 0.28f, 1f);

        CreateCharacterColumn(panel, -315f, true);
        CreateCharacterColumn(panel, 315f, false);

        CreateMenuButton(panel, "ДАЛЬШЕ", "button_start", ConfirmCharacters, new Vector2(300f, 72f), new Vector2(-170f, -260f));
        CreateMenuButton(panel, "НАЗАД", "button_exit", CloseCharacterSelection, new Vector2(300f, 72f), new Vector2(170f, -260f));
    }

    private void CreateCharacterColumn(Transform parent, float x, bool playerOne)
    {
        CreateCharacterCard(parent, x, 88f, playerOne, 0, "БАЛАНС", "Ровный стиль без слабых мест", new Color(0.25f, 0.48f, 1f, 1f));
        CreateCharacterCard(parent, x, -48f, playerOne, 1, "СИЛЬНЫЙ", "Напор и тяжелые удары", new Color(1f, 0.28f, 0.22f, 1f));
        CreateCharacterCard(parent, x, -184f, playerOne, 2, "БЫСТРЫЙ", "Темп, маневры и бонусы", new Color(1f, 0.72f, 0.18f, 1f));
    }

    private void CreateCharacterCard(Transform parent, float x, float y, bool playerOne, int index, string title, string description, Color accent)
    {
        bool selected = playerOne ? selectedPlayer1Character == index : selectedPlayer2Character == index;
        GameObject cardObject = new GameObject(title);
        cardObject.transform.SetParent(parent, false);
        Image card = cardObject.AddComponent<Image>();
        card.color = selected ? new Color(accent.r * 0.34f, accent.g * 0.34f, accent.b * 0.34f, 0.96f) : new Color(0.08f, 0.075f, 0.08f, 0.96f);

        Button button = cardObject.AddComponent<Button>();
        button.targetGraphic = card;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() =>
        {
            if (playerOne)
                selectedPlayer1Character = index;
            else
                selectedPlayer2Character = index;

            PlayClick();
            ShowCharacterSelection();
        });

        RectTransform rect = card.rectTransform;
        rect.sizeDelta = new Vector2(460f, 112f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);

        Image portrait = CreateRect(cardObject.transform, "Portrait", accent, new Vector2(-168f, 0f), new Vector2(72f, 72f));
        portrait.raycastTarget = false;
        CreateText(cardObject.transform, title, 28f, new Vector2(40f, 20f), new Vector2(290f, 34f), TextAlignmentOptions.Left).fontStyle = FontStyles.Bold;
        CreateText(cardObject.transform, description, 20f, new Vector2(40f, -22f), new Vector2(290f, 42f), TextAlignmentOptions.Left);

        if (selected)
            CreateText(cardObject.transform, "ВЫБРАН", 18f, new Vector2(172f, -42f), new Vector2(120f, 24f), TextAlignmentOptions.Right).color = accent;

    }

    private void ConfirmCharacters()
    {
        PlayerPrefs.SetInt("Player1Character", selectedPlayer1Character);
        PlayerPrefs.SetInt("Player2Character", selectedPlayer2Character);
        PlayerPrefs.Save();
        CloseCharacterSelection();
        ShowArenaSelection();
    }

    private void CloseCharacterSelection()
    {
        if (characterSelectionOverlay == null)
            return;

        Destroy(characterSelectionOverlay);
        characterSelectionOverlay = null;
    }


    private void ShowArenaSelection()
    {
        if (arenaSelectionOverlay != null)
            Destroy(arenaSelectionOverlay);

        selectedArenaScene = gameSceneName;
        slowRoundSelected = false;

        arenaSelectionOverlay = new GameObject("ArenaSelectionOverlay");
        arenaSelectionOverlay.transform.SetParent(canvas.transform, false);
        Image overlay = arenaSelectionOverlay.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.58f);
        RectTransform overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        RectTransform panel = CreatePanel("ArenaSelectionPanel", new Vector2(820f, 560f), Vector2.zero, 0.94f);
        panel.SetParent(arenaSelectionOverlay.transform, false);

        CreateText(panel, "\u0412\u042B\u0411\u041E\u0420 \u0410\u0420\u0415\u041D\u042B", 44f, new Vector2(0f, 206f), new Vector2(680f, 58f), TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
        CreateText(panel, "\u0410\u0440\u0435\u043d\u0430", 34f, new Vector2(-250f, 118f), new Vector2(230f, 46f), TextAlignmentOptions.Left).fontStyle = FontStyles.Bold;

        smallArenaToggle = CreateSelectionToggle(panel, "\u041c\u0430\u043b\u0435\u043d\u044c\u043a\u0430\u044f", new Vector2(30f, 120f), true, value =>
        {
            if (!value)
                return;

            selectedArenaScene = gameSceneName;
            if (largeArenaToggle != null)
                largeArenaToggle.SetIsOnWithoutNotify(false);
        });

        largeArenaToggle = CreateSelectionToggle(panel, "\u0411\u043e\u043b\u044c\u0448\u0430\u044f", new Vector2(30f, 62f), false, value =>
        {
            if (!value)
                return;

            selectedArenaScene = LargeArenaScene;
            if (smallArenaToggle != null)
                smallArenaToggle.SetIsOnWithoutNotify(false);
        });

        CreateText(panel, "\u0421\u043a\u043e\u0440\u043e\u0441\u0442\u044c \u0440\u0430\u0443\u043d\u0434\u0430", 30f, new Vector2(-265f, -18f), new Vector2(300f, 46f), TextAlignmentOptions.Left).fontStyle = FontStyles.Bold;

        fastRoundToggle = CreateSelectionToggle(panel, "\u0411\u044b\u0441\u0442\u0440\u044b\u0439", new Vector2(190f, -18f), true, value =>
        {
            if (!value)
                return;

            slowRoundSelected = false;
            if (slowRoundToggle != null)
                slowRoundToggle.SetIsOnWithoutNotify(false);
        });

        slowRoundToggle = CreateSelectionToggle(panel, "\u041c\u0435\u0434\u043b\u0435\u043d\u043d\u044b\u0439", new Vector2(190f, -76f), false, value =>
        {
            if (!value)
                return;

            slowRoundSelected = true;
            if (fastRoundToggle != null)
                fastRoundToggle.SetIsOnWithoutNotify(false);
        });

        CreateMenuButton(panel, "\u0418\u0413\u0420\u0410\u0422\u042c", "button_start", StartSelectedArena, new Vector2(300f, 72f), new Vector2(-165f, -214f));
        CreateMenuButton(panel, "\u041d\u0410\u0417\u0410\u0414", "button_exit", CloseArenaSelection, new Vector2(300f, 72f), new Vector2(165f, -214f));
    }

    private Toggle CreateSelectionToggle(Transform parent, string label, Vector2 position, bool isOn, Action<bool> onChanged)
    {
        GameObject toggleObject = new GameObject(label);
        toggleObject.transform.SetParent(parent, false);
        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.isOn = isOn;
        toggle.onValueChanged.AddListener(value => onChanged(value));

        RectTransform rect = toggle.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(360f, 46f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;

        Image background = CreateRect(toggleObject.transform, "Background", new Color(0.08f, 0.08f, 0.09f, 0.95f), new Vector2(-155f, 0f), new Vector2(38f, 38f));
        Image check = CreateRect(toggleObject.transform, "Checkmark", new Color(0.95f, 0.55f, 0.18f, 1f), new Vector2(-155f, 0f), new Vector2(24f, 24f));
        toggle.targetGraphic = background;
        toggle.graphic = check;

        CreateText(toggleObject.transform, label, 30f, new Vector2(34f, 0f), new Vector2(285f, 42f), TextAlignmentOptions.Left);
        return toggle;
    }

    private void StartSelectedArena()
    {
        PlayerPrefs.SetFloat(GameManager.RoundHealthMultiplierPreference, slowRoundSelected ? 2f : 1f);
        PlayerPrefs.Save();
        TransitionTo(string.IsNullOrEmpty(selectedArenaScene) ? gameSceneName : selectedArenaScene);
    }

    private void CloseArenaSelection()
    {
        if (arenaSelectionOverlay == null)
            return;

        Destroy(arenaSelectionOverlay);
        arenaSelectionOverlay = null;
    }
    private void BuildSettings()
    {
        CreateTitle(new Vector2(0f, -72f), 78f);
        CreateSectionHeading("НАСТРОЙКИ", new Vector2(0f, -168f));

        RectTransform panel = CreatePanel("SettingsPanel", new Vector2(1320f, 820f), new Vector2(0f, -35f), 0.82f);
        CreateText(panel, "Управление", 42f, new Vector2(0f, 342f), new Vector2(560f, 52f), TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;

        CreateText(panel, "Игрок 1", 34f, new Vector2(-330f, 294f), new Vector2(280f, 42f), TextAlignmentOptions.Center).color = new Color(0.35f, 0.56f, 1f, 1f);
        CreateText(panel, "Игрок 2", 34f, new Vector2(330f, 294f), new Vector2(280f, 42f), TextAlignmentOptions.Center).color = new Color(1f, 0.35f, 0.28f, 1f);

        CreateBindingColumn(panel, -330f, new[]
        {
            ControlAction.Player1Left,
            ControlAction.Player1Right,
            ControlAction.Player1Jump,
            ControlAction.Player1Drop,
            ControlAction.Player1LightAttack,
            ControlAction.Player1HeavyAttack
        });
        CreateBindingColumn(panel, 330f, new[]
        {
            ControlAction.Player2Left,
            ControlAction.Player2Right,
            ControlAction.Player2Jump,
            ControlAction.Player2Drop,
            ControlAction.Player2LightAttack,
            ControlAction.Player2HeavyAttack
        });

        bindingHint = CreateText(panel, "Выберите действие и нажмите новую клавишу.", 28f, new Vector2(0f, -92f), new Vector2(860f, 42f), TextAlignmentOptions.Center);
        CreateFullscreenToggle(panel, -148f);
        CreateMenuButton(panel, "ПО УМОЛЧАНИЮ", "button_settings", ResetControls, new Vector2(430f, 74f), new Vector2(0f, -214f));
        CreateSliderRow(panel, "Музыка", -300f, AudioManager.MusicVolume, value =>
        {
            AudioManager.MusicVolume = value;
        });
        CreateSliderRow(panel, "Звуки", -365f, AudioManager.SfxVolume, value =>
        {
            AudioManager.SfxVolume = value;
        });
        CreateMenuButton(panel, "НАЗАД", "button_exit", () => TransitionTo(MainMenuScene), new Vector2(330f, 64f), new Vector2(0f, -448f));
        RefreshKeyLabels();
    }

    private void BuildAchievements()
    {
        CreateTitle(new Vector2(0f, -65f), 74f);
        CreateSectionHeading("ДОСТИЖЕНИЯ", new Vector2(0f, -150f));

        RectTransform panel = CreatePanel("AchievementsPanel", new Vector2(1540f, 840f), new Vector2(0f, -34f), 0.82f);
        achievements = CreateAchievements();

        CreateText(panel, "PLAYER 1", 34f, new Vector2(-390f, 362f), new Vector2(300f, 42f), TextAlignmentOptions.Center).color = new Color(0.35f, 0.56f, 1f, 1f);
        CreateText(panel, "PLAYER 2", 34f, new Vector2(390f, 362f), new Vector2(300f, 42f), TextAlignmentOptions.Center).color = new Color(1f, 0.35f, 0.28f, 1f);

        GameObject scrollObject = new GameObject("AchievementsScroll");
        scrollObject.transform.SetParent(panel, false);
        RectTransform scrollRectTransform = scrollObject.AddComponent<RectTransform>();
        scrollRectTransform.sizeDelta = new Vector2(1500f, 650f);
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = new Vector2(0f, -20f);
        Image viewportImage = scrollObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.04f);
        Mask mask = scrollObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("AchievementsContent");
        contentObject.transform.SetParent(scrollObject.transform, false);
        RectTransform content = contentObject.AddComponent<RectTransform>();
        float contentHeight = CalculateAchievementsContentHeight(achievements);
        content.sizeDelta = new Vector2(1500f, contentHeight);
        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;

        ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = scrollRectTransform;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 42f;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        float y = -24f;
        string currentCategory = null;
        for (int i = 0; i < achievements.Length; i++)
        {
            AchievementInfo achievement = achievements[i];
            if (currentCategory != achievement.Category)
            {
                currentCategory = achievement.Category;
                TextMeshProUGUI categoryLabel = CreateText(content, currentCategory, 28f, new Vector2(0f, y), new Vector2(1260f, 38f), TextAlignmentOptions.Left);
                categoryLabel.fontStyle = FontStyles.Bold;
                categoryLabel.rectTransform.anchorMin = new Vector2(0.5f, 1f);
                categoryLabel.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                categoryLabel.rectTransform.pivot = new Vector2(0.5f, 1f);
                categoryLabel.rectTransform.anchoredPosition = new Vector2(0f, y);
                y -= 42f;
            }

            CreateAchievementCard(content, achievement, 1, new Vector2(-390f, y));
            CreateAchievementCard(content, achievement, 2, new Vector2(390f, y));
            y -= 84f;
        }

        CreateMenuButton(panel, "СБРОСИТЬ ДОСТИЖЕНИЯ", "button_exit", ResetAchievements, new Vector2(500f, 62f), new Vector2(-270f, -388f));
        CreateMenuButton(panel, "НАЗАД", "button_settings", () => TransitionTo(MainMenuScene), new Vector2(340f, 62f), new Vector2(285f, -388f));
    }

    private float CalculateAchievementsContentHeight(AchievementInfo[] infos)
    {
        float height = 60f;
        string currentCategory = null;
        foreach (AchievementInfo info in infos)
        {
            if (currentCategory != info.Category)
            {
                currentCategory = info.Category;
                height += 42f;
            }

            height += 84f;
        }

        return height;
    }
    private void BuildShop()
    {
        CreateTitle(new Vector2(0f, -92f), 84f);
        CreateSectionHeading("МАГАЗИН", new Vector2(0f, -208f));

        RectTransform panel = CreatePanel("ShopPanel", new Vector2(930f, 420f), new Vector2(0f, -25f), 0.82f);
        CreateText(panel, "Заготовка под будущий магазин", 38f, new Vector2(0f, 95f), new Vector2(760f, 54f), TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
        CreateText(panel, "Позже сюда можно добавить скины, визуальные эффекты, фоны арены и другие косметические предметы.", 28f, new Vector2(0f, 5f), new Vector2(760f, 110f), TextAlignmentOptions.Center);
        CreateMenuButton(panel, "НАЗАД", "button_settings", () => TransitionTo(MainMenuScene), new Vector2(330f, 66f), new Vector2(0f, -145f));
    }

    private void CreateBindingColumn(Transform parent, float x, ControlAction[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            ControlAction action = actions[i];
            float y = 238f - i * 52f;
            string label = ControlBindings.GetLabel(action).Replace("Игрок 1: ", "").Replace("Игрок 2: ", "");
            CreateText(parent, label, 26f, new Vector2(x - 130f, y), new Vector2(290f, 40f), TextAlignmentOptions.Left);

            TextMeshProUGUI keyLabel = null;
            Button button = CreateMenuButton(parent, "", "button_settings", () => StartRebind(action), new Vector2(180f, 46f), new Vector2(x + 160f, y));
            keyLabel = button.GetComponentInChildren<TextMeshProUGUI>();
            keyLabels[action] = keyLabel;
        }
    }

    private void StartRebind(ControlAction action)
    {
        waitingForBinding = action;
        if (bindingHint != null)
            bindingHint.text = "Нажмите клавишу для: " + ControlBindings.GetLabel(action);
    }

    private void ResetControls()
    {
        ControlBindings.ResetToDefaults();
        RefreshKeyLabels();
        if (bindingHint != null)
            bindingHint.text = "Настройки управления сброшены.";
    }

    private void RefreshKeyLabels()
    {
        foreach (KeyValuePair<ControlAction, TextMeshProUGUI> pair in keyLabels)
        {
            pair.Value.text = ControlBindings.GetKeyName(ControlBindings.Get(pair.Key));
        }
    }

    private void CreateAchievementCard(Transform parent, AchievementInfo achievement, int playerNumber, Vector2 position)
    {
        GameObject cardObject = new GameObject("AchievementCard");
        cardObject.transform.SetParent(parent, false);
        Image card = cardObject.AddComponent<Image>();
        bool unlocked = AchievementProgress.IsUnlocked(playerNumber, achievement.Id);
        card.color = unlocked ? new Color(0.18f, 0.28f, 0.18f, 0.95f) : new Color(0.09f, 0.085f, 0.09f, 0.95f);

        RectTransform rect = card.rectTransform;
        rect.sizeDelta = new Vector2(720f, 78f);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;

        Color accent = unlocked ? new Color(0.95f, 0.65f, 0.18f, 1f) : new Color(0.35f, 0.35f, 0.38f, 1f);
        Image marker = CreateRect(cardObject.transform, "Marker", accent, new Vector2(-348f, -1f), new Vector2(7f, 76f));
        marker.raycastTarget = false;

        TextMeshProUGUI title = CreateText(cardObject.transform, achievement.Title, 24f, new Vector2(-135f, 19f), new Vector2(430f, 30f), TextAlignmentOptions.Left);
        title.fontStyle = FontStyles.Bold;
        title.enableAutoSizing = false;
        title.fontSize = 24f;

        string conditionText = achievement.IsSecret && !unlocked ? "Секретное достижение" : achievement.Condition;
        TextMeshProUGUI condition = CreateText(cardObject.transform, conditionText, 20f, new Vector2(18f, -17f), new Vector2(635f, 42f), TextAlignmentOptions.Left);
        condition.enableAutoSizing = false;
        condition.fontSize = 20f;
        condition.enableWordWrapping = true;
        condition.overflowMode = TextOverflowModes.Ellipsis;

        TextMeshProUGUI status = CreateText(cardObject.transform, unlocked ? "ПОЛУЧЕНО" : "НЕ ПОЛУЧЕНО", 18f, new Vector2(250f, 18f), new Vector2(210f, 24f), TextAlignmentOptions.Right);
        status.color = accent;
        status.enableAutoSizing = false;
        status.fontSize = 18f;
    }
    private AchievementInfo[] CreateAchievements()
    {
        return new[]
        {
            new AchievementInfo("БАЗОВЫЕ ДОСТИЖЕНИЯ", AchievementProgress.FirstBlood, "Первая кровь", "Нанести первый удар по противнику."),
            new AchievementInfo("БАЗОВЫЕ ДОСТИЖЕНИЯ", AchievementProgress.FirstRound, "Первый раунд", "Выиграть первый раунд."),
            new AchievementInfo("БАЗОВЫЕ ДОСТИЖЕНИЯ", AchievementProgress.FirstWin, "Первая победа", "Выиграть первый матч."),
            new AchievementInfo("БАЗОВЫЕ ДОСТИЖЕНИЯ", AchievementProgress.AtticVeteran50, "Ветеран чердака", "Провести 50 матчей."),
            new AchievementInfo("БАЗОВЫЕ ДОСТИЖЕНИЯ", AchievementProgress.AtticRegular100, "Завсегдатай чердака", "Провести 100 матчей."),

            new AchievementInfo("ДОСТИЖЕНИЯ ПЕРЕГРЕВА", AchievementProgress.FirstOverheat, "Осторожно, горячо", "Впервые перегреться."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЕРЕГРЕВА", AchievementProgress.Furnace, "Плавильная печь", "Перегреться 10 раз за все время."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЕРЕГРЕВА", AchievementProgress.CoolHeadMatch, "Хладнокровный", "Выиграть матч ни разу не перегревшись."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЕРЕГРЕВА", AchievementProgress.Miscalculated, "Не рассчитал", "Проиграть матч после перегрева."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЕРЕГРЕВА", AchievementProgress.Terminator, "Терминатор", "Выиграть матч, заставив противника перегреться не менее 3 раз."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЕРЕГРЕВА", AchievementProgress.SystemOverload, "Перегрузка системы", "Одновременно перегреться обоим игрокам."),

            new AchievementInfo("ДОСТИЖЕНИЯ БОНУСОВ", AchievementProgress.Medic, "Медик", "Подобрать первую аптечку."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОНУСОВ", AchievementProgress.ShieldBearer, "Щитоносец", "Подобрать первый щит."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОНУСОВ", AchievementProgress.Collector, "Коллекционер", "Подобрать 50 бонусов."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОНУСОВ", AchievementProgress.LootHunter, "Охотник за лутом", "Подобрать все типы бонусов в одном матче."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОНУСОВ", AchievementProgress.LastHopePickup, "Последняя надежда", "Подобрать аптечку при здоровье менее 15%."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОНУСОВ", AchievementProgress.ShieldWin, "Непробиваемый", "Выиграть матч под действием щита."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОНУСОВ", AchievementProgress.Greed, "Жадность", "Подобрать 3 бонуса подряд раньше противника."),

            new AchievementInfo("ДОСТИЖЕНИЯ ПЛАТФОРМ", AchievementProgress.Climber, "Верхолаз", "Добраться до верхней платформы."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЛАТФОРМ", AchievementProgress.HeightKing, "Король высоты", "Провести на верхних платформах суммарно 2 минуты."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЛАТФОРМ", AchievementProgress.Acrobat, "Акробат", "Перепрыгнуть противника 25 раз."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЛАТФОРМ", AchievementProgress.ParkourRunner, "Паркурщик", "Побывать на всех платформах арены за один раунд."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЛАТФОРМ", AchievementProgress.AtticLord, "Властелин чердака", "Выиграть матч, не спускаясь на нижний уровень."),
            new AchievementInfo("ДОСТИЖЕНИЯ ПЛАТФОРМ", AchievementProgress.FloorIsLava, "Пол - лава", "Выиграть раунд, проведя на нижней платформе менее 10 секунд."),

            new AchievementInfo("ДОСТИЖЕНИЯ БОЕВОЙ СИСТЕМЫ", AchievementProgress.HitStreak5, "Серия ударов", "Нанести 5 ударов подряд."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОЕВОЙ СИСТЕМЫ", AchievementProgress.HitStreak10, "Неудержимый", "Нанести 10 ударов подряд."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОЕВОЙ СИСТЕМЫ", AchievementProgress.KnockoutMachine, "Машина для нокаутов", "Нанести 5000 урона суммарно."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОЕВОЙ СИСТЕМЫ", AchievementProgress.Sniper, "Снайпер", "Попасть каждым ударом в течение одного раунда."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОЕВОЙ СИСТЕМЫ", AchievementProgress.CounterAttack, "Контрудар", "Победить после проигранного предыдущего раунда."),
            new AchievementInfo("ДОСТИЖЕНИЯ БОЕВОЙ СИСТЕМЫ", AchievementProgress.PerfectFight, "Идеальный бой", "Выиграть матч без получения урона."),

            new AchievementInfo("ДОСТИЖЕНИЯ ВЫЖИВАНИЯ", AchievementProgress.LastHp, "На волоске от смерти", "Выиграть матч с HP ниже 10%."),
            new AchievementInfo("ДОСТИЖЕНИЯ ВЫЖИВАНИЯ", AchievementProgress.Comeback, "Камбэк", "Выиграть матч, имея меньше здоровья, чем противник, большую часть игры."),
            new AchievementInfo("ДОСТИЖЕНИЯ ВЫЖИВАНИЯ", AchievementProgress.Immortal, "Бессмертный", "Выиграть 5 матчей подряд."),
            new AchievementInfo("ДОСТИЖЕНИЯ ВЫЖИВАНИЯ", AchievementProgress.LastChance, "Последний шанс", "Победить после аптечки на критическом здоровье."),

            new AchievementInfo("СЕКРЕТНЫЕ ДОСТИЖЕНИЯ", AchievementProgress.AtticGhost, "Чердачный призрак", "Не получить ни одного удара за весь матч.", true),
            new AchievementInfo("СЕКРЕТНЫЕ ДОСТИЖЕНИЯ", AchievementProgress.ProfessionalCoward, "Профессиональный трус", "Выиграть матч, используя только бонусы и добивающие удары.", true),
            new AchievementInfo("СЕКРЕТНЫЕ ДОСТИЖЕНИЯ", AchievementProgress.HotPotato, "Горячая картошка", "Подобрать бонус менее чем через 1 секунду после его появления.", true),
            new AchievementInfo("СЕКРЕТНЫЕ ДОСТИЖЕНИЯ", AchievementProgress.ItsATrap, "Это ловушка!", "Получить урон сразу после подбора аптечки.", true)
        };
    }

    private void ResetAchievements()
    {
        AchievementProgress.ResetAll();
        TransitionTo(AchievementsScene);
    }

    private RectTransform CreateEmptyRoot(string name, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject rootObject = new GameObject(name);
        rootObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = rootObject.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        return rect;
    }

    private void CreateTitle(Vector2 anchoredPosition, float size)
    {
        TextMeshProUGUI title = CreateText(canvas.transform, "ATTIC\nFIGHT", size, anchoredPosition, new Vector2(690f, 240f), TextAlignmentOptions.Center);
        title.fontStyle = FontStyles.Bold;
        title.color = new Color(1f, 0.78f, 0.22f, 1f);
        title.outlineWidth = 0.18f;
        title.outlineColor = new Color(0.1f, 0.04f, 0.02f, 1f);
        title.lineSpacing = -20f;
    }

    private void CreateSectionHeading(string text, Vector2 anchoredPosition)
    {
        TextMeshProUGUI heading = CreateText(canvas.transform, text, 44f, anchoredPosition, new Vector2(760f, 66f), TextAlignmentOptions.Center);
        heading.fontStyle = FontStyles.Bold;
        heading.color = textLight;
    }

    private RectTransform CreatePanel(string name, Vector2 size, Vector2 anchoredPosition, float alpha)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(canvas.transform, false);

        Image image = panelObject.AddComponent<Image>();
        image.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);

        RectTransform rect = image.rectTransform;
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        return rect;
    }

    private Button CreateMenuButton(Transform parent, string label, string spriteName, Action action, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(label.Length > 0 ? label : "KeyButton");
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        Sprite sprite = Resources.Load<Sprite>("Menu/" + spriteName);
        image.sprite = sprite;
        image.color = Color.white;
        image.type = Image.Type.Simple;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() =>
        {
            PlayClick();
            action?.Invoke();
        });

        RectTransform rect = image.rectTransform;
        rect.sizeDelta = size;
        if (anchoredPosition != Vector2.zero)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
        }

        TextMeshProUGUI text = CreateText(buttonObject.transform, label, Mathf.Min(34f, size.y * 0.44f), Vector2.zero, size, TextAlignmentOptions.Center);
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.outlineWidth = 0.12f;
        text.outlineColor = Color.black;

        AddButtonFeedback(buttonObject, image);
        StartCoroutine(AnimateButtonAppear(buttonObject.transform, menuButtonCreateIndex++ * 0.035f));
        return button;
    }

    private IEnumerator AnimateButtonAppear(Transform target, float delay)
    {
        target.localScale = Vector3.one * 0.88f;

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float duration = 0.18f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.Lerp(Vector3.one * 0.88f, Vector3.one, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    private void AddButtonFeedback(GameObject target, Image image)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();
        AddTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            image.color = buttonHover;
            target.transform.localScale = Vector3.one * 1.07f;
            PlayHover();
        });
        AddTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            image.color = Color.white;
            target.transform.localScale = Vector3.one;
        });
        AddTrigger(trigger, EventTriggerType.PointerDown, () =>
        {
            image.color = buttonPressed;
            target.transform.localScale = Vector3.one * 0.94f;
        });
        AddTrigger(trigger, EventTriggerType.PointerUp, () =>
        {
            image.color = buttonHover;
            target.transform.localScale = Vector3.one * 1.07f;
        });
    }

    private void AddTrigger(EventTrigger trigger, EventTriggerType type, Action action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    private void CreateSliderRow(Transform parent, string label, float y, float value, Action<float> onChanged)
    {
        CreateText(parent, label, 30f, new Vector2(-405f, y), new Vector2(210f, 42f), TextAlignmentOptions.Left);

        GameObject sliderObject = new GameObject(label + "Slider");
        sliderObject.transform.SetParent(parent, false);
        Image hitArea = sliderObject.AddComponent<Image>();
        hitArea.color = new Color(1f, 1f, 1f, 0f);
        hitArea.raycastTarget = true;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.onValueChanged.AddListener(v => onChanged(v));

        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(560f, 40f);
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(150f, y);

        Image trackShadow = CreateRect(sliderObject.transform, "TrackShadow", new Color(0.06f, 0.045f, 0.03f, 0.95f), Vector2.zero, new Vector2(576f, 24f));
        Image track = CreateRect(sliderObject.transform, "Track", new Color(0.95f, 0.55f, 0.18f, 1f), Vector2.zero, new Vector2(560f, 14f));
        RectTransform handleArea = CreateRectTransform(sliderObject.transform, "Handle Slide Area", Vector2.zero, new Vector2(560f, 40f));
        Image handle = CreateRect(handleArea, "Handle", textLight, Vector2.zero, new Vector2(28f, 38f));
        slider.direction = Slider.Direction.LeftToRight;
        slider.targetGraphic = handle;
        slider.fillRect = null;
        slider.handleRect = handle.rectTransform;
        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
        trackShadow.raycastTarget = false;
        track.raycastTarget = false;
    }

    private RectTransform CreateRectTransform(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject rectObject = new GameObject(name);
        rectObject.transform.SetParent(parent, false);
        RectTransform rect = rectObject.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        return rect;
    }

    private void CreateFullscreenToggle(Transform parent, float y)
    {
        CreateText(parent, "Полный экран", 24f, new Vector2(-165f, y), new Vector2(230f, 40f), TextAlignmentOptions.Right);

        GameObject toggleObject = new GameObject("FullscreenToggle");
        toggleObject.transform.SetParent(parent, false);
        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.isOn = Screen.fullScreen;
        toggle.onValueChanged.AddListener(value => Screen.fullScreen = value);

        RectTransform toggleRect = toggle.GetComponent<RectTransform>();
        toggleRect.sizeDelta = new Vector2(42f, 42f);
        toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
        toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
        toggleRect.anchoredPosition = new Vector2(0f, y);

        Image background = CreateRect(toggleObject.transform, "Background", new Color(0.08f, 0.08f, 0.09f, 0.95f), Vector2.zero, new Vector2(42f, 42f));
        Image check = CreateRect(toggleObject.transform, "Checkmark", new Color(0.95f, 0.55f, 0.18f, 1f), Vector2.zero, new Vector2(26f, 26f));
        toggle.targetGraphic = background;
        toggle.graphic = check;
    }

    private Image CreateRect(Transform parent, string name, Color color, Vector2 position, Vector2 size)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        RectTransform rect = image.rectTransform;
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        return image;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, float size, Vector2 anchoredPosition, Vector2 boxSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(18f, size * 0.78f);
        label.fontSizeMax = size;
        label.alignment = alignment;
        label.color = textLight;
        label.raycastTarget = false;
        label.extraPadding = true;
        label.fontMaterial.EnableKeyword("UNDERLAY_ON");
        label.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.65f));
        label.fontMaterial.SetFloat("_UnderlayOffsetX", 0.55f);
        label.fontMaterial.SetFloat("_UnderlayOffsetY", -0.55f);

        RectTransform rect = label.rectTransform;
        rect.sizeDelta = boxSize;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        return label;
    }

    private void CreateVersionLabel()
    {
        TextMeshProUGUI label = CreateText(canvas.transform, version, 22f, Vector2.zero, new Vector2(190f, 38f), TextAlignmentOptions.Right);
        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-28f, 24f);
    }

    private void CreateFadeOverlay()
    {
        GameObject fadeObject = new GameObject("FadeOverlay");
        fadeObject.transform.SetParent(canvas.transform, false);
        fadeImage = fadeObject.AddComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = true;

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void TransitionTo(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        yield return Fade(0f, 1f, 0.42f);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            fadeImage.color = color;
            yield return null;
        }

        color.a = to;
        fadeImage.color = color;
        fadeImage.raycastTarget = to > 0.01f;
    }

    private void PlayHover()
    {
    }

    private void PlayClick()
    {
        if (Time.unscaledTime < nextUiClickSoundTime)
            return;

        AudioManager.PlayClick();
        nextUiClickSoundTime = Time.unscaledTime + 0.05f;
    }

    private void QuitGame()
    {
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        yield return Fade(0f, 1f, 0.35f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private bool IsMouseKey(KeyCode key)
    {
        return key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6;
    }

    private class AchievementInfo
    {
        public readonly string Category;
        public readonly string Id;
        public readonly string Title;
        public readonly string Condition;
        public readonly bool IsSecret;

        public AchievementInfo(string category, string id, string title, string condition, bool isSecret = false)
        {
            Category = category;
            Id = id;
            Title = title;
            Condition = condition;
            IsSecret = isSecret;
        }
    }
}
