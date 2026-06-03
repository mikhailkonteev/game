using UnityEngine;

public enum ControlAction
{
    Player1Left,
    Player1Right,
    Player1Jump,
    Player1Drop,
    Player1LightAttack,
    Player1HeavyAttack,
    Player2Left,
    Player2Right,
    Player2Jump,
    Player2Drop,
    Player2LightAttack,
    Player2HeavyAttack
}

public static class ControlBindings
{
    private const string PrefPrefix = "control_";

    public static readonly ControlAction[] AllActions =
    {
        ControlAction.Player1Left,
        ControlAction.Player1Right,
        ControlAction.Player1Jump,
        ControlAction.Player1Drop,
        ControlAction.Player1LightAttack,
        ControlAction.Player1HeavyAttack,
        ControlAction.Player2Left,
        ControlAction.Player2Right,
        ControlAction.Player2Jump,
        ControlAction.Player2Drop,
        ControlAction.Player2LightAttack,
        ControlAction.Player2HeavyAttack
    };

    public static KeyCode Get(ControlAction action)
    {
        return (KeyCode)PlayerPrefs.GetInt(GetPrefKey(action), (int)GetDefault(action));
    }

    public static void Set(ControlAction action, KeyCode key)
    {
        PlayerPrefs.SetInt(GetPrefKey(action), (int)key);
        PlayerPrefs.Save();
    }

    public static void ResetToDefaults()
    {
        foreach (ControlAction action in AllActions)
            PlayerPrefs.DeleteKey(GetPrefKey(action));

        PlayerPrefs.Save();
    }

    public static KeyCode GetDefault(ControlAction action)
    {
        switch (action)
        {
            case ControlAction.Player1Left: return KeyCode.A;
            case ControlAction.Player1Right: return KeyCode.D;
            case ControlAction.Player1Jump: return KeyCode.W;
            case ControlAction.Player1Drop: return KeyCode.S;
            case ControlAction.Player1LightAttack: return KeyCode.F;
            case ControlAction.Player1HeavyAttack: return KeyCode.G;
            case ControlAction.Player2Left: return KeyCode.LeftArrow;
            case ControlAction.Player2Right: return KeyCode.RightArrow;
            case ControlAction.Player2Jump: return KeyCode.UpArrow;
            case ControlAction.Player2Drop: return KeyCode.DownArrow;
            case ControlAction.Player2LightAttack: return KeyCode.K;
            case ControlAction.Player2HeavyAttack: return KeyCode.L;
            default: return KeyCode.None;
        }
    }

    public static string GetLabel(ControlAction action)
    {
        switch (action)
        {
            case ControlAction.Player1Left: return "Игрок 1: влево";
            case ControlAction.Player1Right: return "Игрок 1: вправо";
            case ControlAction.Player1Jump: return "Игрок 1: прыжок";
            case ControlAction.Player1Drop: return "Игрок 1: вниз с платформы";
            case ControlAction.Player1LightAttack: return "Игрок 1: удар";
            case ControlAction.Player1HeavyAttack: return "Игрок 1: сильный удар";
            case ControlAction.Player2Left: return "Игрок 2: влево";
            case ControlAction.Player2Right: return "Игрок 2: вправо";
            case ControlAction.Player2Jump: return "Игрок 2: прыжок";
            case ControlAction.Player2Drop: return "Игрок 2: вниз с платформы";
            case ControlAction.Player2LightAttack: return "Игрок 2: удар";
            case ControlAction.Player2HeavyAttack: return "Игрок 2: сильный удар";
            default: return action.ToString();
        }
    }

    public static string GetShortLabel(ControlAction action)
    {
        switch (action)
        {
            case ControlAction.Player1Left:
            case ControlAction.Player2Left:
                return "Влево";
            case ControlAction.Player1Right:
            case ControlAction.Player2Right:
                return "Вправо";
            case ControlAction.Player1Jump:
            case ControlAction.Player2Jump:
                return "Прыжок";
            case ControlAction.Player1Drop:
            case ControlAction.Player2Drop:
                return "Вниз с платформы";
            case ControlAction.Player1LightAttack:
            case ControlAction.Player2LightAttack:
                return "Удар";
            case ControlAction.Player1HeavyAttack:
            case ControlAction.Player2HeavyAttack:
                return "Сильный удар";
            default:
                return action.ToString();
        }
    }

    public static string GetKeyName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftArrow: return "Left";
            case KeyCode.RightArrow: return "Right";
            case KeyCode.UpArrow: return "Up";
            case KeyCode.DownArrow: return "Down";
            case KeyCode.Space: return "Space";
            case KeyCode.Return: return "Enter";
            case KeyCode.Escape: return "Esc";
            default: return key.ToString();
        }
    }

    private static string GetPrefKey(ControlAction action)
    {
        return PrefPrefix + action;
    }
}
