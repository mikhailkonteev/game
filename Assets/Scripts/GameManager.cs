using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool IsGameOver { get; private set; }
    public static bool IsPaused { get; private set; }
    public static bool IsInputBlocked => IsGameOver || IsPaused;

    public Health player1Health;
    public Health player2Health;

    public TextMeshProUGUI player1HPText;
    public TextMeshProUGUI player2HPText;
    public TextMeshProUGUI winnerText;

    void Awake()
    {
        IsGameOver = false;
        IsPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!IsGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            SetPaused(!IsPaused);
        }

        // Update HP labels.
        player1HPText.text = "P1 HP: " + player1Health.currentHealth;
        player2HPText.text = "P2 HP: " + player2Health.currentHealth;

        // Check win condition.
        if (!IsGameOver)
        {
            if (player1Health.currentHealth <= 0)
            {
                IsGameOver = true;
                winnerText.text = "Player 2 Wins! Press R";
            }
            else if (player2Health.currentHealth <= 0)
            {
                IsGameOver = true;
                winnerText.text = "Player 1 Wins! Press R";
            }
        }

        // Restart.
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    void OnGUI()
    {
        if (!IsPaused)
            return;

        float menuWidth = 320f;
        float menuHeight = 230f;
        Rect menuRect = new Rect(
            (Screen.width - menuWidth) * 0.5f,
            (Screen.height - menuHeight) * 0.5f,
            menuWidth,
            menuHeight);

        GUI.Box(menuRect, "PAUSE");

        GUILayout.BeginArea(new Rect(menuRect.x + 40f, menuRect.y + 55f, menuWidth - 80f, menuHeight - 75f));

        if (GUILayout.Button("Continue", GUILayout.Height(42f)))
            SetPaused(false);

        GUILayout.Space(12f);

        if (GUILayout.Button("Restart", GUILayout.Height(42f)))
            RestartScene();

        GUILayout.Space(12f);

        if (GUILayout.Button("Quit", GUILayout.Height(42f)))
            QuitGame();

        GUILayout.EndArea();
    }

    void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
