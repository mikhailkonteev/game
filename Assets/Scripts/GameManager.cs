using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool IsGameOver { get; private set; }

    public Health player1Health;
    public Health player2Health;

    public TextMeshProUGUI player1HPText;
    public TextMeshProUGUI player2HPText;
    public TextMeshProUGUI winnerText;

    void Awake()
    {
        IsGameOver = false;
    }

    void Update()
    {
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}